using System;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

namespace Code.Internal.Drone
{
    public class DroneCamera : NetworkBehaviour
    {
        [SerializeField] private GameObject cameraObject;
        [SerializeField] private GameObject irCameraObject;

        [SerializeField] [Range(-45, 90)] private float currentAngle;
        [SerializeField] [Range(-45, 90)] private float minAngle;
        [SerializeField] [Range(0, 90)] private float maxAngle = 90;

        public event Action<Vector3, Quaternion> OnCameraDataUpdated;

        private void Awake()
        {
            irCameraObject.SetActive(false);
        }

        private void Update()
        {
            if (!IsOwner || !IsSpawned)
                return;

            TransmitCameraTransform(Owner);

            if (DroneInput.Instance && cameraObject.activeSelf != DroneInput.Instance.DroneCam)
                cameraObject.SetActive(DroneInput.Instance.DroneCam);

            if(!DroneInput.Instance)
                return;

            currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

            currentAngle -= Time.deltaTime * UnityEngine.Input.GetAxis("Mouse ScrollWheel") * 1000;

            var rot = cameraObject.transform.localRotation;
            rot = Quaternion.Euler(currentAngle, rot.y, rot.z);
            cameraObject.transform.localRotation = rot;
        }

        public void SetIrMode()
        {
            var isIrModeNow = irCameraObject.activeSelf;
            irCameraObject.SetActive(!isIrModeNow);
            DroneCameraEffectController.Instance.SetIrMode(!isIrModeNow);
        }

        [ServerRpc]
        private void TransmitCameraTransform(NetworkConnection sender)
        {
            var position = cameraObject.transform.position;
            var rotation = cameraObject.transform.rotation;

            OnCameraDataUpdated?.Invoke(position, rotation);
        }
    }
}