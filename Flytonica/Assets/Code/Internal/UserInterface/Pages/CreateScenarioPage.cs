using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Code.Internal.API;
using Code.Internal.API.Wrappers;
using Code.Internal.Drone;
using Code.Internal.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class CreateScenarioPage : Page
    {
        [Serializable]
        private struct StepMark
        {
            public Image numberMark, lineMark;

            public void Unmark() => SetMark(false);
            public void Mark() => SetMark(true);

            public void SetMark(bool isMark)
            {
                numberMark?.gameObject.SetActive(isMark);
                lineMark?.gameObject.SetActive(isMark);
            }
        }

        [Serializable]
        private class TestSaveFormat
        {
            public MapSettings currentMap;
            public ScenarioType currentType;
            public DroneSettings currentDrone;
            public DroneFlightSettings currentMode;

            public TestSaveFormat(MapSettings currentMap, ScenarioType currentType, DroneSettings currentDrone,
                DroneFlightSettings currentMode)
            {
                this.currentMap = currentMap;
                this.currentType = currentType;
                this.currentDrone = currentDrone;
                this.currentMode = currentMode;
            }
        }

        [Header("Containers: ")] [SerializeField]
        private AvailableMapsSettings availableMaps;

        [SerializeField] private AvailableDronesSettings availableDrones;

        [Header("Elements: ")] [SerializeField]
        private Button continueButton;

        [SerializeField] private Button toLocationSettingsButton;

        [Header("Steps management: ")] [SerializeField]
        private List<StepMark> stepsMarks;

        [SerializeField] private GameObject step1, step2, step3, step4;

        [Header("Step 1: ")] [SerializeField] private Toggle enableViewSelection;
        [SerializeField] private Toggle viewSelection;
        [SerializeField] private ToggleGroup mapsGroup;
        [SerializeField] private ToggleGroup typeSelectionGroup;

        private readonly Dictionary<Toggle, MapSettings> _mapToggles = new();
        private readonly Dictionary<Toggle, ScenarioType> _scenarioTypeToggles = new();

        [Header("Step 2: ")] [SerializeField] private ToggleGroup droneGroup;
        [SerializeField] private ToggleGroup modeGroup;

        private readonly Dictionary<Toggle, DroneSettings> _droneToggles = new();
        private readonly Dictionary<Toggle, DroneFlightSettings> _modeToggles = new();

        [Header("Step 3: ")] [SerializeField] private Toggle isWindToggle;
        [SerializeField] private Toggle isAllLayersEqualToggle;
        [SerializeField] private GameObject windHeader;
        [SerializeField] private GameObject overlaySettings;
        [SerializeField] private List<GameObject> layersSettings;
        [SerializeField] private TMP_Dropdown overlayForce, overlayDirection;
        [SerializeField] private List<TMP_Dropdown> forces, directions;

        [Header("Step 4: ")] [SerializeField] private TMP_InputField title;
        [SerializeField] private TMP_InputField description;

        [Header("Prefabs: ")] [SerializeField] private Toggle togglePrefab;

        private int _currentStep;

        private bool _isViewSelection;
        private bool _isFirstView;

        private MapSettings _currentMap;
        private ScenarioType _currentType;
        private DroneSettings _currentDrone;
        private DroneFlightSettings _currentMode;

        protected override void Awake()
        {
            base.Awake();

            isWindToggle.onValueChanged.AddListener(WindTogglesChange);
            isAllLayersEqualToggle.onValueChanged.AddListener(WindTogglesChange);
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            _currentStep = 1;
            SetStep();
            continueButton.onClick.AddListener(NextStep);
            toLocationSettingsButton.onClick.AddListener(GoToLocationSettings);
        }

        protected override void OnClose()
        {
            base.OnClose();
            continueButton.onClick.RemoveListener(NextStep);
            toLocationSettingsButton.onClick.RemoveListener(GoToLocationSettings);
        }

        protected override void OnBackClick()
        {
            --_currentStep;
            if (_currentStep < 1)
                base.OnBackClick();
            else
                SetStep(true);
        }

        private void GoToLocationSettings()
        {
            var wind = new List<WindLayerSettings>();

            for (var i = 0; i < _currentMap.windLayersCount; i++)
                wind.Add(!isAllLayersEqualToggle
                    ? new WindLayerSettings(forces[i].value, directions[i].value)
                    : new WindLayerSettings(overlayForce.value, overlayDirection.value));

            //Этот объект надо будет пробросит в 3D редактор чтобы добавлять предметы
            var dataContainer = new ScenarioSettingsData(title.text, description.text,
                availableDrones.drones.IndexOf(_currentDrone),
                availableMaps.maps.IndexOf(_currentMap), _currentType, _currentDrone.flightModes.IndexOf(_currentMode),
                wind);


            //Tут временно отправка файла
            var jsonData = JsonUtility.ToJson(dataContainer);
            print(jsonData);
            var settingsFile = Encoding.UTF8.GetBytes(jsonData);

            var form = new WWWForm();
            form.AddField("name", title.text);
            form.AddBinaryData("file", settingsFile, $"Scenario.json");

            HttpClient.PostFormData(LinkConstants.MapConfigCreateUrl, form);
        }

        private void NextStep()
        {
            ++_currentStep;
            SetStep();
        }

        private void SetStep(bool isBack = false)
        {
            if (!isBack)
            {
                switch (_currentStep)
                {
                    case 1:
                        InitStep1();
                        break;
                    case 2:
                        _isViewSelection = enableViewSelection.isOn;
                        _isFirstView = viewSelection.isOn;
                        _currentMap = _mapToggles[mapsGroup.GetFirstActiveToggle()];
                        _currentType = _scenarioTypeToggles[typeSelectionGroup.GetFirstActiveToggle()];
                        InitStep2();
                        break;
                    case 3:
                        _currentDrone = _droneToggles[droneGroup.GetFirstActiveToggle()];
                        _currentMode = _modeToggles[modeGroup.GetFirstActiveToggle()];
                        InitStep3();
                        break;
                    case 4:
                        InitStep4();
                        break;
                    default:
                        Debug.LogError("Argument Index exeption");
                        break;
                }
            }

            OnCurrentStep(_currentStep);
            continueButton.gameObject.SetActive(_currentStep < 4);
            toLocationSettingsButton.gameObject.SetActive(_currentStep == 4);
            SetStepMarks();
        }

        private void SetStepMarks()
        {
            stepsMarks.Select((mark, index) => new { mark, index })
                .ToList()
                .ForEach(item => item.mark.SetMark(item.index < _currentStep));
        }


        private void OnCurrentStep(int step)
        {
            step1.SetActive(step == 1);
            step2.SetActive(step == 2);
            step3.SetActive(step == 3);
            step4.SetActive(step == 4);
        }

        private void InitStep1()
        {
            ClearToggles(_mapToggles);
            ClearToggles(_scenarioTypeToggles);

            foreach (var map in availableMaps.maps)
                SetToggle(mapsGroup, map.name, _mapToggles, map);

            foreach (ScenarioType type in Enum.GetValues(typeof(ScenarioType)))
                SetToggle(typeSelectionGroup, type.GetName(), _scenarioTypeToggles, type);
        }

        private void InitStep2()
        {
            ClearToggles(_droneToggles);
            foreach (var drone in availableDrones.drones)
                SetToggle(droneGroup, drone.name, _droneToggles, drone);

            foreach (var droneToggle in _droneToggles.Keys)
                droneToggle.onValueChanged.AddListener(_ => UpdateModes());
        }

        private void UpdateModes(DroneSettings drone = null)
        {
            var modes = drone != null
                ? drone.flightModes
                : _droneToggles[droneGroup.GetFirstActiveToggle()].flightModes;

            ClearToggles(_modeToggles);

            foreach (var mode in modes)
                SetToggle(modeGroup, mode.name, _modeToggles, mode);
        }

        private void InitStep3()
        {
            var isEnableWindSettings = _currentMap.windLayersCount > 0;

            isWindToggle.isOn = !isEnableWindSettings;
            isWindToggle.interactable = isEnableWindSettings;

            WindTogglesChange();
        }

        private void WindTogglesChange(bool _ = false)
        {
            var isWind = !isWindToggle.isOn;
            var isEqual = isAllLayersEqualToggle.isOn;
            print($"{isWind} {isEqual}");

            windHeader.SetActive(isWind);
            isAllLayersEqualToggle.interactable = isWind;

            if (isWind)
            {
                if (isEqual)
                {
                    overlaySettings.SetActive(true);
                    layersSettings.ForEach(l => l.SetActive(false));
                }
                else
                {
                    overlaySettings.SetActive(false);
                    layersSettings
                        .Select((layer, index) => new { layer, index })
                        .ToList()
                        .ForEach(item => item.layer.SetActive(item.index < _currentMap.windLayersCount));
                }
            }
            else
            {
                overlaySettings.SetActive(false);
                layersSettings.ForEach(l => l.SetActive(false));
            }
        }

        //Last
        private void InitStep4()
        {
        }

        private void SetToggle<T>(ToggleGroup group, string text, IDictionary<Toggle, T> dictionary, T data)
        {
            var toggle = Instantiate(togglePrefab, group.transform);
            toggle.GetComponentInChildren<TMP_Text>().text = text;
            toggle.group = group;
            dictionary.Add(toggle, data);
        }

        private void ClearToggles<T>(Dictionary<Toggle, T> toggles)
        {
            foreach (var t in toggles)
                Destroy(t.Key.gameObject);

            toggles.Clear();
        }
    }
}