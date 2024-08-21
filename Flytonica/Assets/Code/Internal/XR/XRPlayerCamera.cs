using System;
using Code.Internal.Drone;
using Unity.Cinemachine;
using UnityEngine;

namespace Code.Internal.XR
{
    public class XRPlayerCamera : MonoBehaviour
    {
        private Transform _myTransform;
        
        private void Awake()
        {
            _myTransform = transform;
        }

        private void Update()
        {
            var target = FindAnyObjectByType<DroneController>();
            if (target != null)
                _myTransform.LookAt(target.transform);
        }
    }
}