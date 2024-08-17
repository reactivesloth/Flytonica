using System;
using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Object;
using Unity.VisualScripting;
using Code.Internal.Input;
using Code.Internal.UserInterface;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Internal.Drone
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class DroneController : NetworkBehaviour
    {
        [SerializeField] private DroneSettings droneSettings;
        
        [SerializeField] private DroneEngine engineFL;
        [SerializeField] private DroneEngine engineFR;
        [SerializeField] private DroneEngine engineRL;
        [SerializeField] private DroneEngine engineRR;
        
        private Transform _transform;
        private Rigidbody _rigidBody;
        
        private DroneInput _droneInput;
        private int _currentFlightMode;
        private DroneFlightSettings _currentFlightSettings;

        [FormerlySerializedAs("_targetHeight")] public float targetHeight = 1;

        private float _throttle = 0;
        private float _pitch = 0;
        private float _roll = 0;
        private float _yaw = 0;

        protected override void OnValidate()
        {
            InitializeDrone();
        }
        
        private void Awake()
        {
            _droneInput = GetComponent<DroneInput>();
            _rigidBody = GetComponent<Rigidbody>();
            _transform = GetComponent<Transform>();

            InitializeDrone();
        }

        private void InitializeDrone()
        {
            UpdateFlightMode();
            InitializeEngines();
            InitializePhysics();
        }

        private void InitializeEngines()
        {
            engineFL.InitializeEngine(droneSettings, true);
            engineFR.InitializeEngine(droneSettings, false);
            engineRL.InitializeEngine(droneSettings, false);
            engineRR.InitializeEngine(droneSettings, true);
        }

        private void InitializePhysics()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _rigidBody.mass = droneSettings.weight;

            var com = Vector3.zero;
            com += engineFL.transform.position;
            com += engineFR.transform.position;
            com += engineRL.transform.position;
            com += engineRR.transform.position;
            com /= 4;
            com.y = 0;
            _rigidBody.centerOfMass = com;

            if (!engineFL.GetComponent<NetworkTransform>())
                engineFL.AddComponent<NetworkTransform>();
            if (!engineFR.GetComponent<NetworkTransform>())
                engineFR.AddComponent<NetworkTransform>();
            if (!engineRL.GetComponent<NetworkTransform>())
                engineRL.AddComponent<NetworkTransform>();
            if (!engineRR.GetComponent<NetworkTransform>())
                engineRR.AddComponent<NetworkTransform>();
        }

        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            base.OnOwnershipClient(prevOwner);
            print($"{_droneInput.Throttle} {_droneInput.Yaw} {_droneInput.Pitch} {_droneInput.Roll}");
            _rigidBody.isKinematic = !IsOwner;
        }

        private void Update()
        {
            if(!_droneInput.IsOwner)
                return;
            if (Calibration.Instance.IsCalibrating)
                return;

            UpdateInput();
            
            if (_droneInput.DroneMode || _currentFlightSettings == null)
            {
                UpdateFlightMode();
            }

            if (_droneInput.RestartButton)
            {
                _transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                _rigidBody.linearVelocity = Vector3.zero;
                _rigidBody.angularVelocity = Vector3.zero;
            }
            
            UpdateRotation();
        }

        private void FixedUpdate()
        {
            UpdateEngines();
        }

        private void UpdateInput()
        {
            _throttle = (_droneInput.Throttle + 1) / 2;
            _pitch = _droneInput.Pitch;
            _roll = _droneInput.Roll;
            _yaw = _droneInput.Yaw;
        }

        private void UpdateFlightMode()
        {
            _currentFlightMode++;
            if (_currentFlightMode >= droneSettings.flightModes.Length)
                _currentFlightMode = 0;
                
            _currentFlightSettings = droneSettings.flightModes[_currentFlightMode];
            UISubtitle.Instance?.SetTextInstant($"Полетный режим: {_currentFlightSettings.name}", 3);
        }

        private void UpdateEngines()
        {
            if (_currentFlightSettings == null) return;
            
            _rigidBody.freezeRotation = _rigidBody.linearVelocity.magnitude > 1;
            _rigidBody.linearDamping = _rigidBody.linearVelocity.magnitude > 1 ? 0.5f : 0;

            var controlFl = (_pitch > 0 ? _pitch : 0) - (_yaw > 0 ? _yaw : 0) - (_roll > 0 ? 0 : -_roll);
            var controlFr = (_pitch > 0 ? _pitch : 0) - (_yaw > 0 ? 0 : -_yaw) - (_roll > 0 ? _roll : 0);
            var controlRl = (_pitch > 0 ? 0 : -_pitch) - (_yaw > 0 ? 0 : -_yaw) - (_roll > 0 ? 0 : -_roll);
            var controlRr = (_pitch > 0 ? 0 : -_pitch) - (_yaw > 0 ? _yaw : 0) - (_roll > 0 ? _roll : 0);
            var acceleration = 0.0f;

            switch (_currentFlightSettings.throttleType)
            {
                case ControlType.MANUAL:
                    acceleration = _currentFlightSettings.accelerationCurve.Evaluate(_throttle);
                    targetHeight = _transform.position.y;
                    break;
                case ControlType.STABILIZED:
                    targetHeight += _droneInput.Throttle * (_droneInput.Throttle > 0 ? _currentFlightSettings.maxAscendingSpeed : _currentFlightSettings.maxDescendingSpeed) * Time.deltaTime;
                    targetHeight = Mathf.Clamp(targetHeight, 0, _currentFlightSettings.maxHeight);
                    
                    var speed = Mathf.Clamp(targetHeight - _transform.position.y, -_currentFlightSettings.maxDescendingSpeed, _currentFlightSettings.maxAscendingSpeed) / 10;
                    acceleration = _currentFlightSettings.accelerationCurve.Evaluate(0.5f + speed);
                    if (targetHeight < 1)
                    {
                        acceleration = 0;
                    }
                    
                    break;
            }
            
            engineFL.UpdateEngine(_rigidBody, acceleration, controlFl);
            engineFR.UpdateEngine(_rigidBody, acceleration, controlFr);
            engineRR.UpdateEngine(_rigidBody, acceleration, controlRl);
            engineRL.UpdateEngine(_rigidBody, acceleration, controlRr);
        }

        private void UpdateRotation()
        {
            if (_currentFlightSettings == null) return;
            
            var rot = _transform.eulerAngles;
            var rotationMagnitude = new Vector2(_pitch, _roll).magnitude;

            if (_rigidBody.linearVelocity.magnitude > 0.1f)
            {
                if (_currentFlightSettings.rotatingType == ControlType.STABILIZED ||
                    (_currentFlightSettings.rotatingType == ControlType.MIXED &&
                     rotationMagnitude < _currentFlightSettings.axisModeChangeValue))
                {
                    _transform.Rotate(
                        new Vector3(0, _yaw, 0) * (_currentFlightSettings.maxAngularSpeed * Time.deltaTime),
                        Space.Self);
                    
                    _transform.rotation = Quaternion.Lerp(_transform.rotation, rotationMagnitude > 0.1f
                        ? Quaternion.Euler(_pitch * _currentFlightSettings.maxStabilizedAngle,
                            rot.y,
                            -_roll * _currentFlightSettings.maxStabilizedAngle)
                        : Quaternion.Euler(-rot.x, rot.y, -rot.z), Time.deltaTime * 5);
                }
                else
                {
                    _transform.Rotate(
                        new Vector3(_pitch, _yaw, -_roll) * (_currentFlightSettings.maxAngularSpeed * Time.deltaTime),
                        Space.Self);
                }
            }
        }
    }
}