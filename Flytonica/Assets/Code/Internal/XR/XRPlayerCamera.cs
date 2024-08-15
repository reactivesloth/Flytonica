using System;
using Code.Internal.Drone;
using Unity.Cinemachine;
using UnityEngine;

namespace Code.Internal.XR
{
    public class XRPlayerCamera : MonoBehaviour
    {
        private CinemachineCamera _camera;
        
        private void Awake()
        {
            _camera = gameObject.GetComponentInChildren<CinemachineCamera>(true);
        }

        private void Update()
        {
            if (_camera.Target.TrackingTarget == null)
            {
                var target = FindAnyObjectByType<DroneController>();
                if (target != null)
                    _camera.Target.TrackingTarget = target.transform;
            }
        }
    }
}