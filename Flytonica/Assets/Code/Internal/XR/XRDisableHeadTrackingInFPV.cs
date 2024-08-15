using System;
using Code.Internal.Drone;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SpatialTracking;

namespace Code.Internal.XR
{
    [RequireComponent(typeof(XROrigin))]
    public class XRDisableHeadTrackingInFPV : MonoBehaviour
    {
        private XROrigin _xrOrigin;
        private TrackedPoseDriver[] _poseDriver;
        private DroneInput _droneInput;

        private void Awake()
        {
            _xrOrigin = gameObject.GetComponent<XROrigin>();
            _poseDriver = gameObject.GetComponentsInChildren<TrackedPoseDriver>(true);
        }

        private void Update()
        {
            if (_droneInput == null)
            {
                _droneInput = FindAnyObjectByType<DroneInput>();
                return;
            }

            if (_xrOrigin.enabled != !_droneInput.DroneCam)
                _xrOrigin.enabled = !_droneInput.DroneCam;

            foreach (var poseDriver in _poseDriver)
            {
                if (poseDriver.enabled != !_droneInput.DroneCam)
                    poseDriver.enabled = !_droneInput.DroneCam;
            }
        }
    }
}