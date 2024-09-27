using System;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface;
using Code.Internal.UserInterface.DroneHudElements;
using UnityEngine;

namespace Code.Internal.Drone
{
    [RequireComponent(typeof(DroneController))]
    public class DroneSensors : MonoBehaviour
    {
        private DroneController _droneController;
        private Rigidbody _rigidbody;
        private Transform _transform;

        private float _cameraSignal = 1f;
        private float _inputSignal = 1f;

        private DroneFlightSettings savedFlightSettings;
        private Transform _playerTransform;

        public float CameraSignalModifier { get; set; } = 1f;
        public float InputSignalModifier { get; set; } = 1f;

        public float CameraSignal => _cameraSignal;
        public float InputSignal => _inputSignal;

        
        private void Awake()
        {
            _droneController = gameObject.GetComponent<DroneController>();
            _rigidbody = gameObject.GetComponent<Rigidbody>();
            _transform = transform;
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            if (savedFlightSettings != null)
                DroneHUD.Instance.AltValueElement.MaxValue = (int)savedFlightSettings.maxHeight;
        }

        private void Update()
        {
            if (DroneHUD.Instance.IsShowing())
            {
                DroneHUD.Instance.AltValueElement.Set(_transform.position.y);
                DroneHUD.Instance.SpeedValueElement.Set(_rigidbody.linearVelocity.magnitude * 3.6f);

                float batteryLevel = _droneController.Settings.bateteryCellCount * _droneController.currentVoltage;
                float minBatteryLevel = _droneController.Settings.bateteryCellCount *
                                        _droneController.Settings.minBatteryCellVoltage;
                float maxBatteryLevel = _droneController.Settings.bateteryCellCount *
                                        _droneController.Settings.maxBatteryCellVoltage;
                var bLevel = ((batteryLevel - minBatteryLevel) * 100) / (maxBatteryLevel - minBatteryLevel) / 100;

                DroneHUD.Instance.BatteryElement.SetVoltage(batteryLevel);
                DroneHUD.Instance.BatteryElement.SetСharge(bLevel);
                DroneHUD.Instance.HorizonElement.SetPitch(-_transform.localRotation.eulerAngles.x);
                DroneHUD.Instance.HorizonElement.SetRoll(transform.localEulerAngles.z);
                
                DroneHUD.Instance.CameraSignalElement.SetSignal(_cameraSignal);
                DroneHUD.Instance.InputSignalElement.SetSignal(_inputSignal);
                
                if (savedFlightSettings != null)
                    DroneHUD.Instance.SetMode(savedFlightSettings.modeName);
            }

            if (savedFlightSettings != _droneController.Settings.currentFlightMode)
            {
                savedFlightSettings = _droneController.Settings.currentFlightMode;
                DroneHUD.Instance.SetMessage(MessageType.Normal, $"Переключение режима: {savedFlightSettings.modeName}",
                    2);
            }
            
            UpdateSignals();
        }

        private void UpdateSignals()
        {
            _cameraSignal = _inputSignal = 1f - Vector3.Distance(transform.position, _playerTransform.position) /
                _droneController.Settings.maxDistanceInMetres;
            _cameraSignal *= CameraSignalModifier;
            _inputSignal *= InputSignalModifier;
            
            //TODO: Signal effects
        }
    }
}