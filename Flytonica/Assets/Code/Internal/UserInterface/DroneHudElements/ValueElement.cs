using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.DroneHudElements
{
    public class ValueElement : MonoBehaviour
    {
        [Header("Values: ")] [SerializeField] private int maxValue = 100;
        [SerializeField] private Color normalColor, warningColor, extremeColor;
        [Range(0, 1)] [SerializeField] private float warningPercent, extremePersent;

        [Header("Elements: ")] [SerializeField]
        private TMP_Text valueText;

        [SerializeField] private Slider slider;

        private Graphic _sliderFill;

        public int MaxValue
        {
            get => maxValue;
            set => maxValue = value;
        }

        private void Awake()
        {
            _sliderFill = slider.fillRect.GetComponent<Graphic>();
        }

        public void Set(float value)
        {
            var percent = value / maxValue;
            SetColor(percent);
            slider.value = percent;
            valueText.text = value.ToString("F0");
        }

        private void SetColor(float percent)
        {
            if (percent >= extremePersent)
                valueText.color = _sliderFill.color = extremeColor;
            else if (percent >= warningPercent)
                valueText.color = _sliderFill.color = warningColor;
            else
                valueText.color = _sliderFill.color = normalColor;
        }
    }
}