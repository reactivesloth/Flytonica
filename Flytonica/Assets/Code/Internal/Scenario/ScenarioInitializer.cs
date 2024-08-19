using System;
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
        
        public void Initialize(SceneLoadingSettings settings)
        {
            freeFlightObjects?.SetActive(false);
            tutorialModeObjects?.SetActive(false);
            raceModeObjects?.SetActive(false);
            transportModeObjects?.SetActive(false);
            searchingModeObjects?.SetActive(false);
            searchingIRModeObjects?.SetActive(false);
            
            switch (settings.currentScenario.scenarioType)
            {
                case ScenarioType.FreeFlight:
                    freeFlightObjects.SetActive(true);
                    break;
                case ScenarioType.Tutorial:
                    tutorialModeObjects?.SetActive(true);
                    break;
                case ScenarioType.Race:
                    raceModeObjects?.SetActive(true);
                    break;
                case ScenarioType.Transport:
                    transportModeObjects?.SetActive(true);
                    break;
                case ScenarioType.Searching:
                    searchingModeObjects?.SetActive(true);
                    break;
                case ScenarioType.SearchingWithIR:
                    searchingIRModeObjects?.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}