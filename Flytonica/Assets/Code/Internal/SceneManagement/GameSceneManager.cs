using System;
using System.Linq;
using Code.Internal.Drone;
using Code.Internal.Network;
using Code.Internal.Scenario;
using Code.Internal.UserInterface;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Internal.SceneManagement
{
    public class GameSceneManager : NetworkBehaviour
    {
        public static GameSceneManager Instance { get; private set; }
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
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            UIController.Instance.OnGameStart();
            IsPlaying = true;
        }
        
        public void LoadGlobalScene(MapSettings sceneSettingsCurrentMap, Action callback = null)
        {
            var sceneName = sceneSettingsCurrentMap.loadingSceneName;

            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("Scene name is null or empty. Please check the MapSettings.");
                return;
            }
            
            var sceneLoadData = new SceneLoadData(sceneName);
            Action<SceneLoadEndEventArgs> onSceneLoaded = null;

            onSceneLoaded = args =>
            {
                if(!args.LoadedScenes.Select(s => s.name).Contains(sceneName)) 
                    return;
                Debug.Log($"Scene {sceneName} loaded successfully.");
                callback?.Invoke();
                InstanceFinder.SceneManager.OnLoadEnd -= onSceneLoaded;
            };

            InstanceFinder.SceneManager.OnLoadEnd += onSceneLoaded;
            InstanceFinder.SceneManager.LoadGlobalScenes(sceneLoadData);
        }

        public void ToMenuSingle()
        {
            UIController.Instance.OnMainMenu();
            UnloadScene();
            IsPlaying = false;
        }

        private void LoadSceneLocal(string sceneName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }

        private void UnloadScene()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(scene.name != "Main")
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
        }
        
        public void Replay()
        {
            InstanceFinder.NetworkManager.GetComponent<PlayersSpawner>().DespawnAll();
            UnloadScene();
        }
    }
}