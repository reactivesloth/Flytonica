using System;
using System.Collections;
using Code.Internal.API;
using Code.Internal.API.Wrappers.ReceiveModels;
using Code.Internal.Replays;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class ViewReplayPage: Page
    {
        [SerializeField] private Button mainMenuButton, toStudentsButton, mapButton;

        [SerializeField] private Button playButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Slider seekSlider;
        [SerializeField] private Button speedButton;
        [SerializeField] private TMP_Text currentTimeText, allTimeText, nameText;
        
        [SerializeField] private float[] playbackSpeeds = { 0.5f, 1f, 1.5f, 2f };
        [SerializeField] private int currentSpeedIndex = 1;
        
        private bool _isUpdatingSlider;

        private void OnEnable()
        {
            playButton.onClick.AddListener(OnPlayButtonPressed);
            pauseButton.onClick.AddListener(OnPauseButtonPressed);
            seekSlider.onValueChanged.AddListener(OnSeekSliderChanged);
            speedButton.onClick.AddListener(OnSpeedButtonPressed);

            UpdatePlayPauseButtons(true);
            UpdateSpeedButtonLabel();
            
            StartCoroutine(UpdateSliderCoroutine());
        }

        private void OnDisable()
        {
            playButton.onClick.RemoveListener(OnPlayButtonPressed);
            pauseButton.onClick.RemoveListener(OnPauseButtonPressed);
            seekSlider.onValueChanged.RemoveListener(OnSeekSliderChanged);
            speedButton.onClick.RemoveListener(OnSpeedButtonPressed);
            
            StopCoroutine(UpdateSliderCoroutine());
        }

        public void Init(LogData logData)
        {
            var fileName = $"{logData.user_scenario_id}.replay";
            var filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);

            nameText.text = logData.scenario_name;
            
            if (System.IO.File.Exists(filePath))
            {
                ReplayController.Instance.StartPlayback(logData.user_scenario_id);
            }
            else
            {
                HttpClient.GetBinary(LinkConstants.GetFile(logData.replay_file_path), onSuccess: bytes =>
                {
                    System.IO.File.WriteAllBytes(filePath, bytes);
                    ReplayController.Instance.StartPlayback(logData.user_scenario_id);
                });
            }
        }

        protected override void OnClose()
        {
            ReplayController.Instance.StopPlayback();
            base.OnClose();
        }
        
        private void OnPlayButtonPressed()
        {
            ReplayController.Instance.PlayReplay();
            UpdatePlayPauseButtons(true);
        }

        private void OnPauseButtonPressed()
        {
            ReplayController.Instance.Pause();
            UpdatePlayPauseButtons(false);
        }

        private void OnSeekSliderChanged(float value)
        {
            if(!_isUpdatingSlider)
                ReplayController.Instance.Seek(value);
        }

        private void OnSpeedButtonPressed()
        {
            currentSpeedIndex = (currentSpeedIndex + 1) % playbackSpeeds.Length;
            float selectedSpeed = playbackSpeeds[currentSpeedIndex];
            ReplayController.Instance.SetPlaybackSpeed(selectedSpeed);

            UpdateSpeedButtonLabel();
        }

        private void UpdatePlayPauseButtons(bool isPlaying)
        {
            playButton.gameObject.SetActive(!isPlaying);
            pauseButton.gameObject.SetActive(isPlaying);
        }

        private void UpdateSpeedButtonLabel()
        {
            var selectedSpeed = playbackSpeeds[currentSpeedIndex];
            speedButton.GetComponentInChildren<TMP_Text>().text = $"{selectedSpeed}x";
        }
        
        private IEnumerator UpdateSliderCoroutine()
        {
            while (true)
            {
                if (ReplayController.Instance.TotalPlaybackTime > 0)
                {
                    _isUpdatingSlider = true;
                    
                    // Преобразование текущего времени воспроизведения
                    float currentPlaybackTime = ReplayController.Instance.CurrentPlaybackTime;
                    int currentMinutes = (int)(currentPlaybackTime / 60);
                    int currentSeconds = (int)(currentPlaybackTime % 60);
                    string currentTimeString = $"{currentMinutes:D2}:{currentSeconds:D2}";
                    currentTimeText.text = currentTimeString;

                    // Преобразование общего времени воспроизведения
                    float totalPlaybackTime = ReplayController.Instance.TotalPlaybackTime;
                    int totalMinutes = (int)(totalPlaybackTime / 60);
                    int totalSeconds = (int)(totalPlaybackTime % 60);
                    string totalTimeString = $"{totalMinutes:D2}:{totalSeconds:D2}";
                    allTimeText.text = totalTimeString;
                    
                    var progress = ReplayController.Instance.CurrentPlaybackTime / ReplayController.Instance.TotalPlaybackTime;
                    seekSlider.value = progress;
                    _isUpdatingSlider = false;
                }
                yield return null;
            }
        }
    }
}