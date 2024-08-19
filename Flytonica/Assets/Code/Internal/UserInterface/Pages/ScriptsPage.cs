using System.Collections.Generic;
using System.Linq;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface.Elements;
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

        private readonly Dictionary<SelectScriptButton, ScenarioSettings> _buttonScenarioDictionary = new ();
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

        public void Init(IEnumerable<ScenarioSettings> scenarios)
        {
            Clear();
            
            foreach (var scenario in scenarios)
            {
                var nestedScenarios = scenario.nestedScenarios;
                var openListButton = Instantiate(buttonPrefab, selectScriptParent);
                var list = nestedScenarios is { Length: 0 } ? null : Instantiate(listPrefab, selectScriptParent);
                openListButton.Init(scenario, list);
                _buttonScenarioDictionary.Add(openListButton, scenario);
                openListButton.Selected += OnSelect;

                if (!list || nestedScenarios == null)
                    continue;
                
                foreach (var scenario2 in nestedScenarios)
                {
                    var scenarioButton = Instantiate(buttonPrefab, list.transform);
                    scenarioButton.Init(scenario2);
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
                $"{_selectedScenario.name} - {infoPanel.CurrentMap.name} {infoPanel.CurrentDrone.name} {infoPanel.CurrentFlyMode.name}");
        }

        private void OnSelect(SelectScriptButton button)
        {
            print($"select {_buttonScenarioDictionary[button].name}");
            foreach (var b in _buttonScenarioDictionary.Keys.Where(b => b != button))
                b.UnSelected();
            
            var scenarioInfo = _buttonScenarioDictionary[button];
            if(!scenarioInfo)
                return;

            _selectedScenario = scenarioInfo;
            infoPanel?.Open(_selectedScenario);
        }
    }
}