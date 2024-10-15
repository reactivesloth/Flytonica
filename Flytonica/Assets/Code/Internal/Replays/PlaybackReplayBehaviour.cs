using Code.Internal.Drone;
using Code.Internal.UserInterface;
using UltimateReplay;
using UnityEngine;

namespace Code.Internal.Replays
{
    public class PlaybackReplayBehaviour : ReplayBehaviour
    {
        private void Update()
        {
            if (!IsReplaying)
                return;

            InputHandle();
        }

        private void InputHandle()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.C))
                ChangeCamera();
        }

        private void ChangeCamera()
        {
            var droneInput = FindFirstObjectByType<DroneInput>();
            if (!droneInput)
                return;
            droneInput.DroneCam = !droneInput.DroneCam;
            var droneCamera = FindFirstObjectByType<DroneCamera>();
            if (!droneCamera)
                return;
            droneCamera.SetCamera(droneInput.DroneCam);
            DroneHUD.Instance.ShowHUD(droneInput.DroneCam);
        }

        protected override void OnReplayEnd()
        {
            base.OnReplayEnd();
            DroneHUD.Instance.ShowHUD(false);
        }
    }
}