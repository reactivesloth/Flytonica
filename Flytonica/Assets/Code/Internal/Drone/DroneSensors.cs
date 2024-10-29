using System;
using Code.Internal.UserInterface;
using Code.Internal.UserInterface.DroneHudElements;
using UnityEngine;

namespace Code.Internal.Drone
{
    [RequireComponent(typeof(DroneController))]
    public class DroneSensors : MonoBehaviour
    {
        private DroneController _droneController;
        [SerializeField] private Rigidbody _rigidbody;
        //private Transform _transform;

        private float _cameraSignal = 1f;
        private float _inputSignal = 1f;

        private DroneFlightSettings savedFlightSettings;
        private Transform _playerTransform;

        public float CameraSignalModifier { get; set; } = 1f;
        public float InputSignalModifier { get; set; } = 1f;

        public event Action BatteryDepleted;
        public event Action SignalLostDueToPowerLine;
        public event Action SignalLostDueToElectronicWarfare;
        public event Action SignalLostDueToDistance;
        public event Action SignalLostDueToObstacles;
        
        public float CameraSignal => _cameraSignal;
        public float InputSignal => _inputSignal;
        public float Speed => _rigidbody.linearVelocity.magnitude * 3.6f;
        public float Altitude => transform.position.y;
        public float Health { get; set; }

        public float BatteryVoltage => _droneController.Settings.bateteryCellCount * _droneController.currentVoltage;
        public float BatteryLevel
        {
            get
            {
                float minBatteryLevel = _droneController.Settings.bateteryCellCount * _droneController.Settings.minBatteryCellVoltage;
                float maxBatteryLevel = _droneController.Settings.bateteryCellCount * _droneController.Settings.maxBatteryCellVoltage;
                return ((BatteryVoltage - minBatteryLevel) * 100) / (maxBatteryLevel - minBatteryLevel) / 100;
            }
        }

        public float Pitch
        {
            get
            {
                float pitch = -transform.localEulerAngles.x;
                if (pitch > 180)
                    pitch -= 360;
                if (pitch < -180)
                    pitch += 360;
                return pitch;
            }
        }

        public float Roll => transform.localEulerAngles.z;

        public string ModeName => savedFlightSettings?.modeName ?? "";

        private void OnValidate()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Awake()
        {
            _droneController = GetComponent<DroneController>();
            _rigidbody ??= GetComponent<Rigidbody>();
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

            savedFlightSettings = _droneController.Settings.currentFlightMode;

            if (savedFlightSettings != null)
            {
                DroneHUD.Instance.AltValueElement.MaxValue = (int)savedFlightSettings.maxHeight;
                DroneHUD.Instance.SpeedValueElement.MaxValue = (int)savedFlightSettings.maxSpeed;
            }
        }

        private void Update()
        {
            UpdateHud();

            if (DroneController.Instance && DroneHUD.Instance && DroneHUD.Instance.IsShowing())
            {
                DroneHUD.Instance.BatteryElement.SetVoltage(BatteryVoltage);
                DroneHUD.Instance.BatteryElement.SetСharge(BatteryLevel);

                DroneHUD.Instance.CameraSignalElement.SetSignal(CameraSignal);
                DroneHUD.Instance.InputSignalElement.SetSignal(InputSignal);

                DroneHUD.Instance.HealthValueElement.Set(Health);

                if (!string.IsNullOrEmpty(ModeName))
                    DroneHUD.Instance.SetMode(ModeName);
            }

            if (savedFlightSettings != _droneController.Settings.currentFlightMode)
            {
                savedFlightSettings = _droneController.Settings.currentFlightMode;
                DroneHUD.Instance.SetMessage(MessageType.Normal, $"Переключение режима: {ModeName}", 2);
            }

            UpdateSignals();
        }

        public void UpdateHud()
        {
            if (DroneHUD.Instance != null && DroneHUD.Instance.IsShowing())
            {
                DroneHUD.Instance.AltValueElement.Set(Altitude);
                DroneHUD.Instance.SpeedValueElement.Set(Speed);
                
                DroneHUD.Instance.HorizonElement.SetPitch(Pitch);
                DroneHUD.Instance.HorizonElement.SetRoll(Roll);
            }
        }

        private void UpdateSignals()
        {
            var distance = Vector3.Distance(transform.position, _playerTransform.position);
            var maxDistance = _droneController.Settings.maxDistanceInMetres;
            var signalStrength = 1f - distance / maxDistance;

            _cameraSignal = signalStrength * CameraSignalModifier;
            _inputSignal = signalStrength * InputSignalModifier;
        }
    }
}
