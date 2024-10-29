using System;
using Code.Internal.Drone;
using UnityEngine;

namespace Code.Internal.XR
{
    public class XRDisableHeadTrackingInFPV : MonoBehaviour
    {
        public bool shouldBeEnabledInFPV = true;
        
        [SerializeField] private GameObject switchedCamera;
        [SerializeField] private GameObject[] enabledInFPVObjects;
        [SerializeField] private GameObject[] disabledInFPVObjects;
        private DroneInput _droneInput;

        public bool IsViewed = false;
        
        private void Update()
        {
            if (!_droneInput)
            {
                if(switchedCamera)
                    switchedCamera.SetActive(!shouldBeEnabledInFPV);
                _droneInput = DroneInput.Instance;
            }
            
            if(_droneInput)
            {
                if (switchedCamera)
                    SwitchObject(shouldBeEnabledInFPV, switchedCamera);
                SwitchObject(!shouldBeEnabledInFPV, enabledInFPVObjects);
                SwitchObject(shouldBeEnabledInFPV, disabledInFPVObjects);
            }
            else
            {
                if (switchedCamera)
                    SwitchViewObject(IsViewed, switchedCamera);
            }
        }

        private void SwitchObject(bool value, GameObject o) {
            o?.SetActive(_droneInput.DroneCam switch
            {
                true => value,
                false => !value
            });
        }
        
        private void SwitchViewObject(bool value, GameObject o) {
            o?.SetActive(IsViewed);
        }
        
        private void SwitchObject(bool value, GameObject[] ojbects)
        {
            foreach (var o in ojbects)
            {
                SwitchObject(value, o);
            }
        }

        private void OnDisable()
        {
            if(switchedCamera)
                switchedCamera.SetActive(!shouldBeEnabledInFPV);
        }
    }
}