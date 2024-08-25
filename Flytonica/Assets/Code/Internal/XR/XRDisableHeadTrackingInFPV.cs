using System;
using Code.Internal.Drone;
using UnityEngine;

namespace Code.Internal.XR
{
    public class XRDisableHeadTrackingInFPV : MonoBehaviour
    {
        public bool shouldBeEnabledInFPV = true;
        
        private GameObject _camera;
        private DroneInput _droneInput;

        private void Awake()
        {
            _camera = gameObject.GetComponentInChildren<Camera>(true).gameObject;
        }

        private void Update()
        {
            if (_droneInput == null)
            {
                _camera.SetActive(!shouldBeEnabledInFPV);
                _droneInput = FindAnyObjectByType<DroneInput>();
                return;
            }

            _camera.SetActive(_droneInput.DroneCam switch
            {
                true => shouldBeEnabledInFPV,
                false => !shouldBeEnabledInFPV
            });
        }

        private void OnDisable()
        {
            _camera.SetActive(!shouldBeEnabledInFPV);
        }
    }
}