using System;
using Code.Internal.Drone;
using Code.Internal.UserInterface;
using Code.Internal.UserInterface.DroneHudElements;
using UnityEngine;

namespace Code.Internal.Scenario
{
    public class DangerTrigger : MonoBehaviour
    {
        [SerializeField] private string warningText, errorText;

        [SerializeField] private DroneTriggerCallback warning, danger;
        [Range(0, 1)] [SerializeField] private float cameraLostPercent = 0.5f, controlLostPercent = 0.5f;

        private void Awake()
        {
            warning.OnDroneEnter += OnWarningZoneEnter;
            warning.OnDroneExit += OnWarningZoneExit;
            danger.OnDroneEnter += OnDangerZoneEnter;
            danger.OnDroneExit += OnDangerZoneExit;
        }

        private void Update()
        {
            
        }

        private void OnWarningZoneEnter()
        {
            DroneHUD.Instance.SetMessage(MessageType.Warning, warningText);
        }

        private void OnWarningZoneExit()
        {
            //TODO: HideWarning
            DroneHUD.Instance.ClearMessage();
        }

        private void OnDangerZoneEnter()
        {
            //TODO: Danger Acton
            //DroneHUD.Instance.SetMessage();
        }

        private void OnDangerZoneExit()
        {
            //TODO: Undanger Acton
        }

        private void RandomEffect()
        {
            
        }

        private void OnDestroy()
        {
            warning.OnDroneEnter -= OnWarningZoneEnter;
            warning.OnDroneExit -= OnWarningZoneExit;
            danger.OnDroneEnter -= OnDangerZoneEnter;
            danger.OnDroneExit -= OnDangerZoneExit;
        }
    }
}