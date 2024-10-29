using System;
using Code.Internal.Drone;
using Code.Internal.Scenario.Race;
using Code.Internal.Scenario.Searching;
using Code.Internal.Scenario.Transport;
using Code.Internal.SceneManagement;
using UltimateReplay;
using UnityEngine;

namespace Code.Internal.Scenario
{
    public class ScenarioInitializer : MonoBehaviour
    {
        [SerializeField] private GameObject freeFlightObjects;
        [SerializeField] private GameObject tutorialModeObjects;
        [SerializeField] private GameObject raceModeObjects;
        [SerializeField] private GameObject transportModeObjects;
        [SerializeField] private GameObject searchingModeObjects;
        [SerializeField] private GameObject searchingIRModeObjects;

        private ScenarioSettings _settings;
        private bool _cameraInitialized = false;
        
        private void Update()
        {
            if (DroneController.Instance == null)
            {
                _cameraInitialized = false;
            }

            if (DroneInput.Instance != null && !_cameraInitialized)
            {
                if (_settings.currentDrone != null)
                    _settings.currentDrone.currentFlightMode = _settings.currentDroneMode;
             
                DroneInput.Instance.DroneCanSwitchCam = _settings == null ? true : _settings.cameraSwitchAllowed;
                DroneInput.Instance.DroneCam = _settings == null ? false : !_settings.cameraThirdPerson;
                _cameraInitialized = true;
            }
        }

        public void Initialize(ScenarioSettings settings)
        {
            _cameraInitialized = false;
            _settings = settings;
            _settings.currentDrone.currentFlightMode = _settings.currentDroneMode;
            freeFlightObjects?.SetActive(false);
            tutorialModeObjects?.SetActive(false);
            raceModeObjects?.SetActive(false);
            transportModeObjects?.SetActive(false);
            searchingModeObjects?.SetActive(false);
            searchingIRModeObjects?.SetActive(false);

            var objectsToClean = FindObjectsOfType<SpawnableObject>(true);
            foreach (var o in objectsToClean)
            {
                if (o.GetComponent<SpawnableObject>().Type != MapEditorObjectType.SpawnPoint)
                {
                    DestroyImmediate(o.gameObject);
                }
            }
            
            objectsToClean = FindObjectsOfType<SpawnableObject>(true);
            var objects = _settings.objects;

            foreach (var spawnedObject in objects)
            {
                if (spawnedObject != null)
                {
                    var sObj = Instantiate(
                        Resources.Load(spawnedObject.prefabName.Replace("(Clone)", "")) as GameObject).transform;
                    
                    ReplayManager.AddReplayObjectToRecordScenes(sObj.gameObject);

                    if (sObj.GetComponent<SpawnableObject>().Type == MapEditorObjectType.SpawnPoint)
                    {
                        foreach (var o in objectsToClean)
                        {
                            if (o.GetComponent<SpawnableObject>().Type == MapEditorObjectType.SpawnPoint)
                                DestroyImmediate(o.gameObject);
                        }
                    }

                    switch (_settings.scenarioType)
                    {
                        case ScenarioType.FreeFlight:
                            sObj.SetParent(freeFlightObjects.transform);
                            break;
                        case ScenarioType.Tutorial:
                            sObj.SetParent(tutorialModeObjects.transform);
                            break;
                        case ScenarioType.Race:
                            sObj.SetParent(raceModeObjects.transform);
                            break;
                        case ScenarioType.Transport:
                            sObj.SetParent(transportModeObjects.transform);
                            break;
                        case ScenarioType.Searching:
                            sObj.SetParent(searchingModeObjects.transform);
                            break;
                        case ScenarioType.SearchingWithIR:
                            sObj.SetParent(searchingIRModeObjects.transform);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    sObj.transform.position = spawnedObject.position;
                    sObj.transform.rotation = spawnedObject.rotation;
                    sObj.transform.localScale = spawnedObject.scale;
                    sObj.GetComponent<SpawnableObject>().sortOrder = spawnedObject.sortOrder;
                    sObj.SetSiblingIndex(spawnedObject.sortOrder);
                }
            }

            switch (_settings.scenarioType)
            {
                case ScenarioType.FreeFlight:
                    freeFlightObjects.SetActive(true);
                    break;
                case ScenarioType.Tutorial:
                    tutorialModeObjects?.SetActive(true);
                    break;
                case ScenarioType.Race:
                    raceModeObjects?.SetActive(true);
                    FindAnyObjectByType<ScenarioRace>().Initialize(_settings);
                    break;
                case ScenarioType.Transport:
                    transportModeObjects?.SetActive(true);
                    FindAnyObjectByType<ScenarioTransport>().Initialize(_settings);
                    break;
                case ScenarioType.Searching:
                    searchingModeObjects?.SetActive(true);
                    FindAnyObjectByType<ScenarioSearching>().Initialize(_settings);
                    break;
                case ScenarioType.SearchingWithIR:
                    searchingIRModeObjects?.SetActive(true);
                    FindAnyObjectByType<ScenarioSearching>().Initialize(_settings);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            Wind.Instance.Init(_settings.windSettings);
        }
    }
}