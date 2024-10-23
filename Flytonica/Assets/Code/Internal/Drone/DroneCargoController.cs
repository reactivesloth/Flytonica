using System;
using UnityEngine;
using Code.Internal.Scenario.Transport; // Добавьте этот неймспейс

namespace Code.Internal.Drone
{
    public class DroneCargoController : MonoBehaviour
    {
        [SerializeField] private Rigidbody rigidbody;
        [SerializeField] private Transform attachPoint;
        
        private Rigidbody _currentCargo;

        public bool IsCargoAttached => _currentCargo != null;

        // Переменные для детектирования зоны и таймера
        private TakeZone currentTakeZone = null;
        private float stationaryTime = 0f;
        private const float requiredStationaryTime = 2f; // Требуемое время неподвижности
        private const float velocityThreshold = 0.5f; // Порог скорости для определения неподвижности

        private void OnValidate()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (!DroneController.Instance)
                return;

            if (DroneInput.Instance.DropCargoButton)
                Drop();

            /*// Проверяем, находимся ли над TakeZone и не прикреплен ли уже груз
            if (currentTakeZone != null && !IsCargoAttached)
            {
                // Проверяем скорость дрона
                Rigidbody droneRigidbody = DroneController.Instance.GetComponent<Rigidbody>();
                if (droneRigidbody.linearVelocity.magnitude < velocityThreshold)
                {
                    stationaryTime += Time.deltaTime;
                    if (stationaryTime >= requiredStationaryTime)
                    {
                        AttachCargoFromCurrentZone();
                        stationaryTime = 0f; // Сбрасываем таймер
                    }
                }
                else
                {
                    stationaryTime = 0f; // Сбрасываем таймер, если дрон двигается
                }
            }*/
        }

        /*private void OnTriggerEnter(Collider other)
        {
            // Проверяем, является ли объект TakeZone
            if (other.TryGetComponent(out TakeZone takeZone))
            {
                currentTakeZone = takeZone;
            }
        }*/

        /*private void OnTriggerExit(Collider other)
        {
            // Если вышли из текущей TakeZone, сбрасываем данные
            if (other.TryGetComponent(out TakeZone takeZone))
            {
                if (takeZone == currentTakeZone)
                {
                    currentTakeZone = null;
                    stationaryTime = 0f;
                }
            }
        }*/

        public void Attach(Rigidbody cargo)
        {
            if (!cargo || IsCargoAttached)
                return;

            if(attachPoint)
            {
                cargo.transform.position = attachPoint.position;
                cargo.transform.rotation = attachPoint.rotation;
            }
            
            _currentCargo = cargo;
            var joint = cargo.gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = rigidbody;
        }

        /*private void AttachCargoFromCurrentZone()
        {
            if (currentTakeZone != null && currentTakeZone.cargoObject != null)
            {
                Attach(currentTakeZone.cargoObject.GetComponent<Rigidbody>());
                currentTakeZone.isOn = false; // Отключаем зону, если необходимо
                currentTakeZone = null;
                stationaryTime = 0f;
            }
        }*/

        private void Drop()
        {
            if (!IsCargoAttached)
                return;
            
            var cargo = _currentCargo.GetComponent<CargoObject>();
            var fixedJoint = _currentCargo.GetComponent<FixedJoint>();
            
            if(!cargo || !fixedJoint)
                return;
            
            fixedJoint.connectedBody = null;
            Destroy(fixedJoint);
            cargo.OnDrop();

            _currentCargo = null;
        }

        public void OnReset()
        {
            if (!IsCargoAttached)
                return;
            
            var cargo = _currentCargo.GetComponent<CargoObject>();
            cargo.OnReset();
            Drop();
        }
    }
}
