using Code.Internal.Drone;
using Code.Internal.UserInterface;
using UnityEngine;

namespace Code.Internal
{
    public class FPVModeEffects : MonoBehaviour
    {
        private DroneInput _input;

        private void Update()
        {
            if (_input == null)
            {
                _input = FindAnyObjectByType<DroneInput>();
                return;
            }
            
            if (_input.DroneCam)
            {
                DroneHUD.Instance.ShowHUD(true);
            }
            else
            {
                DroneHUD.Instance.ShowHUD(false);
            }
        }
    }
}