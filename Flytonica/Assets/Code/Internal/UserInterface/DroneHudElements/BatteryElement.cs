using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.DroneHudElements
{
    public class BatteryElement : MonoBehaviour
    {
        [SerializeField] private TMP_Text voltageText;
        [SerializeField] private List<Image> elements;
        [SerializeField] private Color activeColor = Color.white, inactiveColor = Color.gray, errorColor = Color.red;
        [Range(0, 1)] [SerializeField] private float errorPercent = 0.1f;
        
        public void SetСharge(float value)
        {
            value = Mathf.Clamp01(value);
            var activeElementsCount = Mathf.RoundToInt(value * elements.Count);

            for (int i = 0; i < elements.Count; i++)
            {
                if (i < activeElementsCount)
                {
                    elements[i].color = activeColor;
                }
                else
                {
                    elements[i].color = inactiveColor;
                }
            }

            if (value < 0.1f && elements.Count > 0)
            {
                elements[0].color = errorColor;
            }
        }

        public void SetVoltage(float value) => voltageText.text = value.ToString();
    }
}