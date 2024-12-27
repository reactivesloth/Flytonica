using System;
using Code.Internal.API;
using Code.Internal.SceneManagement;
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

        [SerializeField] private GameObject replayControlObject;
        [SerializeField] private SceneLoadingSettings sceneLoadingSettings;

        private ReplayRecordOperation _recordOperation;
        private ReplayPlaybackOperation _playbackOperation;
        private ReplayFileStorage _replayFileStorage;
        private string _replayFilePath;
        private Scene _currentReplayScene;
        private int _taskId;

        public event Action PlaybackFinished;

        public float CurrentPlaybackTime => _playbackOperation?.PlaybackTime ?? 0f;
        public float TotalPlaybackTime => !_playbackOperation.IsDisposed ? _playbackOperation.Duration : 0f;

        public bool IsSceneTransitioning { get; set; }
        
        private CustomMetadata _customMetadata;

        protected void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
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

        public void StartRecording(int taskId, string type = "")
        {
            print($"TASK_ID: {taskId}");
            _replayFilePath = System.IO.Path.Combine(Application.persistentDataPath, taskId > 0 ? $"{taskId}.replay" : "PERSONALREPLAY.replay");
            
            _replayFileStorage = ReplayFileStorage.FromFile(_replayFilePath);
            
            _customMetadata = new CustomMetadata
            {
                ReplayName = sceneLoadingSettings.currentScenarioCollection.name,
                studentName = HttpClient.UserData.name,
                type = type
            };

            _recordOperation = ReplayManager.BeginRecording(_replayFileStorage);
            Debug.Log("Начата запись реплея");
        }

        public string StopRecording()
        {
            if (_recordOperation != null)
            {
                print(_replayFilePath);
                _customMetadata.date = DateTime.Now.ToString("g");
                _replayFileStorage.Metadata = _customMetadata;
                
                _recordOperation.StopRecording();
                Debug.Log("Остановлена запись реплея");
                
                _replayFileStorage.Dispose();
                _replayFileStorage = null;
                _recordOperation.Dispose();
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
            _taskId = taskId;

            StartPlayback(System.IO.Path.Combine(Application.persistentDataPath, $"{taskId}.replay"));
            
            /*_replayFilePath = System.IO.Path.Combine(Application.persistentDataPath, $"{taskId}.replay");
            if (!System.IO.File.Exists(_replayFilePath))
            {
                Debug.LogError("Файл реплея не найден");
                return;
            }

            _replayFileStorage = ReplayFileStorage.FromFile(_replayFilePath);

            _playbackOperation = ReplayManager.BeginPlayback(_replayFileStorage);
            _playbackOperation.Options.PlaybackEndBehaviour = PlaybackEndBehaviour.LoopPlayback;
            Debug.Log("Начато воспроизведение реплея");

            _playbackOperation.OnPlaybackEnd.AddListener(OnReplayFinished);*/
        }

        public void StartPlayback(string path)
        {
            _replayFilePath = path;
            if (!System.IO.File.Exists(_replayFilePath))
            {
                Debug.LogError("Файл реплея не найден");
                return;
            }

            _replayFileStorage = ReplayFileStorage.FromFile(_replayFilePath);

            _playbackOperation = ReplayManager.BeginPlayback(_replayFileStorage);
            _playbackOperation.Options.PlaybackEndBehaviour = PlaybackEndBehaviour.LoopPlayback;
            Debug.Log("Начато воспроизведение реплея");

            _playbackOperation.OnPlaybackEnd.AddListener(OnReplayFinished);
            
            replayControlObject.SetActive(true);
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
                _playbackOperation.Dispose();
                _playbackOperation = null;
                _currentReplayScene = default;
            }
            else
            {
                Debug.LogWarning("Воспроизведение не было начато");
            }
            
            replayControlObject.SetActive(false);
            
        }

        private void OnReplayFinished()
        {
            Debug.Log("Воспроизведение реплея завершено");
            PlaybackFinished?.Invoke();
            StartPlayback(_taskId);
            Pause();
            Seek(0);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_playbackOperation != null)
                _currentReplayScene = scene;
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
        
        public void FastForward(float seconds)
        {
            if (_playbackOperation == null || _playbackOperation.IsDisposed)
            {
                Debug.LogWarning("Cannot fast-forward, playback operation is not active.");
                return;
            }

            float newTime = Mathf.Min(_playbackOperation.PlaybackTime + seconds, _playbackOperation.Duration);
            _playbackOperation.SeekPlayback(newTime);
            Debug.Log($"Перемотка вперед на {seconds} секунд. Новое время воспроизведения: {newTime} секунд.");
        }


        public void SetPlaybackSpeed(float speed)
        {
            if (_playbackOperation == null) return;
            _playbackOperation.PlaybackTimeScale = Mathf.Max(0, speed);
        }
    }
    
    [Serializable]
    public class CustomMetadata : ReplayMetadata
    {
        public string studentName = "Гость";
        public string date;
        public string type;
        public int timeInSeconds;
    }
}