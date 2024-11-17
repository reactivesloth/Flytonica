using System.Linq;
using Code.Internal.UserInterface;
using System.Threading.Tasks;
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
        private bool _replayEnded = false;

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
            if(string.IsNullOrEmpty(_activeSceneName) && trackedScenesNames.Contains(SceneManager.GetActiveScene().name))
                _activeSceneName = SceneManager.GetActiveScene().name;
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

            _replayEnded = false; // Сбрасываем флаг при старте реплея
            _loadedSceneName = string.Empty;
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

            _replayEnded = true; // Устанавливаем флаг окончания реплея
            _loadedSceneName = string.Empty;
            DroneHUD.Instance.ShowHUD(false);
            UnloadAllScenes();
        }

        private async void ReloadScenesAsync()
        {
            if (_replayEnded) return;

            ReplayController.Instance.Pause();
            ReplayController.Instance.IsSceneTransitioning = true;

            await UnloadLoadedScene();

            if (string.IsNullOrEmpty(_activeSceneName) || _replayEnded)
                return;

            var loadSceneOp = SceneManager.LoadSceneAsync(_activeSceneName, LoadSceneMode.Additive);
            await loadSceneOp;

            if (_replayEnded)
            {
                // Если реплей закончился во время загрузки, выгружаем сцену
                await SceneManager.UnloadSceneAsync(_activeSceneName);
                return;
            }

            var loadedScene = SceneManager.GetSceneByName(_activeSceneName);
            if (loadedScene.IsValid())
                SceneManager.SetActiveScene(loadedScene);

            ReplayController.Instance.PlayReplay();
            ReplayController.Instance.IsSceneTransitioning = false;

            Debug.Log($"Сцена {_activeSceneName} загружена во время воспроизведения");
        }

        private async Task UnloadLoadedScene()
        {
            if (string.IsNullOrEmpty(_loadedSceneName) || _replayEnded)
                return;

            var unloadedScene = SceneManager.GetSceneByName(_loadedSceneName);
            if (unloadedScene.IsValid())
                await SceneManager.UnloadSceneAsync(_loadedSceneName);
        }

        private async void UnloadAllScenes()
        {
            foreach (var sceneName in trackedScenesNames)
            {
                var scene = SceneManager.GetSceneByName(sceneName);
                if (scene.isLoaded)
                {
                    Debug.Log($"Выгрузка сцены: {sceneName}");
                    await SceneManager.UnloadSceneAsync(sceneName);
                }
            }
        }
    }
}