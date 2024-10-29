using System;
using FishNet.Connection;
using FishNet.Object;
using UltimateReplay;
using UnityEngine;

namespace Code.Internal.Drone
{
    [ReplayPreparerIgnore]
    public class DroneCameraController : NetworkBehaviour
    {
        [SerializeField] private GameObject cameraObject;
        [SerializeField] private GameObject irCameraObject;

        [SerializeField] [Range(-45, 90)] private float currentAngle;
        [SerializeField] [Range(-45, 90)] private float minAngle;
        [SerializeField] [Range(0, 90)] private float maxAngle = 90;

        public float CurrentAngle => currentAngle;

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

             // if (DroneInput.Instance && cameraObject.activeSelf != DroneInput.Instance.DroneCam)
             //     SetCamera(DroneInput.Instance.DroneCam);

            if(!DroneInput.Instance)
                return;

            currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

            if (DroneInput.Instance != null)
                currentAngle -= Time.deltaTime * DroneInput.Instance.CameraAngleInput * 1000;

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

        public void SetCamera(bool value) => cameraObject.SetActive(value);

        public void SetCameraAngle(float value)
        {
            var rot = cameraObject.transform.localRotation;
            rot = Quaternion.Euler(value, rot.y, rot.z);
            cameraObject.transform.localRotation = rot;
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