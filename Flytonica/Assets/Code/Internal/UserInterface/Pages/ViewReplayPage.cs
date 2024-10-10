using System;
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
        
        [SerializeField] private float[] playbackSpeeds = { 0.5f, 1f, 1.5f, 2f };
        [SerializeField] private int currentSpeedIndex = 1;

        private void OnEnable()
        {
            playButton.onClick.AddListener(OnPlayButtonPressed);
            pauseButton.onClick.AddListener(OnPauseButtonPressed);
            seekSlider.onValueChanged.AddListener(OnSeekSliderChanged);
            speedButton.onClick.AddListener(OnSpeedButtonPressed);

            UpdatePlayPauseButtons(true);
            UpdateSpeedButtonLabel();
        }

        private void OnDisable()
        {
            playButton.onClick.RemoveListener(OnPlayButtonPressed);
            pauseButton.onClick.RemoveListener(OnPauseButtonPressed);
            seekSlider.onValueChanged.RemoveListener(OnSeekSliderChanged);
            speedButton.onClick.RemoveListener(OnSpeedButtonPressed);
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
    }
    }
}