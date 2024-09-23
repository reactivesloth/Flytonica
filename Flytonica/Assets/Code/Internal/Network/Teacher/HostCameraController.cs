using Code.Internal.Drone;
using FishNet.Object;
using UnityEngine;

namespace Code.Internal.Network.Teacher
{
    public class HostCameraController : MonoBehaviour
    {
        public static HostCameraController Instance;

        [SerializeField] private Camera hostCamera;

        private NetworkObject targetDrone;
        
        private void Awake()
        {
            Instance = this;
        }

        public void SetTargetDrone(NetworkObject drone)
        {
            // Отписываемся от предыдущего дрона
            if (targetDrone != null)
            {
                var previousDroneCamera = targetDrone.GetComponent<DroneCamera>();
                if (previousDroneCamera != null)
                {
                    previousDroneCamera.OnCameraDataUpdated -= UpdateCameraPosition;
                }
            }

            targetDrone = drone;

            // Подписываемся на новый дрон
            var droneCamera = targetDrone.GetComponent<DroneCamera>();
            droneCamera.OnCameraDataUpdated += UpdateCameraPosition;
        }

        public void UpdateCameraPosition(Vector3 position, Quaternion rotation)
        {
            hostCamera.transform.position = position;
            hostCamera.transform.rotation = rotation;
        }

        private void OnDisable()
        {
            if (targetDrone != null)
            {
                var droneCamera = targetDrone.GetComponent<DroneCamera>();
                if (droneCamera != null)
                {
                    droneCamera.OnCameraDataUpdated -= UpdateCameraPosition;
                }
            }
        }
    }
}