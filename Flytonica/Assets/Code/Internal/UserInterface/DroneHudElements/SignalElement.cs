using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.DroneHudElements
{
    public class SignalElement : MonoBehaviour
    {
        [SerializeField] private List<Image> elements;
        [SerializeField] private Color activeColor = Color.white, inactiveColor = Color.gray, errorColor = Color.red;
        [Range(0, 1)] [SerializeField] private float errorPercent = 0.1f;
        
        public void SetSignal(float value)
        {
            value = Mathf.Clamp01(value);
            var activeElementsCount = Mathf.RoundToInt(value * elements.Count);

            for (int i = 0; i < elements.Count; i++)
                elements[i].color = i < activeElementsCount ? activeColor : inactiveColor;
            

            if (value < errorPercent && elements.Count > 0)
                elements[0].color = errorColor;
        }
    }
}
