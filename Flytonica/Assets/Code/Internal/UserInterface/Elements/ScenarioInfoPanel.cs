using System.Collections.Generic;
using System.Linq;
using Code.Internal.Drone;
using Code.Internal.SceneManagement;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace Code.Internal.UserInterface.Elements
{
    public class ScenarioInfoPanel : MonoBehaviour
    {
        [SerializeField] [CanBeNull] private string lableTask = "Задание", lableScenario = "Тип сценария";
        [SerializeField] private TMP_Text typeLableText;
        [SerializeField] private TMP_Text typeText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Dropdown locationDropdown, droneDropdown, flyModeDropdown;
        [SerializeField] private List<DroneSettings> drones;

        private ScenarioSettings _currentScenarioSettings;

        private readonly Dictionary<int, MapSettings> _dropdownLocations = new();
        private Dictionary<int, DroneSettings> _dropdownDrones = new();
        private Dictionary<int, DroneFlightSettings> _dropdownFlyModes = new();

        public DroneSettings CurrentDrone => _dropdownDrones[droneDropdown.value];

        public void Open(ScenarioSettings scenarioSettings)
        {
            locationDropdown.onValueChanged.RemoveAllListeners();
            droneDropdown.onValueChanged.RemoveAllListeners();
            flyModeDropdown.onValueChanged.RemoveAllListeners();

            locationDropdown.onValueChanged.AddListener(OnMapDropdownChange);
            droneDropdown.onValueChanged.AddListener(OnDroneDropdownChange);
            flyModeDropdown.onValueChanged.AddListener(OnModeDropdownChange);

            var isTask = scenarioSettings.settingType == SettingType.Task;
            typeLableText?.SetText(isTask ? lableTask : lableScenario);
            typeText.text = isTask ? string.Empty : scenarioSettings.scenarioType.GetName();
            descriptionText.text = scenarioSettings.description;

            gameObject.SetActive(true);
            Init(scenarioSettings);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void Init(ScenarioSettings scenarioSettings)
        {
            _currentScenarioSettings = scenarioSettings;

            InitMapsDropdown(scenarioSettings);
            InitDronesDropDown(scenarioSettings);

            if (scenarioSettings.settingType != SettingType.Task && scenarioSettings.settingType != SettingType.List)
            {
                _currentScenarioSettings.currentMap = _dropdownLocations[locationDropdown.value];
                _currentScenarioSettings.currentDrone = _dropdownDrones[droneDropdown.value];
                _currentScenarioSettings.currentDroneMode = _dropdownFlyModes[flyModeDropdown.value];
            }
        }

        private void InitMapsDropdown(ScenarioSettings scenarioSettings)
        {
            locationDropdown.ClearOptions();
            _dropdownLocations.Clear();
            var locationOptionData = new List<string>();

            if (scenarioSettings.currentMap && scenarioSettings.settingType == SettingType.TaskScenario)
            {
                _dropdownLocations.Add(0, scenarioSettings.currentMap);
                locationOptionData.Add(scenarioSettings.currentMap.name);
            }
            else if (scenarioSettings.availableMaps != null)
            {
                for (var i = 0; i < scenarioSettings.availableMaps.Length; i++)
                {
                    var scenarioSettingsAvailableMap = scenarioSettings.availableMaps[i];
                    _dropdownLocations.Add(i, scenarioSettingsAvailableMap);
                    locationOptionData.Add(scenarioSettingsAvailableMap.name);
                }
            }

            locationDropdown.AddOptions(locationOptionData);
            locationDropdown.interactable = locationDropdown.options.Count > 1;
            locationDropdown.gameObject.SetActive(locationDropdown.options.Count > 0);

            if (scenarioSettings.currentMap)
                locationDropdown.value =
                    _dropdownLocations.FirstOrDefault(l => l.Value == scenarioSettings.currentMap).Key;
        }

        private void InitDronesDropDown(ScenarioSettings scenarioSettings)
        {
            droneDropdown.ClearOptions();
            _dropdownDrones.Clear();
            var droneOptionData = new List<string>();

            if (scenarioSettings.currentDrone && scenarioSettings.settingType == SettingType.TaskScenario)
            {
                _dropdownDrones.Add(0, scenarioSettings.currentDrone);
                droneOptionData.Add(scenarioSettings.currentDrone.modelName);
            }
            else if (scenarioSettings.nestedScenarios == null || scenarioSettings.nestedScenarios.Count == 0)
            {
                for (var i = 0; i < drones.Count; i++)
                {
                    var droneSettings = drones[i];
                    _dropdownDrones.Add(i, droneSettings);
                    droneOptionData.Add(droneSettings.modelName);
                }
            }

            droneDropdown.AddOptions(droneOptionData);
            droneDropdown.interactable = droneDropdown.options.Count > 1;
            droneDropdown.gameObject.SetActive(droneDropdown.options.Count > 0);

            if (scenarioSettings.currentDrone && scenarioSettings.settingType == SettingType.TaskScenario)
                droneDropdown.value =
                    _dropdownDrones.FirstOrDefault(d => d.Value == scenarioSettings.currentDrone).Key;
            else
                InitModesDropdown(scenarioSettings);
        }

        private void InitModesDropdown(ScenarioSettings scenarioSettings)
        {
            flyModeDropdown.ClearOptions();
            _dropdownFlyModes.Clear();

            var droneOptionData = new List<string>();
            if (scenarioSettings.currentDroneMode && scenarioSettings.settingType == SettingType.TaskScenario)
            {
                _dropdownFlyModes.Add(0, scenarioSettings.currentDroneMode);
                droneOptionData.Add(scenarioSettings.currentDroneMode.modeName);
            }
            else if (scenarioSettings.nestedScenarios == null || scenarioSettings.nestedScenarios.Count == 0)
            {
                for (var i = 0; i < CurrentDrone.flightModes.Count; i++)
                {
                    var droneMode = CurrentDrone.flightModes[i];
                    _dropdownFlyModes.Add(i, droneMode);
                    droneOptionData.Add(droneMode.modeName);
                }
            }

            flyModeDropdown.AddOptions(droneOptionData);
            flyModeDropdown.interactable = flyModeDropdown.options.Count > 1;
            flyModeDropdown.gameObject.SetActive(flyModeDropdown.options.Count > 0);

            if (scenarioSettings.currentDroneMode)
                flyModeDropdown.value =
                    _dropdownFlyModes.FirstOrDefault(m => m.Value == scenarioSettings.currentDroneMode).Key;
        }

        private void OnMapDropdownChange(int value)
        {
            _currentScenarioSettings.currentMap = _dropdownLocations[value];
        }

        private void OnDroneDropdownChange(int value)
        {
            _currentScenarioSettings.currentDrone = _dropdownDrones[value];
            InitModesDropdown(_currentScenarioSettings);
            OnModeDropdownChange(0);
        }

        private void OnModeDropdownChange(int value)
        {
            _currentScenarioSettings.currentDroneMode = _dropdownFlyModes[value];
        }
    }
}