using FishNet.Object;
using UnityEngine;

namespace Code.Internal.Drone
{
    public class DroneCamera : NetworkBehaviour
    {
        [SerializeField] private GameObject cameraObject;
        private DroneInput _droneInput;

        private void Awake()
        {
            _droneInput = GetComponent<DroneInput>();
            cameraObject.SetActive(false);
        }

        private void Update()
        {
            if(!IsOwner)
                return;
            
            if (cameraObject.activeSelf != _droneInput.DroneCam)
                cameraObject.SetActive(_droneInput.DroneCam);
        }
    }
}