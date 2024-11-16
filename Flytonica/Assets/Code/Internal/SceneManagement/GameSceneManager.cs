using System;
using System.Linq;
using System.Threading.Tasks;
using Code.Internal.Replays;
using Code.Internal.UserInterface;
using FishNet;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using UltimateReplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Internal.SceneManagement
{
    public class GameSceneManager : NetworkBehaviour
    {
        public static GameSceneManager Instance { get; private set; }
        public string CurrentGlobalScene { get; private set; }
        public MapSettings CurrentMapSettings { get; private set; }

        public bool IsPlaying { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);

            QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("QualitySettingsLevel", 1), false);
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
            CurrentMapSettings = sceneSettingsCurrentMap;
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
                if (!args.LoadedScenes.Select(s => s.name).Contains(sceneName))
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
            ReplayController.Instance.StopRecording();
            UIController.Instance.OnMainMenu();
            UnloadScene();
            IsPlaying = false;
            var player = GameObject.FindWithTag("Player");
            player.transform.position = Vector3.zero;
        }

        private void LoadSceneLocal(string sceneName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }

        private void UnloadScene()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.name != "Main")
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
        }

        public void Replay()
        {
            if (!IsPlaying)
                return;
            if (NetworkManager == null || NetworkManager.ClientManager == null)
                return;

            IsPlaying = false;

            NetworkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
            NetworkManager.ClientManager.StopConnection();
        }

        private void OnClientConnectionState(ClientConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Stopped)
            {
                NetworkManager.ClientManager.StartConnection();
            }
            else if (args.ConnectionState == LocalConnectionState.Started)
            {
                UIController.Instance.OnGameStart();
                IsPlaying = true;
                NetworkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
            }
        }
    }
}