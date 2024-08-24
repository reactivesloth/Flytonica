using System;
using System.Linq;
using Code.Internal.Drone;
using Code.Internal.Network;
using Code.Internal.Scenario;
using Code.Internal.UserInterface;
using FishNet;
using FishNet.Managing.Scened;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Internal.SceneManagement
{
    public class GameSceneManager : MonoBehaviour
    {
        public static GameSceneManager Instance { get; private set; }

        public SceneLoadingSettings settings;

        public string CurrentGlobalScene { get; private set; }
        public bool IsPlaying { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
            
            LoadSceneLocal("UI Scene");
        }

        public void LoadGame()
        {
            LoadSceneGlobal(settings.currentMap.loadingSceneName,
                () =>
                {
                    FindAnyObjectByType<ScenarioInitializer>().Initialize(settings);
                    InstanceFinder.NetworkManager.GetComponent<PlayersSpawner>().SpawnDrones(settings.currentDrone);
                    UIController.Instance.OnGameStart();
                    IsPlaying = true;
                });
        }

        public void Replay()
        {
            InstanceFinder.NetworkManager.GetComponent<PlayersSpawner>().Despawn();
            UnloadSceneGlobal(CurrentGlobalScene, LoadGame);
        }

        public void ToMenuSingle()
        {
            StopLocalConnection();
            UIController.Instance.OnMainMenu();
            UnloadScene(CurrentGlobalScene);
            IsPlaying = false;
        }

        private void StopLocalConnection()
        {
            InstanceFinder.NetworkManager.GetComponent<PlayersSpawner>().Despawn();
            InstanceFinder.ClientManager.StopConnection();
            InstanceFinder.ServerManager.StopConnection(false);
        }

        private void LoadSceneLocal(string sceneName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }
        
        /// <summary>
        /// Загрузка глобальной сцены для всех подключений 
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="callback"></param>
        private void LoadSceneGlobal(string sceneName, Action callback = null)
        {
            var sceneData = new SceneLoadData(sceneName);
            InstanceFinder.SceneManager.LoadGlobalScenes(sceneData);
            InstanceFinder.SceneManager.OnLoadEnd += args =>
            {
                if (args.LoadedScenes.Select(s => s.name).Contains(sceneName))
                {
                    callback?.Invoke();
                    CurrentGlobalScene = sceneName;
                }
            };
        }

        private void UnloadSceneGlobal(string sceneName, Action callback = null)
        {
            var sud = new SceneUnloadData(sceneName);
            InstanceFinder.NetworkManager.SceneManager.UnloadGlobalScenes(sud);
            InstanceFinder.SceneManager.OnUnloadEnd += args =>
            {
                callback?.Invoke();
                CurrentGlobalScene = null;
            };
        }

        private void UnloadScene(string sceneName)
        {
            var sud = new SceneUnloadData(sceneName);
            InstanceFinder.NetworkManager.SceneManager.UnloadGlobalScenes(sud);
        }
    }
}