using System;
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

        protected override void OnBackClick ()
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
                }, Debug.LogError);
        }

        private void OnStart()
        {
            var scenario = scenariosRoot.SelectedButton.GetSaveData<ScenarioSettings>();

            sceneSettings.currentScenario = scenario;
            sceneSettings.currentMap = scenario.currentMap;
            sceneSettings.currentDrone = scenario.currentDrone;
            sceneSettings.currentDrone.currentFlightMode = scenario.currentDroneMode;

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
            HttpClient.Get(
                LinkConstants.MapConfigMultiUrl(new Dictionary<string, string>
                    { { "page", "1" }, { "itemsPerPage", "9999" } }), response =>
                {
                    taskScenariosSettings.scenarios.Clear();
                    var scenarios = JsonUtility.FromJson<MultiScenarioDataResponse>(response);
                    var loadedScenariosCount = 0;

                    foreach (var scenarioData in scenarios.data)
                    {
                        HttpClient.Get(LinkConstants.GetFile(scenarioData.file_file_path), settingsJson =>
                            {
                                var settings = JsonUtility.FromJson<ScenarioSettingsData>(settingsJson);
                                print($"{loadedScenariosCount}.{settings.name}");
                                var scenarioSetting = ScenarioSettings.CreateDynamicTaskScenario(scenarioData.id, settings.name,
                                    settings.description, settings.typeId,
                                    maps.maps[settings.mapId], drones.drones[settings.droneId],
                                    drones.drones[settings.droneId].flightModes[settings.droneModeId]);

                                taskScenariosSettings.scenarios.Add(scenarioSetting);

                                loadedScenariosCount++;
                                if (loadedScenariosCount >= scenarios.data.Count)
                                    InitViewList();
                            },
                            error =>
                            {
                                Debug.LogError(error);
                                loadedScenariosCount++;

                                if (loadedScenariosCount >= scenarios.data.Count)
                                    InitViewList();
                            });
                    }
                }, Debug.LogError);
        }

        private void InitViewList()
        {
            print("INIT");
            var scenarios = taskScenariosSettings.scenarios;
            var generateData = new List<TableButtonGenerateData<ScenarioSettings>>();

            foreach (var scenarioSettings in scenarios)
            {
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