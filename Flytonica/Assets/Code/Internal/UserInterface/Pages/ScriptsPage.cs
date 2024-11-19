using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.Internal.API;
using Code.Internal.API.Wrappers;
using Code.Internal.API.Wrappers.ReceiveModels;
using Code.Internal.Scenario;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface.Elements;
using FishNet;
using FishNet.Transporting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class ScriptsPage : Page
    {
        [SerializeField] private AvailableScenariosSettings taskScenariosSettings;
        [SerializeField] private AvailableMapsSettings maps;
        [SerializeField] private AvailableDronesSettings drones;
        [SerializeField] private string tasksTitle = "Доступные задания", learnTitle = "Доступные сценарии";

        [Header("Conponents:")]
        [SerializeField] private Transform selectScriptParent;
        [SerializeField] private TMP_Text pageTitle;

        [SerializeField] private ScenarioInfoPanel infoPanel;
        [SerializeField] private Button startGameButton;

        [SerializeField] private Button updateButton;

        [Header("Prefabs:")] [SerializeField] private SelectScriptButton buttonPrefab;
        [SerializeField] private GameObject listPrefab;
        [SerializeField] private SceneLoadingSettings sceneSettings;
        [SerializeField] private ScenarioSettings currentScenarioCollection;

        private readonly Dictionary<SelectScriptButton, ScenarioSettings> _buttonScenarioDictionary = new();
        private ScenarioSettings _selectedScenario;

        private bool _isTaskInit;

        protected override void OnOpen()
        {
            base.OnOpen();
            
            startGameButton?.onClick.AddListener(OnStartGame);
            
            updateButton?.onClick.AddListener(InitTasks);
            
            infoPanel?.Close();
        }

        protected override void OnClose()
        {
            base.OnClose();
            
            startGameButton?.onClick.RemoveListener(OnStartGame);
            
            updateButton?.onClick.RemoveListener(InitTasks);
        }

        public void InitTasks()
        {
            Clear();
            GetScenarios(() =>
            {
                Init(taskScenariosSettings.scenarios, true);
            });
        }

        public void Init(List<ScenarioSettings> scenarios, bool isTask = false)
        {
            _isTaskInit = isTask;
            
            updateButton?.gameObject.SetActive(_isTaskInit);
            pageTitle?.SetText(isTask ? tasksTitle : learnTitle);
            
            Clear();

            for (var rootsCounter = 0; rootsCounter < scenarios.Count; rootsCounter++)
            {
                var scenarioRoot = scenarios[rootsCounter];

                var nestedScenarios = scenarioRoot.nestedScenarios;
                var openListButton = Instantiate(buttonPrefab, selectScriptParent);
                var list = nestedScenarios is { Count: 0 } ? null : Instantiate(listPrefab, selectScriptParent);
                openListButton.SetParent(null);
                openListButton.Init(scenarioRoot, (rootsCounter + 1).ToString(), list, _isTaskInit,
                    list != null && !_isTaskInit);
                _buttonScenarioDictionary.Add(openListButton, scenarioRoot);
                openListButton.Selected += OnSelect;
                openListButton.ToggleChanged += OnToggleChanged;

                if (!list || nestedScenarios == null)
                    continue;

                for (var nestedCounter = 0; nestedCounter < nestedScenarios.Count; nestedCounter++)
                {
                    var scenario = nestedScenarios[nestedCounter];

                    var scenarioButton = Instantiate(buttonPrefab, list.transform);
                    scenarioButton.SetParent(openListButton);
                    scenarioButton.Init(scenario, $"{rootsCounter + 1}.{nestedCounter + 1}", isTaskInit: _isTaskInit,
                        isOnToggle: !_isTaskInit);
                    _buttonScenarioDictionary.Add(scenarioButton, scenario);
                    scenarioButton.Selected += OnSelect;
                    scenarioButton.ToggleChanged += OnToggleChanged;
                }
            }
        }

        private void Clear()
        {
            foreach (var button in _buttonScenarioDictionary.Keys)
                Destroy(button.gameObject);

            _buttonScenarioDictionary.Clear();
        }

        private void OnStartGame()
        {
            currentScenarioCollection = _isTaskInit ? GetTask() : GetScenarioList();

            sceneSettings.currentScenarioCollection = currentScenarioCollection;
            sceneSettings.isNet = false;
            sceneSettings.isTask = _isTaskInit;
            sceneSettings.taskId = _isTaskInit ? currentScenarioCollection.id : -1;
            if(sceneSettings.currentScenarioCollection.nestedScenarios?.Count <=0)
                return;
            sceneSettings.currentScenario = sceneSettings.currentScenarioCollection.nestedScenarios[0];
            
            Action<ServerConnectionStateArgs> callback = null;
            callback = args =>
            {
                if (args.ConnectionState != LocalConnectionState.Started) return;
                InstanceFinder.ClientManager.StartConnection();
                ScenarioSwitcherController.Instance.StartTask();
                InstanceFinder.ServerManager.OnServerConnectionState -= callback;
            };
            InstanceFinder.ServerManager.OnServerConnectionState += callback;
            
            InstanceFinder.ServerManager.StartConnection();
        }

        private ScenarioSettings GetTask()
        {
            //Init next
            if (_selectedScenario.nestedScenarios == null) return _selectedScenario;
            for (var i = 0; i < _selectedScenario.nestedScenarios.Count - 1; i++)
                _selectedScenario.nestedScenarios[i].nextScenario = _selectedScenario.nestedScenarios[i + 1];
            return _selectedScenario;
        }

        private ScenarioSettings GetScenarioList()
        {
            var selectedScenarios =
                _buttonScenarioDictionary.Where(s => s.Key.ToggleIsOn && s.Value.settingType == SettingType.Scenario)
                    .Select(s => s.Value).ToList();

            //Init next
            for (var i = 0; i < selectedScenarios.Count - 1; i++)
                selectedScenarios[i].nextScenario = selectedScenarios[i + 1];

            var scenarioList = ScriptableObject.CreateInstance<ScenarioSettings>();
            scenarioList.settingType = SettingType.List;
            scenarioList.nestedScenarios = selectedScenarios;
            return scenarioList;
        }
        
        private void OnSelect(SelectScriptButton button)
        {
            if (!button)
            {
                infoPanel.Close();
                return;
            }

            var scenarioInfo = _buttonScenarioDictionary[button];
            if (!scenarioInfo)
                return;

            foreach (var b in _buttonScenarioDictionary.Keys)
            {
                if (b != button && b.ParentButton != button && b != button.ParentButton)
                {
                    b.UnSelected();
                }
            }
            
            button.SelectWithoutNotify();
            
            if (_isTaskInit && button.ParentButton != null)
            {
                // Open infoPanel without modifying _selectedScenario
                infoPanel?.Open(scenarioInfo);
                return;
            }

            

            _selectedScenario = scenarioInfo;
            infoPanel?.Open(_selectedScenario);
        }


        private void OnToggleChanged(SelectScriptButton button)
        {
            var rootButton = button.GetRootButton();

            if (button.ParentButton == null)
            {
                foreach (var child in button.GetAllDescendants())
                {
                    child.SetToggleState(button.ToggleIsOn);
                }
            }

            foreach (var b in _buttonScenarioDictionary.Keys)
            {
                if (b.GetRootButton() != rootButton)
                {
                    b.SetToggleState(false);
                    foreach (var child in b.GetAllDescendants())
                    {
                        child.SetToggleState(false);
                    }
                }
            }
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
                onError: (error, code) =>
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
                onError: (error, code) =>
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
                task.description = taskData.scenario.description;
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
                onError: (error, code) =>
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
                    drones.drones[scenarioSettingsData.droneId].flightModes[scenarioSettingsData.droneModeId],
                    scenarioSettingsData.cameraThirdPerson,
                    scenarioSettingsData.cameraAllowedSwitchModeId,
                    scenarioSettingsData.objects,
                    scenarioSettingsData.windLayers);

                taskScenarios.Add(scenarioSetting);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        #endregion
    }
}