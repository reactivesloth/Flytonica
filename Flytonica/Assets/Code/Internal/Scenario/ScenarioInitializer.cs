using System;
using Code.Internal.Drone;
using Code.Internal.Scenario.Race;
using Code.Internal.Scenario.Searching;
using Code.Internal.SceneManagement;
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
            if (DroneInput.Instance != null && !_cameraInitialized)
            {
                DroneInput.Instance.DroneCanSwitchCam = _settings == null ? true : _settings.cameraSwitchAllowed;
                DroneInput.Instance.DroneCam = _settings == null ? false : !_settings.cameraThirdPerson;
                _cameraInitialized = true;
            }
        }

        public void Initialize(ScenarioSettings settings)
        {
            _cameraInitialized = false;
            _settings = settings;
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
                var sObj = Instantiate(Resources.Load(spawnedObject.prefabName.Replace("(Clone)", "")) as GameObject).transform;
                sObj.position = spawnedObject.position;
                sObj.rotation = spawnedObject.rotation;
                sObj.transform.localScale = spawnedObject.scale;
                
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
                        FindAnyObjectByType<ScenarioRace>().Initialize();
                        break;
                    case ScenarioType.Transport:
                        sObj.SetParent(raceModeObjects.transform);
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
                    FindAnyObjectByType<ScenarioRace>().Initialize();
                    break;
                case ScenarioType.Transport:
                    transportModeObjects?.SetActive(true);
                    break;
                case ScenarioType.Searching:
                    searchingModeObjects?.SetActive(true);
                    FindAnyObjectByType<ScenarioSearching>().Initialize();
                    break;
                case ScenarioType.SearchingWithIR:
                    searchingIRModeObjects?.SetActive(true);
                    FindAnyObjectByType<ScenarioSearching>().Initialize();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}