using System;
using Code.Internal.Drone;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Internal.XR
{
    public class XRDisableHeadTrackingInFPV : MonoBehaviour
    {
        public bool shouldBeEnabledInFPV = true;
        
        private GameObject _camera;
        [SerializeField] private GameObject[] enabledInFPVObjects;
        [SerializeField] private GameObject[] disabledInFPVObjects;
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
                _droneInput = DroneInput.Instance;
                return;
            }

            _camera.SetActive(_droneInput.DroneCam switch
            {
                true => shouldBeEnabledInFPV,
                false => !shouldBeEnabledInFPV
            });

            SwitchObject(shouldBeEnabledInFPV, _camera);
            SwitchObject(!shouldBeEnabledInFPV, enabledInFPVObjects);
            SwitchObject(shouldBeEnabledInFPV, disabledInFPVObjects);
        }

        private void SwitchObject (bool value, GameObject o) {
            o.SetActive(_droneInput.DroneCam switch
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
            _camera.SetActive(!shouldBeEnabledInFPV);
        }
    }
}