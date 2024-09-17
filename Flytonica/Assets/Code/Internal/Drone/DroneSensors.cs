using Code.Internal.SceneManagement;
using Code.Internal.UserInterface;
using Code.Internal.UserInterface.DroneHudElements;
using UnityEngine;

namespace Code.Internal.Drone
{
    [RequireComponent(typeof(DroneController))]
    public class DroneSensors : MonoBehaviour
    {
        private DroneController _droneController;
        private Rigidbody _rigidbody;
        private Transform _transform;

        private DroneFlightSettings savedFlightSettings;
        
        private void Awake()
        {
            _droneController = gameObject.GetComponent<DroneController>();
            _rigidbody = gameObject.GetComponent<Rigidbody>();
            _transform = transform;
        }

        private void Update()
        {
            if (DroneHUD.Instance.IsShowing())
            {
                DroneHUD.Instance.AltValueElement.Set(_transform.position.y);
                DroneHUD.Instance.SpeedValueElement.Set(_rigidbody.linearVelocity.magnitude * 3.6f);
                DroneHUD.Instance.BatteryElement.SetVoltage(_droneController.Settings.currentVoltageV);
                DroneHUD.Instance.HorizonElement.SetPitch(-_transform.localRotation.eulerAngles.x);
                DroneHUD.Instance.HorizonElement.SetRoll(transform.localEulerAngles.z);
                DroneHUD.Instance.SetMode(savedFlightSettings.modeName);
            }
            
            if (savedFlightSettings != _droneController.Settings.currentFlightMode)
            {
                savedFlightSettings = _droneController.Settings.currentFlightMode;
                DroneHUD.Instance.SetMessage (MessageType.Normal, $"Переключение режима: {savedFlightSettings.modeName}", 2);
            }
        }
    }
}