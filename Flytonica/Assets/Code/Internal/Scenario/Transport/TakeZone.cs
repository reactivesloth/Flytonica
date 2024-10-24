using System;
using UnityEngine;
using Code.Internal.Drone;
using Code.Internal.UserInterface;

namespace Code.Internal.Scenario.Transport
{
    public class TakeZone : MonoBehaviour
    {
        [SerializeField] private CargoObject spawnedCargoPrefab;
        [SerializeField] private Transform cargoSpawnPoint;
        [SerializeField] private byte connectionId;

        public CargoObject cargoObject;

        private const float attachmentDuration = 2f; // Время прикрепления
        private const float velocityThreshold = 0.5f; // Порог скорости для определения неподвижности

        private bool droneInZone = false;
        private bool isAttaching = false;
        private float attachmentProgress = 0f;

        public int FallCount { get; private set; } = 0;
        public int CollisionCount { get; private set; } = 0;
        
        public byte ConnectionId => connectionId;

        public int TakeCount { get; private set; } = 0;

        public bool IsDelivery { get; private set; }

        private void Awake()
        {
            FallCount = 0;
            CollisionCount = 0;
        }

        private void Update()
        {
            if (!droneInZone || IsDelivery)
                return;
            if (DroneController.Instance == null || DroneController.Instance.DroneCargoController == null)
                return;
            if(DroneController.Instance.DroneCargoController.IsCargoAttached)
                return;

            var droneRigidbody = DroneController.Instance.GetComponent<Rigidbody>();

            if (droneRigidbody == null)
                return;

            var droneVelocity = droneRigidbody.linearVelocity.magnitude;

            if (isAttaching)
            {
                // Проверяем условия провала
                if (droneVelocity > velocityThreshold)
                {
                    // Дрон начал двигаться слишком быстро, прерываем процесс
                    isAttaching = false;
                    attachmentProgress = 0f;
                    OnFailure();
                    return;
                }

                // Обновляем прогресс прикрепления
                attachmentProgress += Time.deltaTime / attachmentDuration;
                OnProgress(Mathf.Clamp01(attachmentProgress));

                if (attachmentProgress >= 1f)
                {
                    // Прикрепление завершено успешно
                    isAttaching = false;
                    attachmentProgress = 1f;
                    AttachCargoToDrone();
                    OnSuccess();
                }
            }
            else
            {
                // Если дрон неподвижен, начинаем процесс прикрепления
                if (droneVelocity < velocityThreshold)
                {
                    isAttaching = true;
                    attachmentProgress = 0f;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out DroneCargoController _))
            {
                droneInZone = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out DroneCargoController _))
            {
                droneInZone = false;
                isAttaching = false;
                attachmentProgress = 0f;
            }
        }

        public CargoObject SpawnCargo()
        {
            if (cargoObject) return cargoObject;
            
            var cargo = Instantiate(spawnedCargoPrefab, cargoSpawnPoint.position, cargoSpawnPoint.rotation);
            cargo.Init(this);
            cargoObject = cargo;
            return cargo;
        }

        private void AttachCargoToDrone()
        {
            if (DroneController.Instance.DroneCargoController.IsCargoAttached)
                return;
            if (cargoObject == null)
                SpawnCargo();
            var cargoRigidbody = cargoObject.GetComponent<Rigidbody>();
            if (cargoRigidbody == null)
                return;
            if (cargoObject.state == CargoState.Delivered)
                return;

            TakeCount++;
            DroneController.Instance.DroneCargoController.Attach(cargoRigidbody);
            cargoObject.OnAttach();
            cargoObject = null;
        }

        public void OnReset()
        {
            if(!cargoObject) return;
            cargoObject.transform.position = cargoSpawnPoint.position;
            cargoObject.transform.rotation = cargoSpawnPoint.rotation;
        }

        protected void OnProgress(float progress)
        {
            DroneHUD.Instance.AimElement.SetProgressValue(progress);
        }

        protected void OnSuccess()
        {
            DroneHUD.Instance.AimElement.Flash(Color.green, 1);
        }

        protected void OnFailure()
        {
            DroneHUD.Instance.AimElement.Flash(Color.red, 1);
        }

        public void CargoFall()
        {
            print(FallCount);
            FallCount++;
        }

        public void CargoCollision() => CollisionCount++;

        public void Respawn()
        {
            SpawnCargo();
        }

        public void SetDelivery()
        {
            IsDelivery = true;
        }
    }
}
