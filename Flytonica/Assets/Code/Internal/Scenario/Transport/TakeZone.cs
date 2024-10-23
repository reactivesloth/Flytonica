using UnityEngine;
using Code.Internal.Drone; // Добавляем этот неймспейс для доступа к DroneController

namespace Code.Internal.Scenario.Transport
{
    public class TakeZone : MonoBehaviour
    {
        [SerializeField] private CargoObject spawnedCargoPrefab;
        [SerializeField] private Transform cargoSpawnPoint;
        [SerializeField] private byte connectionId;

        public CargoObject cargoObject;

        public float stationaryTime = 0f;
        private const float requiredStationaryTime = 2f; // Время неподвижности
        private const float velocityThreshold = 0.5f; // Порог скорости

        private bool droneInZone = false;

        public byte ConnectionId => connectionId;
        
        private void Update()
        {
            if (!droneInZone || cargoObject.State != CargoState.NotAttached)
                return;

            if (DroneController.Instance == null || DroneController.Instance.DroneCargoController == null)
                return;

            var droneRigidbody = DroneController.Instance.GetComponent<Rigidbody>();

            if (droneRigidbody == null)
                return;

            var droneVelocity = droneRigidbody.linearVelocity.magnitude;

            print($"Drone velocity: {droneVelocity}");
            if (droneVelocity < velocityThreshold)
            {
                stationaryTime += Time.deltaTime;

                if (stationaryTime >= requiredStationaryTime)
                {
                    AttachCargoToDrone();
                    stationaryTime = 0f;
                }
            }
            else
            {
                stationaryTime = 0f;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out DroneCargoController _))
            {
                print("DRONE");
                droneInZone = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out DroneCargoController _))
            {
                droneInZone = false;
                stationaryTime = 0f;
            }
        }

        public CargoObject SpawnCargo()
        {
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
                return;
            var cargoRigidbody = cargoObject.GetComponent<Rigidbody>();
            if (cargoRigidbody == null)
                return;
            if(cargoObject.State == CargoState.Delivered)
                return;

            DroneController.Instance.DroneCargoController.Attach(cargoRigidbody);
            cargoObject.OnAttach();
        }

        public void OnReset()
        {
            cargoObject.transform.position = cargoSpawnPoint.position;
            cargoObject.transform.rotation = cargoSpawnPoint.rotation;
        }
    }
}
