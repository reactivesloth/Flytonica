using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.DroneHudElements
{
    public class ValueElement : MonoBehaviour
    {
        [SerializeField] private bool invert;
        
        [Header("Values: ")] [SerializeField] private int maxValue = 100;
        [SerializeField] private Color normalColor, warningColor, extremeColor;
        [Range(0, 1)] [SerializeField] private float warningPercent, extremePersent;

        [Header("Elements: ")] [SerializeField]
        private TMP_Text valueText;
        [SerializeField] private Image graphicsIcon;
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
            if (valueText != null)
                valueText.text = value.ToString("F0");
        }

        private void SetColor(float percent)
        {
            if (invert ? percent < 1 - extremePersent :percent >= extremePersent)
            {
                if (valueText != null)
                    valueText.color = extremeColor;
                if (graphicsIcon != null)
                    graphicsIcon.color = extremeColor;
                if (_sliderFill != null)
                    _sliderFill.color = extremeColor;
            }
            else if (invert ? percent < 1 - warningPercent :percent >= warningPercent)
            {
                if (valueText != null)
                    valueText.color = warningColor;
                if (graphicsIcon != null)
                    graphicsIcon.color = warningColor;
                if (_sliderFill != null)
                    _sliderFill.color = warningColor;
            }
            else
            {
                if (valueText != null)
                    valueText.color = normalColor;
                if (graphicsIcon != null)
                    graphicsIcon.color = normalColor;
                if (_sliderFill != null)
                    _sliderFill.color = normalColor;
            }
        }
    }
}