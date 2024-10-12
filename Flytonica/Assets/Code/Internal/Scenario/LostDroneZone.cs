using System;
using Code.Internal.Drone;
using Code.Internal.UserInterface;
using Code.Internal.UserInterface.DroneHudElements;
using FishNet;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.Internal.Scenario
{
    public class LostDroneZone : MonoBehaviour
    {
        [SerializeField] private string warningText, errorText;

        [SerializeField] private DroneTriggerCallback warning, danger;
        [Range(0, 1)] [SerializeField] private float cameraLostPercent = 0.5f, controlLostPercent = 0.5f;

        private DroneSensors CurrentDroneSensors => DroneController.Instance.DroneSensors;

        private void Awake()
        {
            warning.OnDroneEnter += OnWarningZoneEnter;
            warning.OnDroneExit += OnWarningZoneExit;
            danger.OnDroneEnter += OnDangerZoneEnter;
            danger.OnDroneExit += OnDangerZoneExit;
        }

        private void OnWarningZoneEnter()
        {
            if (DroneHUD.Instance != null)
                DroneHUD.Instance.SetMessage(MessageType.Warning, warningText);
            RandomEffect(0.5f);
        }

        private void OnWarningZoneExit()
        {
            if (DroneHUD.Instance != null)
                DroneHUD.Instance.ClearMessage();
            CurrentDroneSensors.CameraSignalModifier = 1;
            CurrentDroneSensors.InputSignalModifier = 1;
        }

        private void OnDangerZoneEnter()
        {
            if (DroneHUD.Instance != null)
                DroneHUD.Instance.SetMessage(MessageType.Error, errorText);
            RandomEffect();
        }

        private void OnDangerZoneExit()
        {
            if(warning.IsDroneInZone)
                OnWarningZoneEnter();
            else
                OnWarningZoneExit();
        }

        private void RandomEffect(float targetValue = 0)
        {
            if (Random.value < cameraLostPercent)
                CurrentDroneSensors.CameraSignalModifier = targetValue;
            
            if (Random.value < controlLostPercent)
                CurrentDroneSensors.InputSignalModifier = targetValue;
        }

        private void OnDestroy()
        {
            OnWarningZoneExit();
            OnDangerZoneExit();
            
            warning.OnDroneEnter -= OnWarningZoneEnter;
            warning.OnDroneExit -= OnWarningZoneExit;
            danger.OnDroneEnter -= OnDangerZoneEnter;
            danger.OnDroneExit -= OnDangerZoneExit;
        }
    }
}