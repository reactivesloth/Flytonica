using System;
using System.Collections.Generic;
using System.Linq;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface.Elements;
using FishNet;
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

        private bool _isTaskInit;

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

        public void Init(List<ScenarioSettings> scenarios, bool isTask = false)
        {
            _isTaskInit = isTask;
            Clear();

            for (var rootsCounter = 0; rootsCounter < scenarios.Count; rootsCounter++)
            {
                var scenarioRoot = scenarios[rootsCounter];

                var nestedScenarios = scenarioRoot.nestedScenarios;
                var openListButton = Instantiate(buttonPrefab, selectScriptParent);
                var list = nestedScenarios is { Count: 0 } ? null : Instantiate(listPrefab, selectScriptParent);
                openListButton.SetParent(null);
                openListButton.Init(scenarioRoot, (rootsCounter + 1).ToString(), list, _isTaskInit);
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
                    scenarioButton.Init(scenario, $"{rootsCounter + 1}.{nestedCounter + 1}", isTaskInit: _isTaskInit);
                    _buttonScenarioDictionary.Add(scenarioButton, scenario);
                    scenarioButton.Selected += OnSelect;
                    scenarioButton.ToggleChanged += OnToggleChanged;
                }
            }

            var select = _buttonScenarioDictionary.Keys.FirstOrDefault();
            OnSelect(select);
            select?.Select();
        }

        private void Clear()
        {
            foreach (var button in _buttonScenarioDictionary.Keys)
                Destroy(button.gameObject);

            _buttonScenarioDictionary.Clear();
        }

        //TODO: переписать для вариации запуска мульти\одичночный сценарий 
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
                if (args.ConnectionState != LocalConnectionState.Started) return;
                InstanceFinder.ClientManager.StartConnection();
                InstanceFinder.ServerManager.OnServerConnectionState -= callback;
            };
            InstanceFinder.ServerManager.OnServerConnectionState += callback;
        }

        private void StartTask()
        {
            
        }

        private void StartScenariosList()
        {
            
        }

        private void OnSelect(SelectScriptButton button)
        {
            if (!button)
            {   
                infoPanel.Close();
                return;
            }
            
            if (_isTaskInit && button.ParentButton != null)
                return;

            foreach (var b in _buttonScenarioDictionary.Keys)
            {
                if (b != button && b.ParentButton != button && b != button.ParentButton)
                {
                    b.UnSelected();
                }
            }

            button.SelectWithoutNotify();

            var scenarioInfo = _buttonScenarioDictionary[button];
            if (!scenarioInfo)
                return;

            _selectedScenario = scenarioInfo;
            infoPanel?.Open(_selectedScenario);
        }

        private void OnToggleChanged(SelectScriptButton button)
        {
            // Получаем корневую кнопку выбранной кнопки
            var rootButton = button.GetRootButton();

            // Если выбран переключатель родительской кнопки
            if (button.ParentButton == null)
            {
                // Устанавливаем состояние всех дочерних переключателей
                foreach (var child in button.GetAllDescendants())
                {
                    child.SetToggleState(button.ToggleIsOn);
                }
            }

            // Сбрасываем переключатели в других корнях
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
    }
}