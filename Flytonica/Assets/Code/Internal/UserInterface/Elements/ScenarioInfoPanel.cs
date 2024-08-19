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
        

        private readonly Dictionary<int, MapSettings> _dropdownLocations = new ();
        private Dictionary<int, DroneSettings> _dropdownDrones = new ();
        private Dictionary<int, DroneFlightSettings> _dropdownFlyModes = new ();

        public MapSettings CurrentMap => _dropdownLocations[locationDropdown.value];
        public DroneSettings CurrentDrone => _dropdownDrones[droneDropdown.value];
        public DroneFlightSettings CurrentFlyMode => _dropdownFlyModes[flyModeDropdown.value];
        
        public void Open(ScenarioSettings scenarioSettings)
        {
            gameObject.SetActive(true);
            Init(scenarioSettings);
            droneDropdown.onValueChanged.AddListener(InitModesDropdown);
        }

        public void Close()
        {
            gameObject.SetActive(false);
            droneDropdown.onValueChanged.RemoveListener(InitModesDropdown);
        }

        private void Init(ScenarioSettings scenarioSettings)
        {
            typeText.text = scenarioSettings.GetScenarioTypeName(scenarioSettings.scenarioType);
            descriptionText.text = scenarioSettings.description;
            
            
            locationDropdown.ClearOptions();
            _dropdownLocations.Clear();
            var locationOptionData = new List<string>();
            for (var i = 0; i < scenarioSettings.availableMaps.Length; i++)
            {
                var scenarioSettingsAvailableMap = scenarioSettings.availableMaps[i];
                _dropdownLocations.Add(i, scenarioSettingsAvailableMap);
                locationOptionData.Add(scenarioSettingsAvailableMap.name);
            }
            locationDropdown.AddOptions(locationOptionData);
            
            droneDropdown.ClearOptions();
            _dropdownDrones.Clear();
            var droneOptionData = new List<string>();
            for (var i = 0; i < drones.Count; i++)
            {
                var droneSettings = drones[i];
                _dropdownDrones.Add(i, droneSettings);
                droneOptionData.Add(droneSettings.name);
            }
            droneDropdown.AddOptions(droneOptionData);
            
            InitModesDropdown(0);
        }

        private void InitModesDropdown(int _)
        {
            flyModeDropdown.ClearOptions();
            _dropdownFlyModes.Clear();
            var droneOptionData = new List<string>();
            for (var i = 0; i < CurrentDrone.flightModes.Length; i++)
            {
                var droneMode = CurrentDrone.flightModes[i];
                _dropdownFlyModes.Add(i, droneMode);
                droneOptionData.Add(droneMode.name);
            }
            flyModeDropdown.AddOptions(droneOptionData);
        }
    }
}
