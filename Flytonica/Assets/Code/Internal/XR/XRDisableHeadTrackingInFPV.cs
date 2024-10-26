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

        private void Update()
        {
            if (_droneInput == null)
            {
                if(switchedCamera)
                    switchedCamera.SetActive(!shouldBeEnabledInFPV);
                _droneInput = DroneInput.Instance;
                return;
            }
            
            if(switchedCamera)
                SwitchObject(shouldBeEnabledInFPV, switchedCamera);
            SwitchObject(!shouldBeEnabledInFPV, enabledInFPVObjects);
            SwitchObject(shouldBeEnabledInFPV, disabledInFPVObjects);
        }

        private void SwitchObject (bool value, GameObject o) {
            o?.SetActive(_droneInput.DroneCam switch
            {
                true => value,
                false => !value
            });
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