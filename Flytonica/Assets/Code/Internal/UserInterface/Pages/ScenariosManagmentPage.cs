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

            if (!_isEditMode)
                InitViewList();
            InitScenariosList();

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

        private void OnCreate()
        {
            createScenarioPage?.Open();
        }

        private void OnDelete()
        {
            var popup = FindObjectOfType<PopupPanel>(true);
            popup.SetTitle("Удалить сценарий?");
            popup.SetDescription(
                $"Вы уверены, что хотите удалить сценарий {scenariosRoot.SelectedButton.GetSaveData<ScenarioData>().name}? Его нельзя будет восстановить.");
            popup.SetLeftButton(() => Debug.Log("Удалить"), "Удалить", null, Color.red, Color.white);
            popup.SetRightButton(popup.Hide, "Отменить");
            popup.Show();
        }

        private void OnStart()
        {
            //TODO: Start Game Logic
            var scenario = scenariosRoot.SelectedButton.GetSaveData<ScenarioSettings>();
            
            sceneSettings.currentScenario = scenario;
            sceneSettings.currentMap = scenario.currentMap;
            sceneSettings.currentDrone = scenario.currentDrone;
            sceneSettings.currentDrone.currentFlightMode = scenario.currentDrone.currentFlightMode;

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
        private void InitScenariosList()
        {
            HttpClient.Get(LinkConstants.MapConfigMultiUrl(), response =>
            {
                taskScenariosSettings.scenarios.Clear();
                scenariosRoot.Clear();
                var scenarios = JsonUtility.FromJson<MultiScenarioDataResponse>(response);
                var loadedScenariosCount = 0;

                print(scenarios.total_count);
                foreach (var scenarioData in scenarios.data)
                {
                    HttpClient.Get(LinkConstants.GetFile(scenarioData.file_file_path), settingsText =>
                    {
                        loadedScenariosCount++;
                        var settings = JsonUtility.FromJson<ScenarioSettingsData>(settingsText);
                        var scenarioSetting = ScenarioSettings.Create(scenarioData.name, "", settings.typeId,
                            maps.maps[settings.mapId], drones.drones[settings.droneId]);
                        
                        taskScenariosSettings.scenarios.Add(scenarioSetting);

                        if (loadedScenariosCount >= scenarios.total_count)
                            InitViewList();
                    },
                    error =>
                    {
                        Debug.LogError(error);
                        loadedScenariosCount++;
                        
                        if (loadedScenariosCount >= scenarios.total_count)
                            InitViewList();
                    });
                }
            }, Debug.LogError);
        }

        private void InitViewList()
        {
            var scenarios = taskScenariosSettings.scenarios;
            var generateData = new List<TableButtonGenerateData<ScenarioSettings>>();

            foreach (var scenarioData in scenarios)
            {
                var display = new[] { scenarioData.name };
                var data = new TableButtonGenerateData<ScenarioSettings>(display, scenarioData);
                generateData.Add(data);
            }

            scenariosRoot.Generate(generateData);
        }
    }
}