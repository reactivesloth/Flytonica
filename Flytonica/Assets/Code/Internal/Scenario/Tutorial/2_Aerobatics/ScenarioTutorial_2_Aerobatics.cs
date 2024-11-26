using System;
using System.Collections;
using Code.Internal.Drone;
using UnityEngine;

namespace Code.Internal.Scenario.Tutorial._2_Aerobatics
{
    public class ScenarioTutorial_2_Aerobatics : ScenarioTutorialRoutine
    {
        [SerializeField] private DroneTriggerCallback[] wayPoints;
        [SerializeField] private Material currentMaterial;
        [SerializeField] private Material nextMaterial;
        [SerializeField] private Material defaultMaterial;

        [Header("Repliques")] 
        [SerializeField] private ScenarioTutorialReplique welcomeText;
        [SerializeField] private ScenarioTutorialReplique resetText;
        [SerializeField] private ScenarioTutorialReplique finishText;

        [SerializeField, Tooltip("Максимально допустимое отклонение от линии в метрах")]
        private float maxDeviationDistance = 0.5f;

        [SerializeField, Tooltip("Режим контроля вращения камеры")]
        private CameraViewMode cameraViewMode;

        [SerializeField, Tooltip("Объект, на который должен быть направлен дрон в режиме 'Object'")]
        private Transform targetObject;

        [SerializeField, Tooltip("Допустимая погрешность вращения вокруг оси Y в градусах")]
        private float acceptableRotationError = 5f;

        [SerializeField] private bool isCheckDroneCollision = true;

        private int _currentWayPointIndex;
        private int _nextWayPointIndex;
        private Vector3 _previousWayPointPosition;
        private Vector3 _nextWayPointPosition;

        private bool _isDeviationCheckActive = false;
        private float _initialYRotation;

        public override void Initialize()
        {
            for (var index = 0; index < wayPoints.Length; index++)
            {
                var droneTriggerCallback = wayPoints[index];
                var index1 = index;
                droneTriggerCallback.OnDroneEnter += () => OnEnterWayPoint(index1);
            }

            _previousWayPointPosition = DroneController.Instance.transform.position;

            SetWayPoint(0);

            base.Initialize();
        }

        protected override IEnumerator RunRoutine()
        {
            yield return StartCoroutine(PlayReplique(welcomeText));
            yield return new WaitUntil(() => _currentWayPointIndex == wayPoints.Length); 
            yield return StartCoroutine(PlayReplique(finishText));
            DroneController.Instance.GetComponent<DroneHealthController>().OnObjectCollision -= ResetScenario;
            yield return new WaitUntil(() => DroneController.Instance.EnginesEnabled == false);
            Finish();
            StopAllCoroutines();
        }

        protected override void Update()
        {
            base.Update();
            
            if (!_isDeviationCheckActive) return;

            var dronePosition = DroneController.Instance.transform.position;

            var deviation = CalculateDistanceFromLine(dronePosition, _previousWayPointPosition, _nextWayPointPosition);
            if (deviation > maxDeviationDistance)
            {
                ResetScenario();
                return;
            }

            switch (cameraViewMode)
            {
                case CameraViewMode.None:
                    // Не делаем ничего
                    break;

                case CameraViewMode.Direction:
                    {
                        // Получаем текущий угол вращения вокруг оси Y
                        float currentYRotation = DroneController.Instance.transform.eulerAngles.y;
                        // Вычисляем разницу между текущим углом и начальным
                        float rotationDifference = Mathf.DeltaAngle(currentYRotation, _initialYRotation);
                        if (Mathf.Abs(rotationDifference) > acceptableRotationError)
                        {
                            ResetScenario();
                            return;
                        }
                        break;
                    }

                case CameraViewMode.Object:
                    {
                        if (targetObject == null)
                        {
                            Debug.LogWarning("Target Object is not assigned for CameraViewMode.Object");
                            break;
                        }
                        // Вычисляем направление на целевой объект
                        Vector3 directionToTarget = targetObject.position - dronePosition;
                        directionToTarget.y = 0; // Игнорируем разницу по оси Y
                        if (directionToTarget == Vector3.zero)
                        {
                            // Дрон находится на позиции цели
                            break;
                        }
                        // Ожидаемый угол вращения вокруг оси Y
                        float expectedYRotation = Quaternion.LookRotation(directionToTarget).eulerAngles.y;
                        // Текущий угол вращения дрона
                        float currentYRotation = DroneController.Instance.transform.eulerAngles.y;
                        // Вычисляем разницу углов
                        float rotationDifference = Mathf.DeltaAngle(currentYRotation, expectedYRotation);
                        if (Mathf.Abs(rotationDifference) > acceptableRotationError)
                        {
                            ResetScenario();
                            return;
                        }
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private float CalculateDistanceFromLine(Vector3 point, Vector3 linePoint1, Vector3 linePoint2)
        {
            var lineDirection = linePoint2 - linePoint1;
            var pointToLineStart = point - linePoint1;

            var lineLengthSquared = lineDirection.sqrMagnitude;

            if (lineLengthSquared == 0)
                return Vector3.Distance(point, linePoint1);

            var t = Mathf.Clamp01(Vector3.Dot(pointToLineStart, lineDirection) / lineLengthSquared);
            var projection = linePoint1 + t * lineDirection;

            return Vector3.Distance(point, projection);
        }

        private void ResetScenario()
        {
            StartCoroutine(PlayReplique(resetText));
            _isDeviationCheckActive = false;

            _previousWayPointPosition = DroneController.Instance.transform.position;
            DroneController.Instance.GetComponent<DroneHealthController>().OnObjectCollision -= ResetScenario;

            SetWayPoint(0);
            DroneController.Instance.ResetDrone();
        }

        private void SetWayPoint(int newCurrentIndex)
        {
            for (var i = 0; i < wayPoints.Length; i++)
            {
                SetWaypointMaterial(i, defaultMaterial);
            }

            _currentWayPointIndex = newCurrentIndex;

            if (_currentWayPointIndex >= 0 && _currentWayPointIndex < wayPoints.Length)
            {
                SetWaypointMaterial(_currentWayPointIndex, currentMaterial);

                _nextWayPointPosition = wayPoints[_currentWayPointIndex].transform.position;
            }
            else
            {
                _isDeviationCheckActive = false; // Больше нет вейпоинтов
                return;
            }

            _nextWayPointIndex = _currentWayPointIndex + 1;
            if (_nextWayPointIndex < wayPoints.Length)
                SetWaypointMaterial(_nextWayPointIndex, nextMaterial);
        }

        private void SetWaypointMaterial(int index, Material material)
        {
            if (wayPoints[index].TryGetComponent<Renderer>(out var renderer))
                renderer.material = material;
        }

        private void OnEnterWayPoint(int index)
        {
            if (index != _currentWayPointIndex)
                return;

            _previousWayPointPosition = wayPoints[index].transform.position;

            if (index == 0)
            {
                _isDeviationCheckActive = true;
                
                // Сохраняем начальное вращение дрона в режиме 'Direction'
                if (cameraViewMode == CameraViewMode.Direction)
                {
                    _initialYRotation = DroneController.Instance.transform.eulerAngles.y;
                }
                
                if(isCheckDroneCollision)
                    DroneController.Instance.GetComponent<DroneHealthController>().OnObjectCollision += ResetScenario;
            }

            SetWayPoint(++_currentWayPointIndex);
        }
    }

    public enum CameraViewMode
    {
        None,
        Direction,
        Object
    }
}