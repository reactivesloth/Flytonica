using System.Collections.Generic;
using Code.Internal.Drone;
using Code.Internal.SceneManagement;
using TMPro;
using UnityEngine;

namespace Code.Internal.UserInterface.Elements
{
    public class ScenarioInfoPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text typeText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Dropdown locationDropdown, droneDropdown, flyModeDropdown;
        [SerializeField] private List<DroneSettings> drones;


        private readonly Dictionary<int, MapSettings> _dropdownLocations = new();
        private Dictionary<int, DroneSettings> _dropdownDrones = new();
        private Dictionary<int, DroneFlightSettings> _dropdownFlyModes = new();

        public MapSettings CurrentMap => _dropdownLocations[locationDropdown.value];
        public DroneSettings CurrentDrone => _dropdownDrones[droneDropdown.value];
        public DroneFlightSettings CurrentFlyMode => _dropdownFlyModes[flyModeDropdown.value];

        public void Open(ScenarioSettings scenarioSettings)
        {
            droneDropdown.onValueChanged.RemoveAllListeners();
            gameObject.SetActive(true);
            Init(scenarioSettings);
            droneDropdown.onValueChanged.AddListener(_ => InitModesDropdown(scenarioSettings));
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void Init(ScenarioSettings scenarioSettings)
        {
            typeText.text = scenarioSettings.scenarioType.GetName();
            descriptionText.text = scenarioSettings.description;

            InitMapsDropdown(scenarioSettings);
            InitDronesDropDown(scenarioSettings);
        }

        private void InitMapsDropdown(ScenarioSettings scenarioSettings)
        {
            locationDropdown.ClearOptions();
            _dropdownLocations.Clear();
            var locationOptionData = new List<string>();

            if (scenarioSettings.currentMap)
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
        }

        private void InitDronesDropDown(ScenarioSettings scenarioSettings)
        {
            droneDropdown.ClearOptions();
            _dropdownDrones.Clear();
            var droneOptionData = new List<string>();

            if (scenarioSettings.currentDrone)
            {
                _dropdownDrones.Add(0, scenarioSettings.currentDrone);
                droneOptionData.Add(scenarioSettings.currentDrone.modelName);
            }
            else if(scenarioSettings.nestedScenarios == null || scenarioSettings.nestedScenarios.Count == 0)
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

            InitModesDropdown(scenarioSettings);
        }

        private void InitModesDropdown(ScenarioSettings scenarioSettings)
        {
            
            flyModeDropdown.ClearOptions();
            _dropdownFlyModes.Clear();

            var droneOptionData = new List<string>();
            if (scenarioSettings.currentDroneMode)
            {
                _dropdownFlyModes.Add(0, scenarioSettings.currentDroneMode);
                droneOptionData.Add(scenarioSettings.currentDroneMode.modeName);
            }
            else if(scenarioSettings.nestedScenarios == null || scenarioSettings.nestedScenarios.Count == 0)
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
        }
    }
}