using UltimateReplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Internal.Replays
{
    public class SceneChangeRecorder : ReplayRecordableBehaviour
    {
        private string _activeSceneName = string.Empty;
        private string _loadedSceneName = string.Empty;

        protected override void Awake()
        {
            base.Awake();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!IsRecording) return;

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

        protected override void OnReplayUpdate(float t)
        {
            base.OnReplayUpdate(t);
            if (!IsReplaying)
                return;

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

            _loadedSceneName = null;
            UnloadLoadedScene();
        }

        private async void ReloadScenesAsync()
        {
            if (string.IsNullOrEmpty(_activeSceneName))
                return;

            UnloadLoadedScene();

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