using System;
using Rewired;
using UnityEngine;

namespace Code.Internal.Input
{
    public class JoystickVisual : MonoBehaviour
    {
        public enum Axis
        {
            X,
            Y,
            Z,
            invertX,
            invertY,
            invertZ
        }
        
        [Serializable]
        public class AxisName
        {
            public string controlerName;
            public int pcAxisIndex;
            public int androidAxisIndex;
        }
        
        [Serializable]
        public class JoystickVisualStick
        {
            public Transform transform;
            public Axis axis;
            public AxisName[] axisName;
            public string ReInputAxisName;
            public float deltaMaxAngle;
        }

        [Serializable]
        public class JoystickVisualSwitch
        {
            public Transform transform;
            public Vector3[] rotations;
            public string switchName;
        }
        
        [Serializable]
        public class JoystickVisualButton
        {
            public Transform transform;
            public Vector3 positionPress;
            public Vector3 positionUp;
            public string buttonName;
        }
        
        [SerializeField] private JoystickVisualStick[] sticks;
        [SerializeField] private JoystickVisualSwitch[] switches;
        [SerializeField] private JoystickVisualButton[] buttons;
        
        private void Update()
        {
            foreach (var s in sticks)
            {
                UpdateStick(s);
            }

            foreach (var s in switches)
            {
                UpdateSwitches(s);
            }

            foreach (var b in buttons)
            {
                UpdateButtons(b);
            }
        }

        private void UpdateStick (JoystickVisualStick stick)
        {
            var reInput = ReInput.players.GetPlayer(0);
            var inputStick = reInput.GetAxis(stick.ReInputAxisName);

            var stickRotation = stick.axis switch
            {
                Axis.X => new Vector3(inputStick * stick.deltaMaxAngle, 0, 0),
                Axis.Y => new Vector3(0, inputStick * stick.deltaMaxAngle, 0),
                Axis.Z => new Vector3(0, 0, inputStick * stick.deltaMaxAngle),
                Axis.invertX => new Vector3(-inputStick * stick.deltaMaxAngle, 0, 0),
                Axis.invertY => new Vector3(0, -inputStick * stick.deltaMaxAngle, 0),
                Axis.invertZ => new Vector3(0, 0, -inputStick * stick.deltaMaxAngle),
                _ => throw new ArgumentOutOfRangeException()
            };

            stick.transform.localEulerAngles = stickRotation;
        }

        private void UpdateSwitches(JoystickVisualSwitch switches)
        {
            
        }

        private void UpdateButtons(JoystickVisualButton buttons)
        {
            
        }
    }
}