using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UltimateReplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Internal.Replays
{
    public class SceneChangeRecorder : ReplayBehaviour
    {
        private const ushort SceneChangeEventID = 1;
        private const ushort SceneUnloadEventID = 2;

        protected override void Awake()
        {
            base.Awake();
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!IsRecording) return;

            var eventData = ReplayState.pool.GetReusable();
            eventData.Write(scene.name);

            RecordEvent(SceneChangeEventID, eventData);

            Debug.Log($"Смена сцены записана: {scene.name}");
        }

        private void OnSceneUnloaded(Scene scene)
        {
            if (!IsRecording) return;

            var eventData = ReplayState.pool.GetReusable();
            eventData.Write(scene.name);

            RecordEvent(SceneUnloadEventID, eventData);

            Debug.Log($"Выгрузка сцены записана: {scene.name}");
        }

        protected override void OnReplayEvent(ushort eventID, ReplayState eventData)
        {
            var sceneName = eventData.ReadString();

            if (eventID == SceneChangeEventID)
            {
                if (IsReplaying)
                {
                    // Загружаем сцену и ждем, пока она загрузится
                    StartCoroutine(LoadSceneCoroutine(sceneName));
                }
            }
            else if (eventID == SceneUnloadEventID)
            {
                if (IsReplaying)
                {
                    SceneManager.UnloadSceneAsync(sceneName);
                    Debug.Log($"Сцена {sceneName} выгружена во время воспроизведения");
                }
            }
        }

        private IEnumerator LoadSceneCoroutine(string sceneName)
        {
            var asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (asyncLoad is { isDone: false })
                yield return null;
            
            var loadedScene = SceneManager.GetSceneByName(sceneName);
            if (loadedScene.IsValid())
                SceneManager.SetActiveScene(loadedScene);
            
            Debug.Log($"Сцена {sceneName} загружена во время воспроизведения");
        }
    }
}