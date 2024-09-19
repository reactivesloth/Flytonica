using System;
using System.Collections.Generic;
using System.Linq;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface.Elements;
using FishNet;
using FishNet.Discovery;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class ScriptsPage : Page
    {
        [Header("Conponents:")] [SerializeField]
        private Transform selectScriptParent;

        [SerializeField] private ScenarioInfoPanel infoPanel;
        [SerializeField] private Button startGameButton;

        [Header("Prefabs:")] [SerializeField] private SelectScriptButton buttonPrefab;
        [SerializeField] private GameObject listPrefab;
        [SerializeField] private SceneLoadingSettings sceneSettings;

        private readonly Dictionary<SelectScriptButton, ScenarioSettings> _buttonScenarioDictionary = new();
        private ScenarioSettings _selectedScenario;
        private bool _isNetGame;

        protected override void OnOpen()
        {
            base.OnOpen();
            startGameButton?.onClick.AddListener(OnStartGame);
        }

        protected override void OnClose()
        {
            base.OnClose();
            startGameButton?.onClick.RemoveListener(OnStartGame);
        }

        public void Init(List<ScenarioSettings> scenarios, bool isNet = false)
        {
            _isNetGame = isNet;
            Clear();

            for (var i = 0; i < scenarios.Count; i++)
            {
                var scenario = scenarios[i];

                var nestedScenarios = scenario.nestedScenarios;
                var openListButton = Instantiate(buttonPrefab, selectScriptParent);
                var list = nestedScenarios is { Length: 0 } ? null : Instantiate(listPrefab, selectScriptParent);
                openListButton.Init(scenario, (i + 1).ToString(), list);
                _buttonScenarioDictionary.Add(openListButton, scenario);
                openListButton.Selected += OnSelect;

                if (!list || nestedScenarios == null)
                    continue;

                for (var j = 0; j < nestedScenarios.Length; j++)
                {
                    var scenario2 = nestedScenarios[j];

                    var scenarioButton = Instantiate(buttonPrefab, list.transform);
                    scenarioButton.Init(scenario2, $"{i + 1}.{j + 1}");
                    _buttonScenarioDictionary.Add(scenarioButton, scenario2);
                    scenarioButton.Selected += OnSelect;
                }
            }

            var select = _buttonScenarioDictionary.Keys.FirstOrDefault();
            OnSelect(select);
            select.OnButtonPress();
        }

        private void Clear()
        {
            foreach (var button in _buttonScenarioDictionary.Keys)
                Destroy(button.gameObject);

            _buttonScenarioDictionary.Clear();
        }

        private void OnStartGame()
        {
                Debug.Log(
                    $"Scenario: {_selectedScenario.name}\n" +
                    $"Map: {infoPanel.CurrentMap.name}\n" +
                    $"Drone:{infoPanel.CurrentDrone.name}\n" +
                    $"Mode:{infoPanel.CurrentFlyMode.name}");

            sceneSettings.currentScenario = _selectedScenario;
            sceneSettings.currentMap = infoPanel.CurrentMap;
            sceneSettings.currentDrone = infoPanel.CurrentDrone;
            sceneSettings.currentDrone.currentFlightMode = infoPanel.CurrentFlyMode;

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
            
            if (_isNetGame)
                InstanceFinder.NetworkManager.GetComponent<NetworkDiscovery>().AdvertiseServer();
        }

        private void OnSelect(SelectScriptButton button)
        {
            print($"select {_buttonScenarioDictionary[button].name}");
            foreach (var b in _buttonScenarioDictionary.Keys.Where(b => b != button))
                b.UnSelected();

            var scenarioInfo = _buttonScenarioDictionary[button];
            if (!scenarioInfo)
                return;

            _selectedScenario = scenarioInfo;
            infoPanel?.Open(_selectedScenario);
        }
    }
}