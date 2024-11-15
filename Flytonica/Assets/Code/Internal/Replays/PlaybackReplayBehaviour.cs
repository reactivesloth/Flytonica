using Code.Internal.Drone;
using Code.Internal.UserInterface;
using UltimateReplay;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

namespace Code.Internal.Replays
{
    public class PlaybackReplayBehaviour : ReplayBehaviour
    {
        [SerializeField] private GameObject uiPanelInTablet;
        
        private DroneInput _droneInput;
        private DroneCameraController _droneCameraController;

        private void Update()
        {
            if (!IsReplaying)
                return;
            
            FindObjects();
            InputHandle();
        }

        private void FindObjects()
        {
            if (!_droneInput)
                _droneInput = FindAnyObjectByType<DroneInput>(FindObjectsInactive.Exclude);
            if(!_droneCameraController)
                _droneCameraController = FindFirstObjectByType<DroneCameraController>();
        }

        private void InputHandle()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.C))
                ChangeCamera();
        }
        
        private void ChangeCamera()
        {
            if (!_droneInput)
                return; 
            _droneInput.DroneCam = !_droneInput.DroneCam;
            if (!_droneCameraController)
                return;
            _droneCameraController.SetCamera(_droneInput.DroneCam);
            DroneHUD.Instance.ShowHUD(_droneInput.DroneCam);
        }

        protected override void OnReplayStart()
        {
            base.OnReplayStart();
            
            UIController.Instance.SetUiToTablet(uiPanelInTablet, true);
        }

        protected override void OnReplayEnd()
        {
            base.OnReplayEnd();
            if (!IsReplaying)
                return;
            UIController.Instance.SetUiToTablet(uiPanelInTablet, false);
            DroneHUD.Instance.ShowHUD(false);
            if(_droneInput)
                _droneInput.DroneCam = false;
            if(_droneCameraController)
                _droneCameraController.SetCamera(_droneInput.DroneCam);

            _droneInput = null;
            _droneCameraController = null;
        }
    }
}