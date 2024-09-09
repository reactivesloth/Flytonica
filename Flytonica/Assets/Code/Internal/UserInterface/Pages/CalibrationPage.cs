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
        
        protected override void OnBackClick()
        {
            base.OnBackClick();
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            
        }

        protected override void OnClose()
        {
            base.OnClose();
            
        }

        private void NextStep()
        {
            _currentStep++;
            
        }
    }

    [Serializable]
    public struct Step
    {
        public GameObject step, help;
        public bool last;
    }
}