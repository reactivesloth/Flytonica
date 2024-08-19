using System;
using System.Linq;
using Code.Internal.Drone;
using Code.Internal.Network;
using Code.Internal.Scenario;
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

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
            
            LoadSceneLocal("MatchmakingDemoScene");
        }

        public void LoadGame()
        {
            UnloadScene("MatchmakingDemoScene");
            LoadSceneGlobal(settings.currentMap.name,
                () =>
                {
                    FindAnyObjectByType<ScenarioInitializer>().Initialize(settings);
                    InstanceFinder.NetworkManager.GetComponent<PlayersSpawner>().SpawnDrones(settings.currentDrone);
                });
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
                    callback?.Invoke();
            };
        }

        private void UnloadScene(string sceneName)
        {
            var sud = new SceneUnloadData(sceneName);
            InstanceFinder.NetworkManager.SceneManager.UnloadGlobalScenes(sud);
        }
    }
}