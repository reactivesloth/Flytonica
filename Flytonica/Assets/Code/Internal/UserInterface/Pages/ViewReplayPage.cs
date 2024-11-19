using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Code.Internal.API;
using Code.Internal.API.Wrappers.ReceiveModels;
using Code.Internal.Drone;
using Code.Internal.Replays;
using TMPro;
using UltimateReplay;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class ViewReplayPage : Page
    {
        [SerializeField] private Button mainMenuButton, toStudentsButton, mapButton, nextButton;

        [SerializeField] private RectTransform playZone;
        [SerializeField] private Button playButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Slider seekSlider;
        [SerializeField] private Button speedButton;
        [SerializeField] private TMP_Text currentTimeText, allTimeText, nameText;
        [SerializeField] private Button backSeekButton, forwardSeekButton;

        [SerializeField] private float[] playbackSpeeds = { 0.5f, 1f, 1.5f, 2f };
        [SerializeField] private int currentSpeedIndex = 1;

        [Header("Players")] [SerializeField] private TMP_Text playersNamesText;
        [SerializeField] private TMP_Text replayOwnerText;

        private bool _isUpdatingSlider;
        private Vector2 originalAnchoredPosition;
        private Vector2 originalSizeDelta;

        private void OnEnable()
        {
            nextButton.onClick.AddListener(OnNext);
            mapButton.onClick.AddListener(OnMap);

            playButton.onClick.AddListener(OnPlayButtonPressed);
            pauseButton.onClick.AddListener(OnPauseButtonPressed);
            seekSlider.onValueChanged.AddListener(OnSeekSliderChanged);
            speedButton.onClick.AddListener(OnSpeedButtonPressed);

            backSeekButton.onClick.AddListener(BackwardSeek);
            forwardSeekButton.onClick.AddListener(ForwardSeek);

            UpdatePlayPauseButtons(true);
            UpdateSpeedButtonLabel();

            ReplayController.Instance.PlaybackFinished += OnPauseButtonPressed;

            if (DroneHUD.Instance != null)
            {
                RectTransform gameUi = DroneHUD.Instance.gameUi;

                originalAnchoredPosition = gameUi.anchoredPosition;
                originalSizeDelta = gameUi.sizeDelta;

                gameUi.anchoredPosition = playZone.anchoredPosition;
                gameUi.sizeDelta = playZone.sizeDelta;
            }
        }

        private void OnMap()
        {
            FindAnyObjectByType<bl_MiniMap>()?.m_Canvas?.gameObject.SetActive(true);
        }

        private void OnNext() => PlaybackReplayBehaviour.Instance.ChangeCamera();

        private void Update()
        {
            if (!ReplayManager.IsReplayingAny) return;
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

                var progress = ReplayController.Instance.CurrentPlaybackTime /
                               ReplayController.Instance.TotalPlaybackTime;
                seekSlider.value = progress;
                _isUpdatingSlider = false;
            }
        }

        protected override void OnBackClick()
        {
            if (ReplayController.Instance.IsSceneTransitioning)
                return;
            base.OnBackClick();
        }

        private void OnDisable()
        {
            nextButton.onClick.RemoveListener(OnNext);
            mapButton.onClick.AddListener(OnMap);

            playButton.onClick.RemoveListener(OnPlayButtonPressed);
            pauseButton.onClick.RemoveListener(OnPauseButtonPressed);
            seekSlider.onValueChanged.RemoveListener(OnSeekSliderChanged);
            speedButton.onClick.RemoveListener(OnSpeedButtonPressed);

            backSeekButton.onClick.RemoveListener(BackwardSeek);
            forwardSeekButton.onClick.RemoveListener(ForwardSeek);

            ReplayController.Instance.PlaybackFinished -= OnPauseButtonPressed;

            if (DroneHUD.Instance != null)
            {
                RectTransform gameUi = DroneHUD.Instance.gameUi;
                gameUi.anchoredPosition = originalAnchoredPosition;
                gameUi.sizeDelta = originalSizeDelta;
            }
        }

        public void Init(LogData logData)
        {
            var usedId = logData.user_scenario_id >= 0 ? logData.user_scenario_id : logData.replay_id;
            var fileName = $"{usedId}.replay";

            var filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);
            SetOwnerName(logData.user_name);

            nameText.text = logData.scenario_name;

            if (System.IO.File.Exists(filePath))
            {
                ReplayController.Instance.StartPlayback(usedId);
            }
            else
            {
                HttpClient.GetBinary(LinkConstants.GetFile(logData.replay_file_path), onSuccess: bytes =>
                {
                    System.IO.File.WriteAllBytes(filePath, bytes);
                    ReplayController.Instance.StartPlayback(usedId);
                });
            }
        }

        public void Init(LocalLogData logData)
        {
            nameText.text = logData.metadata.ReplayName;
            SetOwnerName(logData.metadata.studentName);

            if (System.IO.File.Exists(logData.path))
            {
                ReplayController.Instance.StartPlayback(logData.path);
            }
        }

        public void SetPlayerList(List<DroneController> drones)
        {
            var text = new StringBuilder();
            text.Append("Игроки в сессии:");

            for (var i = 0; i < drones.Count; i++)
            {
                var replayBeh = drones[i].GetComponent<DroneReplayBehaviour>();
                text.Append(replayBeh.PlayerName);
                text.Append(i < drones.Count - 1 ? ", " : ".");
            }

            playersNamesText.text = text.ToString();
        }


        public void SetOwnerName(string ownerName) => replayOwnerText.text = $"Реплей принадлежит игроку {ownerName}";

        protected override void OnClose()
        {
            ReplayController.Instance.StopPlayback();
            base.OnClose();
        }

        private void OnPlayButtonPressed()
        {
            if (ReplayController.Instance.IsSceneTransitioning) return;
            ReplayController.Instance.PlayReplay();
            UpdatePlayPauseButtons(true);
        }

        private void OnPauseButtonPressed()
        {
            if (ReplayController.Instance.IsSceneTransitioning) return;
            ReplayController.Instance.Pause();
            UpdatePlayPauseButtons(false);
        }

        private void OnSeekSliderChanged(float value)
        {
            if (ReplayController.Instance.IsSceneTransitioning) return;
            if (!_isUpdatingSlider)
                ReplayController.Instance.Seek(value);
        }

        private void OnSpeedButtonPressed()
        {
            if (ReplayController.Instance.IsSceneTransitioning) return;
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


        private void BackwardSeek() => ReplayController.Instance.FastForward(-10);
        private void ForwardSeek() => ReplayController.Instance.FastForward(10);
    }
}