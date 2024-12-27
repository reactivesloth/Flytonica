using System;
using System.Linq;
using Code.Internal.Drone;
using Code.Internal.UserInterface;
using Code.Internal.XR;
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
                _input = DroneInput.Instance;
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

        private void OnDisable()
        {
            DroneHUD.Instance.ShowHUD(false);
        }
    }
}