using System;
using System.Collections;
using System.Collections.Generic;
using UltimateReplay;
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

            var isReplayFile = ReplayFileStorage.IsReplayFile(_replayFilePath);
            Debug.Log("IsReplayFile = " + isReplayFile);

            _replayFileStorage = ReplayFileStorage.FromFile(_replayFilePath);

            _playbackOperation = ReplayManager.BeginPlayback(_replayFileStorage);
            Debug.Log("Начато воспроизведение реплея");

            _playbackOperation.OnPlaybackStop.AddListener(OnReplayFinished);
        }

        public void StopPlayback()
        {
            if (_playbackOperation != null)
            {
                _playbackOperation.StopPlayback();
                Debug.Log("Остановлено воспроизведение реплея");

                _replayFileStorage.Dispose();
                _replayFileStorage = null;
                _playbackOperation = null;
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
    }
}
