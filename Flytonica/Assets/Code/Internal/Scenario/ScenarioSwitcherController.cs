using System;
using Code.Internal.Network;
using Code.Internal.SceneManagement;
using FishNet;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Internal.Scenario
{
    public class ScenarioSwitcherController : MonoBehaviour
    {
        public static ScenarioSwitcherController Instance { get; private set; }

        [SerializeField] private SceneLoadingSettings sceneSettings;

        private void Awake()
        {
            Instance = this;
        }

        public void NextOrEnd()
        {
            if(sceneSettings.currentScenario.nextScenario)
                Next();
            else
                End();
        }

        private void Next()
        {
            sceneSettings.currentScenario = sceneSettings.currentScenario.nextScenario;

            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            InstanceFinder.NetworkManager.GetComponent<PlayersSpawner>().DespawnAll();
            GameSceneManager.Instance.LoadGlobalScene(sceneSettings.currentScenario.currentMap, OnSceneLoaded);
            //TODO: Переключение карты, инициализация нового сценария, спавн нового дрона 
            
            //End();
        }

        private void OnSceneLoaded()
        {
            FindAnyObjectByType<ScenarioInitializer>().Initialize(sceneSettings.currentScenario);
            var drone = InstanceFinder.NetworkManager.GetComponent<PlayersSpawner>().Spawn(InstanceFinder.ClientManager.Connection, sceneSettings.currentScenario.currentDrone);
        }

        private void End()
        {
            //TODO: Send log (report)
            
            if (InstanceFinder.ServerManager.Started)
                InstanceFinder.ServerManager.StopConnection(true);
            InstanceFinder.ClientManager.StopConnection();

            GameSceneManager.Instance.ToMenuSingle();
        }
    }
}