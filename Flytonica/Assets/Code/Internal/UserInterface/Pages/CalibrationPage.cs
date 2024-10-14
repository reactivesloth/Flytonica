using System;
using Rewired;
using Rewired.UI.ControlMapper;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class CalibrationPage: Page
    {
        [SerializeField] private Button nextButton, recolibrateButton, saveButton;
        [SerializeField] private GameObject calibrating;
        [SerializeField] private Slider gasSlider, rotateSlider, pitchSlider, rollSlider;
        [SerializeField] private ControlMapper controlrMapper;
        private Player _player;

        private void Start()
        {
            _player = ReInput.players.GetPlayer(0);
            controlrMapper = FindAnyObjectByType<ControlMapper>();
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
            if (controlrMapper != null)
                controlrMapper.Open();
            
            if (calibrating != null)
                calibrating.SetActive(true);
        }

        protected override void OnClose()
        {
            base.OnClose();
            controlrMapper.Close(true);
        }
    }
}