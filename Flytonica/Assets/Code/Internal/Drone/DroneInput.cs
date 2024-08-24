using FishNet.Object;
using UnityEngine;
using Rewired;
using UnityEngine.InputSystem;
using Joystick = Rewired.Joystick;

namespace Code.Internal.Drone
{
    public class DroneInput : NetworkBehaviour
    {
        [SerializeField] private InputActionReference changeModeAction, changeCameraAction, restartAction;
        
        [Range(-1, 1)] public float Throttle;
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
            
            Throttle = _player.GetAxis("Throttle");
            Yaw = _player.GetAxis("Yaw");
            Pitch = _player.GetAxis("Pitch");
            Roll = _player.GetAxis("Roll");
            
            DroneMode = _player.GetButtonDown("DroneMode") || changeModeAction.action.WasPressedThisFrame();
            if (_player.GetButtonDown("DroneCamera") || changeCameraAction.action.WasPressedThisFrame())
                DroneCam = !DroneCam;

            RestartButton = _player.GetButtonDown("DroneRestart") || restartAction.action.WasPressedThisFrame();
            
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