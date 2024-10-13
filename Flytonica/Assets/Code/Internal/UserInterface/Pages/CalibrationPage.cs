using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class CalibrationPage: Page
    {
        [SerializeField] private Button nextButton, recolibrateButton, saveButton;
        [SerializeField] private GameObject calibrating, endCalibration;
        [SerializeField] private List<Step> steps;
        [SerializeField] private Slider gasSlider, rotateSlider, pitchSlider, rollSlider;

        private int _currentStep = 0;

        private void Update()
        {
            //var player = ReInput.players.GetPlayer(0);

            // gasSlider.value = player.GetAxis("Throttle");
            // rotateSlider.value = player.GetAxis("Yaw");
            // pitchSlider.value = player.GetAxis("Pitch");
            // rollSlider.value = player.GetAxis("Roll");
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            
            _currentStep = 0;
            SetCurrentStep();

            calibrating.SetActive(true);
            endCalibration.SetActive(false);
        }

        protected override void OnClose()
        {
            base.OnClose();
        }

        private void NextStep(int doneStep)
        {
            _currentStep = doneStep + 1;
            SetCurrentStep();
        }

        private void SetCurrentStep()
        {
            steps.ForEach(s => s.Hide());
            steps[_currentStep].Show();
            
            calibrating.SetActive(!steps[_currentStep].last);
            endCalibration.SetActive(steps[_currentStep].last);
            
            nextButton.gameObject.SetActive(!steps[_currentStep].last);
            recolibrateButton.gameObject.SetActive(steps[_currentStep].last);
            saveButton.gameObject.SetActive(steps[_currentStep].last);
        }
    }

    [Serializable]
    public struct Step
    {
        public GameObject step, help;
        public bool last;

        public void Show()
        {
            if (help != null)
                help.SetActive(true);
        }

        public void Hide()
        {
            if (help != null)
                help.SetActive(false);
        }
    }
}