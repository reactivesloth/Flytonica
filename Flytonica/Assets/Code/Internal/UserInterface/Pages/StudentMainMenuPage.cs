using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Code.Internal.API;
using Code.Internal.API.Wrappers;
using Code.Internal.API.Wrappers.ReceiveModels;
using Code.Internal.SceneManagement;
using FishNet;
using FishNet.Discovery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class StudentMainMenuPage : Page
    {
        [SerializeField] private TMP_Text studentNameText;

        [SerializeField] private Button tasksButton,
            singleScriptsButton,
            toRoomButton;

        [SerializeField] private ScriptsPage scriptsPage;
        [SerializeField] private AvailableScenariosSettings singleScenariosSettings;
        [SerializeField] private AvailableScenariosSettings taskScenariosSettings;
        [SerializeField] private AvailableMapsSettings maps;
        [SerializeField] private AvailableDronesSettings drones;

        private List<IPEndPoint> _points = new();
        private IPEndPoint _currentIPEndPoint => _points.LastOrDefault();
        private NetworkDiscovery _discovery => InstanceFinder.NetworkManager.GetComponent<NetworkDiscovery>();

        private void OnDisable()
        {
            _discovery.StopSearchingOrAdvertising();
            _discovery.ServerFoundCallback -= NetworkDiscoveryOnServerFoundCallback;
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            toRoomButton.interactable = _currentIPEndPoint != null;
            _discovery.ServerFoundCallback += NetworkDiscoveryOnServerFoundCallback;
            _discovery.SearchForServers();

            singleScriptsButton.onClick.AddListener(OnSingleScripts);
            tasksButton.onClick.AddListener(OnTaskScripts);
            toRoomButton.onClick.AddListener(OnConnect);

            if (HttpClient.IsAuthorized)
            {
                RequestAndSetUserData();
            }
            else
                SetDemo();
        }

        protected override void OnClose()
        {
            base.OnClose();
            singleScriptsButton.onClick.RemoveListener(OnSingleScripts);
            tasksButton.onClick.RemoveListener(OnTaskScripts);
            toRoomButton.onClick.RemoveListener(OnConnect);
        }


        // Executes the logout function
        protected override void OnBackClick()
        {
            HttpClient.Logout();
            base.OnBackClick();
        }

        #region Tasks Get

        private void GetScenarios(Action onComplete = null)
        {
            StartCoroutine(GetScenariosCoroutine(onComplete));
        }

        private IEnumerator GetScenariosCoroutine(Action onComplete)
        {
            taskScenariosSettings.scenarios.Clear();

            string url = LinkConstants.UserScenarioUrl(HttpClient.UserData.id,
                new Dictionary<string, string> { { "status", "0" }, { "limit", "9999" }, { "page", "1" } });

            string response = null;
            bool requestCompleted = false;

            HttpClient.Get(url,
                onSuccess: data =>
                {
                    response = data;
                    requestCompleted = true;
                },
                onError: error =>
                {
                    Debug.LogError(error);
                    requestCompleted = true;
                });

            while (!requestCompleted)
                yield return null;

            if (string.IsNullOrEmpty(response))
            {
                onComplete?.Invoke();
                yield break;
            }

            var tasksInfo = JsonUtility.FromJson<MultiAssignedScenarioDataResponse>(response);
            var tasksData = tasksInfo.data;

            var taskScenariosList = new List<ScenarioSettings>();

            foreach (var taskInfo in tasksData)
            {
                yield return StartCoroutine(ProcessTask(taskInfo, taskScenariosList));
            }

            taskScenariosSettings.scenarios = taskScenariosList;
            tasksButton.interactable = true;

            onComplete?.Invoke();
        }

        private IEnumerator ProcessTask(AssignedScenarioData taskInfo, List<ScenarioSettings> taskScenariosList)
        {
            string taskUrl = LinkConstants.ScenarioGetUrl(taskInfo.scenario_id);

            string taskResponse = null;
            bool taskRequestCompleted = false;

            HttpClient.Get(taskUrl,
                onSuccess: data =>
                {
                    taskResponse = data;
                    taskRequestCompleted = true;
                },
                onError: error =>
                {
                    Debug.LogError(error);
                    taskRequestCompleted = true;
                });

            while (!taskRequestCompleted)
                yield return null;

            if (string.IsNullOrEmpty(taskResponse))
                yield break;

            var taskData = JsonUtility.FromJson<TaskData>(taskResponse);
            var scenariosData = taskData.mapconfigs.data;

            var taskScenarios = new List<ScenarioSettings>();

            foreach (var scenario in scenariosData)
            {
                yield return StartCoroutine(ProcessScenario(scenario, taskInfo.id, taskScenarios));
            }

            // Check if the task has any valid scenarios
            if (taskScenarios.Count > 0)
            {
                var task = ScriptableObject.CreateInstance<ScenarioSettings>();
                task.settingType = SettingType.Task;
                task.scenarioType = ScenarioType.Searching; // Adjust as necessary
                task.name = taskData.scenario.name;
                task.nestedScenarios = taskScenarios;
                task.id = taskInfo.id;

                taskScenariosList.Add(task);
            }
            else
            {
                Debug.Log($"Task '{taskData.scenario.name}' has no nested scenarios and will be skipped.");
            }
        }

        private IEnumerator ProcessScenario(ScenarioData scenario, int taskId, List<ScenarioSettings> taskScenarios)
        {
            string scenarioUrl = LinkConstants.GetFile(scenario.file_file_path);

            string scenarioResponse = null;
            bool scenarioRequestCompleted = false;

            HttpClient.Get(scenarioUrl,
                onSuccess: data =>
                {
                    scenarioResponse = data;
                    scenarioRequestCompleted = true;
                },
                onError: error =>
                {
                    Debug.LogError(error);
                    scenarioRequestCompleted = true;
                });

            while (!scenarioRequestCompleted)
                yield return null;

            if (string.IsNullOrEmpty(scenarioResponse))
                yield break;

            var scenarioSettingsData = JsonUtility.FromJson<ScenarioSettingsData>(scenarioResponse);

            try
            {
                var scenarioSetting = ScenarioSettings.CreateDynamicTaskScenario(
                    taskId,
                    scenarioSettingsData.name,
                    scenarioSettingsData.description,
                    scenarioSettingsData.typeId,
                    maps.maps[scenarioSettingsData.mapId],
                    drones.drones[scenarioSettingsData.droneId],
                    drones.drones[scenarioSettingsData.droneId].flightModes[scenarioSettingsData.droneModeId]);

                taskScenarios.Add(scenarioSetting);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        #endregion

        private void OnSingleScripts()
        {
            scriptsPage.Init(singleScenariosSettings.scenarios);
            scriptsPage.Open();
        }

        private void OnTaskScripts()
        {
            GetScenarios(() =>
            {
                scriptsPage.Init(taskScenariosSettings.scenarios, true);
                scriptsPage.Open();
            });
        }

        private void OnConnect()
        {
            InstanceFinder.ClientManager.StartConnection(_currentIPEndPoint.Address.ToString());
        }

        private void NetworkDiscoveryOnServerFoundCallback(IPEndPoint obj)
        {
            if (!_points.Contains(obj))
                _points.Add(obj);
            toRoomButton.interactable = _currentIPEndPoint != null;
        }

        private void RequestAndSetUserData()
        {
            tasksButton.interactable = false;
            
            if (HttpClient.UserData == null)
                HttpClient.Get(LinkConstants.UserInfoUrl, data =>
                    {
                        HttpClient.SetUserData(JsonUtility.FromJson<UserData>(data));
                        SetData();
                        // GetScenarios(); // No longer needed here
                    },
                    Debug.LogError);
            else
                SetData();
        }

        private void SetData()
        {
            var data = HttpClient.UserData;
            print(data.name);
            studentNameText.text = data.name;
        }

        private void SetDemo()
        {
            tasksButton.interactable = false;
            HttpClient.SetUserData(new UserData { name = "Гость", type = UserType.Guest});
            SetData();
        }
    }
}
