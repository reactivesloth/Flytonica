using System;
using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Object;
using Unity.VisualScripting;
using Code.Internal.Input;
using Code.Internal.UserInterface;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

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
        
        private float _throttle = 0;
        private float _pitch = 0;
        private float _roll = 0;
        private float _yaw = 0;

        private float controlFl, controlFr, controlRl, controlRr, acceleration;

        [Range(2.4f, 3.8f)]
        public float currentVoltage = 3.8f;

        public DroneSettings Settings => droneSettings;
        
        public static DroneController Instance { get; private set; }

        private float batteryLevelPercent = 1;
        private float deltaSpd;
        private float throttleHold;
        
        public DroneSensors DroneSensors { get; private set; }

        protected override void OnValidate()
        {
            _rigidBody = GetComponent<Rigidbody>();
            InitializeDrone();
        }
        
        private void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _transform = GetComponent<Transform>();
            DroneSensors = GetComponent<DroneSensors>();

            InitializeDrone();
        }

        private void Start()
        {
            _droneInput = DroneInput.Instance;
        }

        private void InitializeDrone()
        {
            //UpdateFlightMode();
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
            print(_rigidBody);
            _rigidBody.isKinematic = !IsOwner;
        }

        private void Update()
        {
            if(!_droneInput.IsOwner)
                return;
            
            if (Instance == null)
            {
                Instance = this;
            }
            
            // if (Calibration.Instance.IsCalibrating)
            //     return;

            UpdateInput();
            
            if (_currentFlightSettings == null)
                _currentFlightSettings = droneSettings.currentFlightMode;
            
            if (_droneInput.DroneMode)
            {
                UpdateFlightMode();
            }

            if (_droneInput.RestartButton)
            {
                ResetDrone();
            }
            
            UpdateRotation();
        }

        public void ResetDrone()
        {
            var spawnPoint = GameObject.FindGameObjectWithTag("Respawn").transform;
            _transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            _rigidBody.linearVelocity = Vector3.zero;
            _rigidBody.angularVelocity = Vector3.zero;
        }

        private void FixedUpdate()
        {
            UpdateEngines();
        }

        private void UpdateInput()
        {
            _droneInput.InputSignalLevel = DroneSensors.InputSignal;
            _throttle = (_droneInput.Throttle + 1) / 2;
            _pitch = _droneInput.Pitch;
            _roll = _droneInput.Roll;
            _yaw = _droneInput.Yaw;
        }

        private void UpdateFlightMode()
        {
            if (_currentFlightSettings == null)
                _currentFlightSettings = droneSettings.currentFlightMode;
            else
            {
                _currentFlightMode++;
                if (_currentFlightMode >= droneSettings.flightModes.Count)
                    _currentFlightMode = 0;

                _currentFlightSettings = droneSettings.flightModes[_currentFlightMode];
                droneSettings.currentFlightMode = _currentFlightSettings;
            }
        }

        private void UpdateEngines()
        {
            if (_currentFlightSettings == null) return;

            _rigidBody.freezeRotation = _rigidBody.linearVelocity.magnitude > 1;
            _rigidBody.linearDamping = _rigidBody.linearVelocity.magnitude > 0.2f ? 0.5f : 0;

            controlFl = (_pitch > 0 ? _pitch : 0) - (_yaw > 0 ? _yaw : 0) - (_roll > 0 ? 0 : -_roll);
            controlFr = (_pitch > 0 ? _pitch : 0) - (_yaw > 0 ? 0 : -_yaw) - (_roll > 0 ? _roll : 0);
            controlRl = (_pitch > 0 ? 0 : -_pitch) - (_yaw > 0 ? 0 : -_yaw) - (_roll > 0 ? 0 : -_roll);
            controlRr = (_pitch > 0 ? 0 : -_pitch) - (_yaw > 0 ? _yaw : 0) - (_roll > 0 ? _roll : 0);
            acceleration = Mathf.Clamp(acceleration, 0, 1);
            
            if (_currentFlightSettings.throttleType == ControlType.HOLD)
            { 
                var landingGear = Physics.Raycast(_transform.position, Vector3.down, out _, 1);
                acceleration += _droneInput.Throttle switch
                {
                    > 0.2f when _rigidBody.linearVelocity.y < _currentFlightSettings.maxAscendingSpeed => 0.1f,
                    < -0.2f when _rigidBody.linearVelocity.y > (landingGear ? -1 : -_currentFlightSettings.maxDescendingSpeed) => -0.1f,
                    _ => _rigidBody.linearVelocity.y > 0 ? -0.1f : 0.1f
                };
                acceleration = Mathf.Clamp(acceleration, 0, 1);
            }
            else
            {
                acceleration = _currentFlightSettings.accelerationCurve.Evaluate(_throttle);
            }

            CalculateBattery();

            engineFL.UpdateEngine(_rigidBody, currentVoltage, acceleration, controlFl);
            engineFR.UpdateEngine(_rigidBody, currentVoltage, acceleration, controlFr);
            engineRR.UpdateEngine(_rigidBody, currentVoltage, acceleration, controlRl);
            engineRL.UpdateEngine(_rigidBody, currentVoltage, acceleration, controlRr);
        }

        private void CalculateBattery()
        {
            currentVoltage -= droneSettings.batteryEnergyWh / 3600 / droneSettings.bateteryCellCount * Math.Min(0.1f, acceleration)* Time.deltaTime;
            float batteryLevel = droneSettings.bateteryCellCount * currentVoltage;
            float minBatteryLevel = droneSettings.bateteryCellCount * droneSettings.minBatteryCellVoltage;
            float maxBatteryLevel = droneSettings.bateteryCellCount * droneSettings.maxBatteryCellVoltage;
            batteryLevelPercent = ((batteryLevel - minBatteryLevel) * 100) / (maxBatteryLevel - minBatteryLevel);
        }
        
        private void UpdateRotation()
        {
            if (_currentFlightSettings == null) return;
            
            var rotation = Quaternion.identity;
            var eulerAngles = _transform.eulerAngles;
            var rotationMagnitude = new Vector2(_pitch, _roll).magnitude;
            
            var linearVelocity = _rigidBody.linearVelocity;
            if (linearVelocity.magnitude > 0.01f)
            {
                switch (_currentFlightSettings.rotatingType)
                {
                    case ControlType.STABILIZED:
                    case ControlType.MIXED when
                        rotationMagnitude < _currentFlightSettings.axisModeChangeValue:
                    {
                        rotation = rotationMagnitude > 0.1f ? Quaternion.Euler(_pitch * _currentFlightSettings.maxStabilizedAngle, eulerAngles.y, -_roll * _currentFlightSettings.maxStabilizedAngle) : Quaternion.Euler(-eulerAngles.x, eulerAngles.y, -eulerAngles.z);
                        _transform.Rotate(new Vector3(0, _yaw, 0) * (_currentFlightSettings.maxAngularSpeed * Time.deltaTime), Space.Self);
                        _transform.rotation = Quaternion.Lerp(_transform.rotation, rotation, Time.deltaTime * 5);
                        break;
                    }
                    case ControlType.HOLD:
                        if (rotationMagnitude > 0.1f && _rigidBody.linearVelocity.magnitude < _currentFlightSettings.maxStabilizedSpeed)
                        {
                            rotation = Quaternion.Euler(_pitch * _currentFlightSettings.maxStabilizedAngle, eulerAngles.y, -_roll * _currentFlightSettings.maxStabilizedAngle);
                        }
                        else
                        {
                            rotation = Quaternion.Euler(-eulerAngles.x, eulerAngles.y, -eulerAngles.z);
                            _rigidBody.linearVelocity = Vector3.Lerp(linearVelocity, new Vector3(Random.Range(-0.2f, 0.2f), linearVelocity.y, Random.Range(-0.2f, 0.2f)), Time.deltaTime);
                        }

                        _transform.Rotate(new Vector3(0, _yaw, 0) * (_currentFlightSettings.maxAngularSpeed * Time.deltaTime), Space.Self);
                        _transform.rotation = Quaternion.Lerp(_transform.rotation, rotation, Time.deltaTime);
                        break;
                    default:
                        _transform.Rotate(new Vector3(_pitch, _yaw, -_roll) * (_currentFlightSettings.maxAngularSpeed * Time.deltaTime), Space.Self);
                        break;
                }
            }
        }

        public float GetRPM()
        {
            return (engineFL.GetRPM() + engineFR.GetRPM() + engineRL.GetRPM() + engineRR.GetRPM()) / 4;
        }
        
        public float GetMaxRPM()
        {
            return (engineFL.MaxRPM + engineFR.MaxRPM + engineRL.MaxRPM + engineRR.MaxRPM) / 4;
        }
    }
}