using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UltimateReplay;
using UltimateReplay.Formatters;
using UltimateReplay.Storage;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Internal.Replays
{
    public class ReplayController : MonoBehaviour
    {
        private static ReplayController _instance;

        public static ReplayController Instance
        {
            get
            {
                if (_instance != null) return _instance;
                var singletonObject = new GameObject("ReplayRecorderSingleton");
                _instance = singletonObject.AddComponent<ReplayController>();
                return _instance;
            }
        }

        private ReplayRecordOperation _recordOperation;
        private ReplayPlaybackOperation _playbackOperation;
        private ReplayFileStorage _replayFileStorage;
        private string _replayFilePath;
        private Scene _currentReplayScene;

        public float CurrentPlaybackTime => _playbackOperation?.PlaybackTime ?? 0f;
        public float TotalPlaybackTime => _playbackOperation?.Duration ?? 0f;

        protected void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        protected void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        public void StartRecording(int taskId)
        {
            _replayFilePath = System.IO.Path.Combine(Application.persistentDataPath, $"{taskId}.replay");

            _replayFileStorage = ReplayFileStorage.FromFile(_replayFilePath);

            _recordOperation = ReplayManager.BeginRecording(_replayFileStorage);
            Debug.Log("Начата запись реплея");
        }

        public string StopRecording()
        {
            if (_recordOperation != null)
            {
                _recordOperation.StopRecording();
                Debug.Log("Остановлена запись реплея");

                _replayFileStorage.Dispose();
                _replayFileStorage = null;
                _recordOperation = null;

                return _replayFilePath;
            }
            else
            {
                Debug.LogWarning("Запись не была начата");
                return null;
            }
        }

        public void StartPlayback(int taskId)
        {
            _replayFilePath = System.IO.Path.Combine(Application.persistentDataPath, $"{taskId}.replay");
            if (!System.IO.File.Exists(_replayFilePath))
            {
                Debug.LogError("Файл реплея не найден");
                return;
            }

            _replayFileStorage = ReplayFileStorage.FromFile(_replayFilePath);
            

            _playbackOperation = ReplayManager.BeginPlayback(_replayFileStorage);
            Debug.Log("Начато воспроизведение реплея");

            _playbackOperation.OnPlaybackStop.AddListener(OnReplayFinished);
            
            for(int i = 1; i <= _replayFileStorage.SnapshotSize; i++)
            {
                // Get the snapshot for sequence id 'i'
                ReplaySnapshot snapshot = _replayFileStorage.FetchSnapshot(i);
                
                foreach(ReplayIdentity id in snapshot.Identities)
                {
                    ReplayState objectStateData = snapshot.RestoreSnapshot(id);
                    
                    ReplayObjectFormatter objectFormatter = ReplayFormatter.GetFormatterOfType<ReplayObjectFormatter>();

                    if(objectStateData == null) continue;
                    
                    objectStateData.PrepareForRead();

                    objectFormatter.OnReplayDeserialize(objectStateData);
                    
                    Debug.Log("Component Count: " + objectFormatter.ComponentStates.Count);
                    Debug.Log("Event Count: " + objectFormatter.EventStates.Count);
                    Debug.Log("Method Count: " + objectFormatter.MethodStates.Count);
                    Debug.Log("Variable Count: " + objectFormatter.VariableStates.Count);
                }
            }
        }
        public void StopPlayback()
        {
            if (_playbackOperation != null)
            {
                _playbackOperation.StopPlayback();
                Debug.Log("Остановлено воспроизведение реплея");

                if (_currentReplayScene.IsValid())
                    SceneManager.UnloadSceneAsync(_currentReplayScene);

                _replayFileStorage.Dispose();
                _replayFileStorage = null;
                _playbackOperation = null;
                _currentReplayScene = default;
            }
            else
            {
                Debug.LogWarning("Воспроизведение не было начато");
            }
        }

        private void OnReplayFinished()
        {
            Debug.Log("Воспроизведение реплея завершено");
            StopPlayback();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_playbackOperation != null)
                _currentReplayScene = scene;

            if (_recordOperation == null)
                return;

            var replayObjects =
                new List<ReplayObject>(FindObjectsByType<ReplayObject>(FindObjectsInactive.Include,
                    FindObjectsSortMode.None));

            foreach (var replayObject in replayObjects.Where(o => o.gameObject.scene.name == scene.name))
            {
                ReplayManager.AddReplayObjectToRecordOperation(_recordOperation, replayObject);
                Debug.Log($"Объект {replayObject.name} добавлен в запись");
            }
        }

        private void OnSceneUnloaded(Scene scene)
        {
            print($"UNLOAD {scene.name}");
        }

        public void PlayReplay()
        {
            if (_playbackOperation is not { IsPlaybackPaused: true })
                return;

            _playbackOperation.ResumePlayback();
        }

        public void Pause()
        {
            if (_playbackOperation is not { IsPlaybackPaused: false })
                return;

            _playbackOperation.PausePlayback();
        }

        public void Seek(float normalizedTime)
        {
            _playbackOperation?.SeekPlaybackNormalized(normalizedTime);
        }

        public void SetPlaybackSpeed(float speed)
        {
            if (_playbackOperation == null) return;
                _playbackOperation.PlaybackTimeScale = Mathf.Max(0, speed);
        }
    }
}