using System;
using System.Collections.Generic;
using Code.Internal.Input;
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
            //var calibration = Calibration.Instance;
            // gasSlider.value = calibration._joystick.GetAxis(0);
            // rotateSlider.value = calibration._joystick.GetAxis(1);
            // pitchSlider.value = calibration._joystick.GetAxis(2);
            // rollSlider.value = calibration._joystick.GetAxis(3);
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            //Calibration.Instance.StepDone += NextStep;
            
            _currentStep = 0;
            SetCurrentStep();
            //Calibration.Instance.StartCalibration();
            calibrating.SetActive(true);
            endCalibration.SetActive(false);
        }

        protected override void OnClose()
        {
            base.OnClose();
            //Calibration.Instance.StepDone -= NextStep;
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
            help?.SetActive(true);
        }

        public void Hide()
        {
            help?.SetActive(false);
        }
    }
}