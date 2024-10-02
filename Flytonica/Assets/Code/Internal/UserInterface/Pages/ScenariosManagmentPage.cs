using System;
using System.Collections;
using System.Collections.Generic;
using Code.Internal.API;
using Code.Internal.API.Wrappers;
using Code.Internal.API.Wrappers.ReceiveModels;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface.Elements.TableElements;
using FishNet;
using FishNet.Discovery;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class ScenariosManagmentPage : Page
    {
        [SerializeField] private AvailableScenariosSettings taskScenariosSettings;
        [SerializeField] private AvailableMapsSettings maps;
        [SerializeField] private AvailableDronesSettings drones;
        [SerializeField] private SceneLoadingSettings sceneSettings;

        [SerializeField] private SelectionCollectionManager scenariosRoot;
        [SerializeField] private Button createButton, deleteButton, startButton;
        [SerializeField] private Page createScenarioPage;
        [SerializeField] private Page menuPage;

        private bool _isEditMode;

        public void Init(bool isEdit)
        {
            _isEditMode = isEdit;

            createButton?.gameObject.SetActive(isEdit);
            deleteButton?.gameObject.SetActive(isEdit);

            startButton?.gameObject.SetActive(!isEdit);
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            LoadScenariosList();

            if (_isEditMode)
            {
                createButton?.onClick.AddListener(OnCreate);
                deleteButton?.onClick.AddListener(OnDelete);
                deleteButton?.gameObject.SetActive(scenariosRoot.SelectedButton);

                if (deleteButton != null) scenariosRoot.SelectionStateChange += deleteButton.gameObject.SetActive;
            }
            else
            {
                startButton?.onClick.AddListener(OnStart);

                startButton?.gameObject.SetActive(scenariosRoot.SelectedButton);
                if (startButton != null) scenariosRoot.SelectionStateChange += startButton.gameObject.SetActive;
            }
        }

        protected override void OnClose()
        {
            base.OnClose();

            createButton?.onClick.RemoveListener(OnCreate);
            deleteButton?.onClick.RemoveListener(OnDelete);
            startButton.onClick.RemoveListener(OnStart);

            scenariosRoot.SelectionStateChange -= deleteButton.gameObject.SetActive;
            scenariosRoot.SelectionStateChange -= startButton.gameObject.SetActive;
        }

        protected override void OnBackClick()
        {
            menuPage?.Open();
        }

        private void OnCreate()
        {
            createScenarioPage?.Open();
        }

        private void OnDelete()
        {
            PopupPanel.ShowDeleteTemplate(
                Delete, "сценарий", scenariosRoot.SelectedButton.GetSaveData<ScenarioSettings>().name);
        }

        private void Delete()
        {
            print(LinkConstants.MapConfigDeleteUrl(scenariosRoot.SelectedButton.GetSaveData<ScenarioSettings>()
                .id));
            HttpClient.Delete(
                LinkConstants.MapConfigDeleteUrl(scenariosRoot.SelectedButton.GetSaveData<ScenarioSettings>()
                    .id),
                _ =>
                {
                    taskScenariosSettings.scenarios.Remove(scenariosRoot.SelectedButton
                        .GetSaveData<ScenarioSettings>());
                    InitViewList();
                }, (error, code) => Debug.LogError(error));
        }

        private void OnStart()
        {
            var scenario = scenariosRoot.SelectedButton.GetSaveData<ScenarioSettings>();

            var scenarioCollection = ScriptableObject.CreateInstance<ScenarioSettings>();
            scenarioCollection.nestedScenarios = new List<ScenarioSettings> { scenario };
            sceneSettings.isNet = true;
            sceneSettings.currentScenarioCollection = scenarioCollection;
            sceneSettings.currentScenario = sceneSettings.currentScenarioCollection.nestedScenarios[0];

            InstanceFinder.ServerManager.StartConnection();

            Action<ServerConnectionStateArgs> callback = null;
            callback = args =>
            {
                if (args.ConnectionState == LocalConnectionState.Started)
                {
                    InstanceFinder.ClientManager.StartConnection();
                    InstanceFinder.ServerManager.OnServerConnectionState -= callback;
                }
            };
            InstanceFinder.ServerManager.OnServerConnectionState += callback;

            InstanceFinder.NetworkManager.GetComponent<NetworkDiscovery>().AdvertiseServer();
        }

        /// <summary>
        /// Загрузка сценариев, запаковка в ScenarioSettings и добавление в taskScenariosSettings
        /// </summary>
        private void LoadScenariosList()
        {
            StartCoroutine(LoadScenariosListCoroutine());
        }

        private IEnumerator LoadScenariosListCoroutine()
        {
            taskScenariosSettings.scenarios.Clear();

            // Build the URL for the initial request
            string url = LinkConstants.MapConfigMultiUrl(new Dictionary<string, string>
                { { "page", "1" }, { "itemsPerPage", "9999" } });

            string response = null;
            bool requestCompleted = false;

            // Make the initial request to get the list of scenarios
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

            // Wait for the initial request to complete
            while (!requestCompleted)
                yield return null;

            if (string.IsNullOrEmpty(response))
                yield break;

            var scenarios = JsonUtility.FromJson<MultiScenarioDataResponse>(response);
            var scenariosData = scenarios.data;
            var totalScenarios = scenariosData.Count;

            // Create a list with fixed size to hold ScenarioSettings
            var scenarioSettingsList = new List<ScenarioSettings>(new ScenarioSettings[totalScenarios]);

            // Iterate over the scenarios and process each one sequentially to maintain order
            for (int index = 0; index < totalScenarios; index++)
            {
                var scenarioData = scenariosData[index];
                yield return StartCoroutine(ProcessScenario(scenarioData, index, scenarioSettingsList));
            }

            // Assign the ordered list to taskScenariosSettings.scenarios
            taskScenariosSettings.scenarios = scenarioSettingsList;

            // Now initialize the view list
            InitViewList();
        }

        private IEnumerator ProcessScenario(ScenarioData scenarioData, int index, List<ScenarioSettings> scenarioSettingsList)
        {
            string scenarioUrl = LinkConstants.GetFile(scenarioData.file_file_path);

            string scenarioResponse = null;
            bool scenarioRequestCompleted = false;

            // Make the request to get scenario file data
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

            // Wait for the scenario file request to complete
            while (!scenarioRequestCompleted)
                yield return null;

            if (string.IsNullOrEmpty(scenarioResponse))
                yield break;

            var settings = JsonUtility.FromJson<ScenarioSettingsData>(scenarioResponse);
            print($"{index}.{settings.name}");

            try
            {
                var scenarioSetting = ScenarioSettings.CreateDynamicTaskScenario(scenarioData.id,
                    settings.name,
                    settings.description, settings.typeId,
                    maps.maps[settings.mapId], drones.drones[settings.droneId],
                    drones.drones[settings.droneId].flightModes[settings.droneModeId], settings.cameraThirdPerson, settings.cameraAllowedSwitchModeId, settings.objects, settings.windLayers);

                // Store in the correct index to preserve order
                scenarioSettingsList[index] = scenarioSetting;
            }
            catch (ArgumentOutOfRangeException e)
            {
                Debug.LogError(e);
            }
        }

        private void InitViewList()
        {
            print("INIT");
            var scenarios = taskScenariosSettings.scenarios;
            var generateData = new List<TableButtonGenerateData<ScenarioSettings>>();

            foreach (var scenarioSettings in scenarios)
            {
                if(!scenarioSettings) continue;
                
                var display = new[]
                {
                    scenarioSettings.name, scenarioSettings.currentMap.name, scenarioSettings.scenarioType.GetName(),
                    scenarioSettings.currentDrone.name, scenarioSettings.currentDroneMode.modeName
                };
                var data = new TableButtonGenerateData<ScenarioSettings>(display, scenarioSettings);
                generateData.Add(data);
            }

            scenariosRoot.Generate(generateData);
        }
    }
}
