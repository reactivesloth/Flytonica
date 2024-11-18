using System;
using System.Collections;
using System.Collections.Generic;
using Code.Internal.Scenario;
using Code.Internal.Scenario.Transport;
using Code.Internal.UserInterface;
using Code.Internal.UserInterface.DroneHudElements;
using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Object;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Internal.Drone
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class DroneController : NetworkBehaviour
    {
        [SerializeField] private DroneSettings droneSettings;

        [FormerlySerializedAs("droneCamera")] [SerializeField] private DroneCameraController droneCameraController;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float pathPointInterval = 1.0f; // Интервал в секундах

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

        private bool _isEnginesOn = false;

        private float controlFl, controlFr, controlRl, controlRr, acceleration;

        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip batteryBeep;
        [Range(2.4f, 3.8f)] public float currentVoltage = 3.8f;

        public DroneSettings Settings => droneSettings;

        /// <summary>
        /// The drone of a local player
        /// </summary>
        public static DroneController Instance { get; private set; }

        public DroneCargoController DroneCargoController { get; private set; }

        private float batteryLevelPercent = 1;
        private float deltaSpd;
        private float throttleHold;

        public bool EnginesEnabled => _isEnginesOn;
        public DroneSensors DroneSensors { get; private set; }

        public float Pitch => _pitch;
        public float Roll => _roll;
        public float Yaw => _yaw;
        public float Throttle => _throttle;
        
        public DroneInput DroneInput => _droneInput;
        public DroneCameraController DroneCameraController => droneCameraController;
        
        protected override void OnValidate()
        {
            droneCameraController = GetComponent<DroneCameraController>();
            _rigidBody = GetComponent<Rigidbody>();
            lineRenderer = GetComponentInChildren<LineRenderer>();
            InitializeDrone();
        }

        private void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _transform = GetComponent<Transform>();
            DroneSensors = GetComponent<DroneSensors>();
            DroneCargoController = GetComponent<DroneCargoController>();

            InitializeDrone();
        }

        private void Start()
        {
            StartCoroutine(AddPointCoroutine());
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

        private void InitializePhysics(Rigidbody cargo = null)
        {
            _rigidBody = GetComponent<Rigidbody>();
            _rigidBody.mass = droneSettings.weight;

            /*
            var com = Vector3.zero;
            com += engineFL.transform.position;
            com += engineFR.transform.position;
            com += engineRL.transform.position;
            com += engineRR.transform.position;
            com /= 4;
            com.y = 0;
            
            _rigidBody.centerOfMass = com;*/

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
            _rigidBody.isKinematic = !IsOwner;
        }

        private void Update()
        {
            if(!IsOwner)
                return;
            
            if (!_droneInput)
                _droneInput = DroneInput.Instance;

            if (!_droneInput || !_droneInput.IsOwner)
                return;

            if (Instance == null)
                Instance = this;

            UpdateInput();

            if (_currentFlightSettings == null)
                _currentFlightSettings = droneSettings.currentFlightMode;

            if (_droneInput.DroneMode)
            {
                UpdateFlightMode();
            }

            if (_droneInput.DroneIrMode)
                droneCameraController.SwitchIrMode();

            if (_droneInput.RestartButton)
            {
                ResetDrone();
            }

            if (!_isEnginesOn && DroneHUD.Instance.MessageBoxElement.IsClear)
            {
                 DroneHUD.Instance.SetMessage(MessageType.Normal, "Для запуска двигателей потяните оба стика вниз и сведите к центру пульта", 0.25f);
            }
        }

        public void ResetDrone()
        {
            ResetEngines();

            DroneCargoController.OnReset();
            
            var spawnPoint = GameObject.FindGameObjectWithTag("Respawn").transform;
            _transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            _rigidBody.linearVelocity = Vector3.zero;
            _rigidBody.angularVelocity = Vector3.zero;
        }

        private void FixedUpdate()
        {
            if (!_droneInput || !_droneInput.IsOwner)
                return;

            UpdateEngines();
            UpdateRotation();
        }

        private void UpdateInput()
        {
            EnginesOn();
            
            _droneInput.InputSignalLevel = DroneSensors.InputSignal;
            
            if(!_isEnginesOn)
                return;
            
            _throttle = (_droneInput.Throttle + 1) / 2;
            _pitch = _droneInput.Pitch;
            _roll = _droneInput.Roll;
            _yaw = _droneInput.Yaw;
            
        }

        private void ResetEngines()
        {
            _throttle = _yaw = _roll = _pitch = 0f;
            engineFL.UpdateEngine(_rigidBody, 0, 0, 0);
            engineFR.UpdateEngine(_rigidBody, 0, 0, 0);
            engineRR.UpdateEngine(_rigidBody, 0, 0, 0);
            engineRL.UpdateEngine(_rigidBody, 0, 0, 0);
            _isEnginesOn = false;
        }

        private void EnginesOn()
        {
            if (_isEnginesOn)
                return;

            var leftStickVector = new Vector2(_droneInput.Yaw, _droneInput.Throttle);
            var rightStickVector = new Vector2(_droneInput.Roll, _droneInput.Pitch);

            // Вычисляем длины векторов
            float leftStickMagnitude = leftStickVector.magnitude;
            float rightStickMagnitude = rightStickVector.magnitude;

            // Проверяем, что длина каждого вектора больше 1
            if (leftStickMagnitude > .9f && rightStickMagnitude > .9f)
            {
                // Вычисляем углы векторов в градусах
                float leftStickAngle = MathF.Atan2(leftStickVector.y, leftStickVector.x) * (180 / MathF.PI);
                float rightStickAngle = MathF.Atan2(rightStickVector.y, rightStickVector.x) * (180 / MathF.PI);

                // Нормализуем углы в диапазон [0, 360)
                leftStickAngle = (leftStickAngle + 360) % 360;
                rightStickAngle = (rightStickAngle + 360) % 360;

                // Целевые углы для нижнего правого и нижнего левого положений
                float leftStickTargetAngle = 315f; // Нижний правый угол
                float rightStickTargetAngle = 225f; // Нижний левый угол
                float angleTolerance = 25f; // Допустимое отклонение в градусах

                // Проверяем, находится ли угол стика в допустимом диапазоне
                bool isLeftStickInPosition = MathF.Abs(DeltaAngle(leftStickAngle, leftStickTargetAngle)) <= angleTolerance;
                bool isRightStickInPosition = MathF.Abs(DeltaAngle(rightStickAngle, rightStickTargetAngle)) <= angleTolerance;

                if (isLeftStickInPosition && isRightStickInPosition)
                {
                    // Условия выполнены
                    _isEnginesOn = true;
                }
            }
        }

        // Функция для вычисления минимальной разницы между двумя углами
        private float DeltaAngle(float current, float target)
        {
            float delta = (target - current + 180) % 360 - 180;
            return delta < -180 ? delta + 360 : delta;
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
            if (_droneInput.KILLSWITCH)
            {
                ResetEngines();
                return;
            }
            
            if (_currentFlightSettings == null || !_isEnginesOn) return;

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
                    < -0.2f when _rigidBody.linearVelocity.y >
                                 (landingGear ? -1 : -_currentFlightSettings.maxDescendingSpeed) => -0.1f,
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
            
            if (_throttle < 0.05f && DroneSensors.GetHeightFromFloor() < 1)
                ResetEngines();
        }

        private void CalculateBattery()
        {
            currentVoltage -= droneSettings.batteryEnergyWh / 3600 / droneSettings.bateteryCellCount *
                              Math.Min(0.1f, acceleration) * Time.deltaTime;
            float batteryLevel = droneSettings.bateteryCellCount * currentVoltage;
            float minBatteryLevel = droneSettings.bateteryCellCount * droneSettings.minBatteryCellVoltage;
            float maxBatteryLevel = droneSettings.bateteryCellCount * droneSettings.maxBatteryCellVoltage;
            batteryLevelPercent = ((batteryLevel - minBatteryLevel) * 100) / (maxBatteryLevel - minBatteryLevel);   
            
            if(batteryLevelPercent is >= 1 and <= 11) {
                DroneHUD.Instance.SetMessage(MessageType.Warning,"Обратите внимание: низкий уровень заряда батареи", 5f);
                if (batteryBeep != null && sfxSource != null)
                {
                    if (!sfxSource.isPlaying)
                        sfxSource.PlayOneShot(batteryBeep);
                }
            }
            if(batteryLevelPercent <= 0f)
            {
                DroneSensors.InputSignal = 0;
                DroneSensors.CameraSignal = 0;
                DroneHUD.Instance.SetMessage(MessageType.Error, "Батарея разряжена, связь с квадрокоптером потеряна");
            }
        }

        private void UpdateRotation()
        {
            if (_currentFlightSettings == null || !_isEnginesOn) return;

            Quaternion rotation;
            var eulerAngles = _transform.eulerAngles;
            var rotationMagnitude = new Vector2(_pitch, _roll).magnitude;

            switch (_currentFlightSettings.rotatingType)
            {
                case ControlType.STABILIZED:
                case ControlType.MIXED when
                    rotationMagnitude < _currentFlightSettings.axisModeChangeValue:
                {
                    rotation = rotationMagnitude > 0.01f
                        ? Quaternion.Euler(_pitch * _currentFlightSettings.maxStabilizedAngle, eulerAngles.y,
                            -_roll * _currentFlightSettings.maxStabilizedAngle)
                        : Quaternion.Euler(0, eulerAngles.y, 0);
                    _transform.Rotate(
                        new Vector3(0, _yaw, 0) * (_currentFlightSettings.maxAngularSpeed * Time.deltaTime),
                        Space.Self);
                    _transform.rotation = Quaternion.Lerp(_transform.rotation, rotation, Time.deltaTime * 5);
                    break;
                }
                case ControlType.HOLD:
                    if (rotationMagnitude > 0.1f &&
                        _rigidBody.linearVelocity.magnitude < _currentFlightSettings.maxStabilizedSpeed)
                    {
                        rotation = Quaternion.Euler(_pitch * _currentFlightSettings.maxStabilizedAngle, eulerAngles.y,
                            -_roll * _currentFlightSettings.maxStabilizedAngle);
                    }
                    else
                    {
                        rotation = Quaternion.Euler(-eulerAngles.x, eulerAngles.y, -eulerAngles.z);
                    }

                    _transform.Rotate(
                        new Vector3(0, _yaw, 0) * (_currentFlightSettings.maxAngularSpeed * Time.deltaTime),
                        Space.World);
                    _transform.rotation = Quaternion.Lerp(_transform.rotation, rotation, Time.deltaTime);
                    break;
                default:
                    _transform.Rotate(
                        new Vector3(_pitch, _yaw, -_roll) * (_currentFlightSettings.maxAngularSpeed * Time.deltaTime),
                        Space.Self);
                    break;
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

        public void WindEffect(Vector3 direction, float windSpeed)
        {
            if (_rigidBody == null) return;

            // Плотность воздуха при нормальных условиях (кг/м³)
            const float airDensity = 1.225f;

            // Площадь поперечного сечения дрона (м²), можно настроить под реальные данные дрона
            float crossSectionalArea = 0.3f; // Примерное значение, нужно уточнить для вашего дрона

            // Коэффициент аэродинамического сопротивления (для объекта вроде дрона, это может быть в пределах 0.3 - 0.8)
            float dragCoefficient = 0.5f;

            // Вычисляем силу ветра по аэродинамической формуле
            float windForceMagnitude = 0.5f * airDensity * windSpeed * windSpeed * crossSectionalArea * dragCoefficient;

            // Применяем направление ветра
            Vector3 windForce = direction.normalized * windForceMagnitude;

            // Логгируем силу ветра для отладки
            //print($"Wind Force: {windForce}, Wind Speed: {windSpeed}");

            // Применяем силу ветра к Rigidbody дрона
            _rigidBody.AddForce(windForce, ForceMode.Force);
        }
        
        private IEnumerator AddPointCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(pathPointInterval);
                AddPoint();
            }
        }

        private void AddPoint()
        {
            // Получаем текущую позицию и добавляем её в LineRenderer
            Vector3 currentPosition = _transform.position;
            int pointCount = lineRenderer.positionCount;
            lineRenderer.positionCount = pointCount + 1;
            lineRenderer.SetPosition(pointCount, currentPosition);
        }
    }
}