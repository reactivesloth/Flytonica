using System;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class CalibrationPage: Page
    {
        [SerializeField] private Button nextButton, recolibrateButton, saveButton;
        [SerializeField] private GameObject calibrating;
        [SerializeField] private Slider gasSlider, rotateSlider, pitchSlider, rollSlider;
        private Player _player;

        private void Start()
        {
            _player = ReInput.players.GetPlayer(0);
        }

        private void Update()
        {
            if (ReInput.players.GetPlayer(0) != null)
            {
                gasSlider.value = _player.GetAxis("Throttle");
                rotateSlider.value = _player.GetAxis("Yaw");
                pitchSlider.value = _player.GetAxis("Pitch");
                rollSlider.value = _player.GetAxis("Roll");
            }
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            
            if (calibrating != null)
                calibrating.SetActive(true);
        }

        protected override void OnClose()
        {
            base.OnClose();
        }
    }
}