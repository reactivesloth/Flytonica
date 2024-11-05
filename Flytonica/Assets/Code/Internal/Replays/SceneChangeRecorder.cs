using System.Linq;
using Code.Internal.UserInterface;
using UltimateReplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Internal.Replays
{
    public class SceneChangeRecorder : ReplayRecordableBehaviour
    {
        [SerializeField] private string[] trackedScenesNames;
        
        private string _activeSceneName = string.Empty;
        private string _loadedSceneName = string.Empty;

        protected override void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            base.Awake();
        }

        protected override void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            base.OnDestroy();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!IsRecording || !trackedScenesNames.Contains(scene.name)) return;

            _activeSceneName = scene.name;
            Debug.Log($"Смена сцены записана: {scene.name}");
        }

        public override void OnReplaySerialize(ReplayState state)
        {
            state.Write(_activeSceneName);
        }

        public override void OnReplayDeserialize(ReplayState state)
        {
            _activeSceneName = state.ReadString();
        }

        protected override void OnReplayStart()
        {
            base.OnReplayStart();
            if (!IsReplaying)
                return;
            
            _loadedSceneName = string.Empty;
        }

        protected override void OnReplayUpdate(float t)
        {
            base.OnReplayUpdate(t);
            if (!IsReplaying)
                return;

            print($"{_loadedSceneName} == {_activeSceneName}");
            
            if (_loadedSceneName == _activeSceneName || string.IsNullOrEmpty(_activeSceneName))
                return;

            ReloadScenesAsync();
            _loadedSceneName = _activeSceneName;
        }

        protected override void OnReplayEnd()
        {
            base.OnReplayEnd();
            if (!IsReplaying)
                return;

            print("Replay end event");
            UnloadLoadedScene();
            _loadedSceneName = string.Empty;
            DroneHUD.Instance.ShowHUD(false);
        }

        private async void ReloadScenesAsync()
        {
            UnloadLoadedScene();

            if (string.IsNullOrEmpty(_activeSceneName))
                return;
            
            await SceneManager.LoadSceneAsync(_activeSceneName, LoadSceneMode.Additive);
            var loadedScene = SceneManager.GetSceneByName(_activeSceneName);
            if (loadedScene.IsValid())
                SceneManager.SetActiveScene(loadedScene);

            Debug.Log($"Сцена {_activeSceneName} загружена во время воспроизведения");
        }

        private void UnloadLoadedScene()
        {
            if (string.IsNullOrEmpty(_loadedSceneName))
                return;

            var unloadedScene = SceneManager.GetSceneByName(_loadedSceneName);
            if (unloadedScene.IsValid())
                SceneManager.UnloadSceneAsync(_loadedSceneName);
        }
    }
}