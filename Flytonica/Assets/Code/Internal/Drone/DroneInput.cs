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

        public bool DroneCanSwitchCam = true;
        public bool DroneCanIrMode = true;
        public bool DroneCam = false;
        public bool DroneIrMode = false;
        public bool DroneMode = false;
        public bool DISARM = true;
        public bool RestartButton = false;

        public bool UseInput = true;
        public float InputSignalLevel = 1f;

        private Player _player;
        private Joystick _findJoystick;
        private Joystick _joystick;

        [SerializeField] private float maxDelayTime = 1.0f;
        [SerializeField] private int bufferSize = 120; 

        private float[] _throttleBuffer;
        private float[] _yawBuffer;
        private float[] _pitchBuffer;
        private float[] _rollBuffer;
        private float[] _timeBuffer;

        private int bufferIndex = 0;

        public static DroneInput Instance { get; private set; }
        
        private void Awake()
        {
            
            _player = ReInput.players.GetPlayer(0);

            _throttleBuffer = new float[bufferSize];
            _yawBuffer = new float[bufferSize];
            _pitchBuffer = new float[bufferSize];
            _rollBuffer = new float[bufferSize];
            _timeBuffer = new float[bufferSize];
        }

        private void Update()
        {
            if(!IsOwner)
                return;
            
            if (Instance == null)
                Instance = this;
            
            if (UseInput && !(InputSignalLevel <= 0))
            {
                if (_player.controllers.joystickCount > 0)
                    UpdateJoystick();

                var rawThrottle = _player.GetAxis("Throttle");
                var rawYaw = _player.GetAxis("Yaw");
                var rawPitch = _player.GetAxis("Pitch");
                var rawRoll = _player.GetAxis("Roll");

                _throttleBuffer[bufferIndex] = rawThrottle;
                _yawBuffer[bufferIndex] = rawYaw;
                _pitchBuffer[bufferIndex] = rawPitch;
                _rollBuffer[bufferIndex] = rawRoll;
                _timeBuffer[bufferIndex] = Time.time;

                var delayTime = (1f - Mathf.Clamp01(InputSignalLevel)) * maxDelayTime;

                var delaySteps = Mathf.RoundToInt(delayTime / Time.deltaTime);
                delaySteps = Mathf.Clamp(delaySteps, 0, bufferSize - 1);

                var delayedIndex = (bufferIndex - delaySteps + bufferSize) % bufferSize;

                Throttle = _throttleBuffer[delayedIndex];
                Yaw = _yawBuffer[delayedIndex];
                Pitch = _pitchBuffer[delayedIndex];
                Roll = _rollBuffer[delayedIndex];

                bufferIndex = (bufferIndex + 1) % bufferSize;

                DroneMode = _player.GetButtonDown("DroneMode") || changeModeAction.action.WasPressedThisFrame();

                if (Throttle < -0.9f && DISARM)
                {
                    DISARM = false;
                }

                DISARM = !_player.GetButton("DISARM");
            }
            
            if (!Application.isFocused) return;

            if (DroneCanSwitchCam)
            {
                if (_player.GetButtonDown("DroneCamera") || changeCameraAction.action.WasPressedThisFrame())
                    DroneCam = !DroneCam;
            }

            if (DroneCanIrMode)
            {
                DroneIrMode = _player.GetButtonDown("IR Mode");
            }

            RestartButton = _player.GetButtonDown("DroneRestart") || restartAction.action.WasPressedThisFrame();
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
        }
    }
}