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
        
        private float _throttle = 0;
        private float _pitch = 0;
        private float _roll = 0;
        private float _yaw = 0;

        public float targetHeight = 1;

        private float controlFl, controlFr, controlRl, controlRr, acceleration;
        
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
                var spawnPoint = GameObject.FindGameObjectWithTag("Respawn").transform;
                _transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
                _rigidBody.linearVelocity = Vector3.zero;
                _rigidBody.angularVelocity = Vector3.zero;
                targetHeight = _transform.position.y;
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
            if (_currentFlightSettings == null)
                _currentFlightSettings = droneSettings.initFlightMode;
            else
            {
                _currentFlightMode++;
                if (_currentFlightMode >= droneSettings.flightModes.Length)
                    _currentFlightMode = 0;

                _currentFlightSettings = droneSettings.flightModes[_currentFlightMode];
                UISubtitle.Instance?.SetTextInstant($"Полетный режим: {_currentFlightSettings.name}", 3);
            }
        }

        private void UpdateEngines()
        {
            if (_currentFlightSettings == null) return;
            
            _rigidBody.freezeRotation = _rigidBody.linearVelocity.magnitude > 1;
            _rigidBody.linearDamping = _rigidBody.linearVelocity.magnitude > 1 ? 0.5f : 0;

            controlFl = (_pitch > 0 ? _pitch : 0) - (_yaw > 0 ? _yaw : 0) - (_roll > 0 ? 0 : -_roll);
            controlFr = (_pitch > 0 ? _pitch : 0) - (_yaw > 0 ? 0 : -_yaw) - (_roll > 0 ? _roll : 0);
            controlRl = (_pitch > 0 ? 0 : -_pitch) - (_yaw > 0 ? 0 : -_yaw) - (_roll > 0 ? 0 : -_roll);
            controlRr = (_pitch > 0 ? 0 : -_pitch) - (_yaw > 0 ? _yaw : 0) - (_roll > 0 ? _roll : 0);
            acceleration = 0.0f;

            if (_currentFlightSettings.throttleType == ControlType.HOLD)
            {
                if (Mathf.Abs(targetHeight - transform.position.y) < 1.5f)
                {
                    targetHeight += _droneInput.Throttle * (_droneInput.Throttle > 0 ? _currentFlightSettings.maxAscendingSpeed : _currentFlightSettings.maxDescendingSpeed) * Time.deltaTime;
                    targetHeight = Mathf.Clamp(targetHeight, 0, _currentFlightSettings.maxHeight);
                }
                
                var speed = (targetHeight > _transform.position.y) ? 0.07f : -0.07f;
                acceleration = _currentFlightSettings.accelerationCurve.Evaluate(0.5f + speed);
                if (targetHeight < 1)
                {
                    acceleration = 0;
                }

                switch (targetHeight - transform.position.y)
                {
                    case > 0 when _droneInput.Throttle < 0.44f:
                    case < 0 when _droneInput.Throttle > 0.56f:
                        //targetHeight = _transform.position.y;
                        _rigidBody.linearVelocity = Vector3.Lerp(_rigidBody.linearVelocity, new Vector3(_rigidBody.linearVelocity.x, Random.Range(-0.2f, 0.2f), _rigidBody.linearVelocity.z), Time.deltaTime * 5);
                        break;
                }
            }
            else
            {
                acceleration = _currentFlightSettings.accelerationCurve.Evaluate(_throttle);
                targetHeight = _transform.position.y;
            }

            engineFL.UpdateEngine(_rigidBody, acceleration, controlFl);
            engineFR.UpdateEngine(_rigidBody, acceleration, controlFr);
            engineRR.UpdateEngine(_rigidBody, acceleration, controlRl);
            engineRL.UpdateEngine(_rigidBody, acceleration, controlRr);
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
                        if (rotationMagnitude > 0.1f)
                        {
                            rotation = Quaternion.Euler(_pitch * _currentFlightSettings.maxStabilizedAngle, eulerAngles.y, -_roll * _currentFlightSettings.maxStabilizedAngle);
                        }
                        else
                        {
                            rotation = Quaternion.Euler(-eulerAngles.x, eulerAngles.y, -eulerAngles.z);
                            _rigidBody.linearVelocity = Vector3.Lerp(linearVelocity, new Vector3(Random.Range(-0.2f, 0.2f), linearVelocity.y, Random.Range(-0.2f, 0.2f)), Time.deltaTime * 5);
                        }

                        _transform.Rotate(new Vector3(0, _yaw, 0) * (_currentFlightSettings.maxAngularSpeed * Time.deltaTime), Space.Self);
                        _transform.rotation = Quaternion.Lerp(_transform.rotation, rotation, Time.deltaTime * 5);
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