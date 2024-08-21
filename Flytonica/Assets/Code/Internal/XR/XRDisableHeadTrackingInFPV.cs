using Code.Internal.Drone;
using UnityEngine;

namespace Code.Internal.XR
{
    public class XRDisableHeadTrackingInFPV : MonoBehaviour
    {
        public bool shouldBeEnabledInFPV = true;
        
        private Camera _camera;
        private DroneInput _droneInput;

        private void Awake()
        {
            _camera = gameObject.GetComponentInChildren<Camera>(true);
        }

        private void Update()
        {
            if (_droneInput == null)
            {
                _droneInput = FindAnyObjectByType<DroneInput>();
                return;
            }

            _camera.enabled = _droneInput.DroneCam switch
            {
                true => shouldBeEnabledInFPV,
                false => !shouldBeEnabledInFPV
            };
        }
    }
}