using System.Collections.Generic;
using System.Linq;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface.Elements;
using FishNet;
using FishNet.Discovery;
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

        public void Init(ScenarioSettings[] scenarios)
        {
            Clear();

            for (var i = 0; i < scenarios.Length; i++)
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
        }

        private void Clear()
        {
            foreach (var button in _buttonScenarioDictionary.Keys)
                Destroy(button.gameObject);

            _buttonScenarioDictionary.Clear();
        }

        private void OnStartGame()
        {
            print(
                $"Scenario: {_selectedScenario.name}\n" +
                $"Map: {infoPanel.CurrentMap.name}\n" +
                $"Drone:{infoPanel.CurrentDrone.name}\n" +
                $"Mode:{infoPanel.CurrentFlyMode.name}");

            sceneSettings.currentScenario = _selectedScenario;
            sceneSettings.currentMap = infoPanel.CurrentMap;
            sceneSettings.currentDrone = infoPanel.CurrentDrone;

            InstanceFinder.ClientManager.OnConnectedClients += _ => GameSceneManager.Instance.LoadGame();
            InstanceFinder.ServerManager.StartConnection();
            InstanceFinder.ClientManager.StartConnection();
            InstanceFinder.NetworkManager.GetComponent<NetworkDiscovery>().enabled = false;
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