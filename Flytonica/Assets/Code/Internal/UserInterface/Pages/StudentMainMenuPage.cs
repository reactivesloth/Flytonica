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
            toRoomButton,
            deviceInfoButton,
            selectAvatarButton;

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
            deviceInfoButton.onClick.AddListener(OnDeviceInfo);
            toRoomButton.onClick.AddListener(OnConnect);

            if (HttpClient.IsAuthorized)
                RequestAndSetUserData();
            else
                SetDemo();
        }

        protected override void OnClose()
        {
            base.OnClose();
            singleScriptsButton.onClick.RemoveListener(OnSingleScripts);
            tasksButton.onClick.RemoveListener(OnTaskScripts);
            deviceInfoButton.onClick.RemoveListener(OnDeviceInfo);
            toRoomButton.onClick.RemoveListener(OnConnect);
        }

        private void OnSingleScripts()
        {
            scriptsPage.Init(singleScenariosSettings.scenarios);
            scriptsPage.Open();
        }

        private void OnTaskScripts()
        {
            HttpClient.Get(
                LinkConstants.UserScenarioUrl(HttpClient.UserData.id,
                    new Dictionary<string, string> { { "status", "0" } }),
                response =>
                {
                    taskScenariosSettings.scenarios.Clear();

                    var tasksInfo = JsonUtility.FromJson<MultiAssignedScenarioDataResponse>(response);
                    var loadedTaskCount = 0;

                    foreach (var taskInfo in tasksInfo.data)
                    {
                        HttpClient.Get(LinkConstants.ScenarioGetUrl(taskInfo.scenario_id), taskResponse =>
                            {
                                var taskData = JsonUtility.FromJson<TaskData>(taskResponse);

                                var taskScenarios = new List<ScenarioSettings>(); // Список сценариев в задании

                                var loadedScenarioCount = 0;
                                foreach (var scenario in taskData.mapconfigs.data)
                                {
                                    HttpClient.Get(LinkConstants.GetFile(scenario.file_file_path), scenarioResponse =>
                                    {
                                        var scenarioSettingsData =
                                            JsonUtility.FromJson<ScenarioSettingsData>(scenarioResponse);
                                        var scenarioSetting = ScenarioSettings.Create(scenarioSettingsData.name,
                                            scenarioSettingsData.description, scenarioSettingsData.typeId,
                                            maps.maps[scenarioSettingsData.mapId],
                                            drones.drones[scenarioSettingsData.droneId]);

                                        taskScenarios.Add(scenarioSetting);

                                        loadedScenarioCount++;
                                        if (loadedScenarioCount >= taskData.mapconfigs.total_count)
                                        {
                                            loadedTaskCount++;
                                            OnTaskInit(taskData.scenario.name, taskScenarios);
                                            if(loadedTaskCount >= tasksInfo.total_count)
                                                OnAllTaskInit();
                                        }
                                        
                                    }, error =>
                                    {
                                        Debug.LogError(error);

                                        loadedScenarioCount++;
                                        if (loadedScenarioCount >= taskData.mapconfigs.total_count)
                                        {
                                            loadedTaskCount++;
                                            OnTaskInit(taskData.scenario.name, taskScenarios);
                                            if(loadedTaskCount >= tasksInfo.total_count)
                                                OnAllTaskInit();
                                        }
                                    });
                                }
                                
                            },
                            error =>
                            {
                                Debug.LogError(error);
                            });
                    }
                },
                Debug.LogError);
        }

        private void OnTaskInit(string taskName, List<ScenarioSettings> scenarios)
        {
            var task = ScriptableObject.CreateInstance<ScenarioSettings>();
            task.name = taskName;
            task.nestedScenarios = scenarios;

            print($"Задание {task.name}, Сценариев {task.nestedScenarios.Count}");

            taskScenariosSettings.scenarios.Add(task);
            print(taskScenariosSettings.scenarios.Count);
        }

        private void OnAllTaskInit()
        {
            print(taskScenariosSettings.scenarios.Count);
            scriptsPage.Init(taskScenariosSettings.scenarios);
            scriptsPage.Open();
        }

        private void OnDeviceInfo()
        {
            scriptsPage.Init(taskScenariosSettings.scenarios, true);
            scriptsPage.Open();
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
            if (HttpClient.UserData == null)
                HttpClient.Get(LinkConstants.UserInfoUrl, data =>
                    {
                        HttpClient.SetUserData(JsonUtility.FromJson<UserData>(data));
                        SetData();
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
        }
    }
}