using FishNet.Object;
using UnityEngine;

namespace Code.Internal.Drone
{
    public class DroneCamera : NetworkBehaviour
    {
        [SerializeField] private GameObject cameraObject;
        private DroneInput _droneInput;
        
        [SerializeField] [Range(-45,90)] private float currentAngle;
        [SerializeField] [Range(-45, 90)] private float minAngle;
        [SerializeField] [Range(0,90)] private float maxAngle = 90;

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

            currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
            
            currentAngle += Time.deltaTime * UnityEngine.Input.GetAxis("Mouse ScrollWheel") * 100;
            
            var rot = cameraObject.transform.localRotation;
            rot = Quaternion.Euler(currentAngle, rot.y, rot.z);
            cameraObject.transform.localRotation = rot;
        }
    }
}