using System;
using System.Collections.Generic;
using Rewired;
using TMPro;
using UnityEngine;

namespace Code.Internal.Input
{
    public class AxesShower : MonoBehaviour
    {
        [SerializeField] private Transform textParent;
        [SerializeField] private TMP_Text _text;
        
        private Joystick _joystick;

        private void Start()
        {
            _joystick = ReInput.controllers.Joysticks[0];
        }

        private void Update()
        {
            if (_joystick == null) return;
            _text.text = string.Empty;
            
            for (var i = 0; i < _joystick.axisCount; i++)
            {
                var axisValue = _joystick.GetAxis(i);
                _text.text += $"\n Axis {_joystick.Axes[i].name} ({i}): {axisValue:F2}";
            }
            
            for (var i = 0; i < _joystick.axisCount; i++)
            {
                var buttonValue = _joystick.GetButton(i);
                _text.text += $"\n Axis {_joystick.Buttons[i].name} ({i}): {buttonValue:F2}";
            }
        }
    }
}