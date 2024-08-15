using Code.Internal.Input;
using FishNet.Object;
using UnityEngine;
using Rewired;

namespace Code.Internal.Drone
{
    public enum DroneFlyingMode
    {
        ACRO,
        ANGLE,
        HORIZON,
        ALTHOLD
    }

    public class DroneInput : NetworkBehaviour
    {
        [Range(0, 1)] public float Throttle;
        [Range(-1, 1)] public float Yaw;
        [Range(-1, 1)] public float Pitch;
        [Range(-1, 1)] public float Roll;

        public bool DroneCam = false;
        public bool DroneMode = false;
        public bool DISARM = true;
        public bool RestartButton = false;
        
        public bool UseInput = true;
        
        private Player _player;
        private Joystick _findJoystick;
        private Joystick _joystick;

        private void Awake()
        {
            _player = ReInput.players.GetPlayer(0);
            print(_player);
        }

        private void Update()
        {
            if (!UseInput)
                return;

            if (_player.controllers.joystickCount > 0)
                UpdateJoystick();
            
            if (Calibration.Instance != null)
            {
                if (Calibration.Instance.IsCalibrating)
                    return;
            }
            
            Throttle = _player.GetAxis("Throttle");
            Yaw = _player.GetAxis("Yaw");
            Pitch = _player.GetAxis("Pitch");
            Roll = _player.GetAxis("Roll");
            
            DroneMode = _player.GetButtonDown("DroneMode");
            if (_player.GetButtonDown("DroneCamera"))
                DroneCam = !DroneCam;

            RestartButton = _player.GetButtonDown("DroneRestart");
            
            if (Throttle < -0.9f && DISARM)
            {
                DISARM = false;
            }

            DISARM = !_player.GetButton("DISARM");
        }
        
        private void UpdateJoystick()
        {
            _findJoystick = null;
            
            foreach (var joystick in ReInput.controllers.Joysticks)
            {
                if (joystick.hardwareName.ToLower().Contains("flysky"))
                {
                    _findJoystick = joystick;
                    break;
                }
            }

            if (_findJoystick == null)
                _findJoystick = _player.controllers.Joysticks[0];

            if (_joystick != _findJoystick)
            {
                _joystick = _findJoystick;
                _player.controllers.Joysticks.Clear();
                _player.controllers.Joysticks.Add(_joystick);
            }

            print(_joystick.hardwareName + " " + _joystick.name);
        }
    }
}