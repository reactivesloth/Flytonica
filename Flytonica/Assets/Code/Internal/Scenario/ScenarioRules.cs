using System;
using Code.Internal.Drone;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface;
using Code.Internal.UserInterface.DroneHudElements;
using Code.Internal.UserInterface.Pages;
using UnityEngine;

namespace Code.Internal.Scenario
{
    public enum ScenarioZoneWarningType
    {
        Clear,
        Warning,
        Unallowed
    }
    
    public class ScenarioRules : MonoBehaviour
    {
        [SerializeField] private AudioClip outOfLocationWarning;
        [SerializeField] private AudioClip outOfLocation;
        [SerializeField] private SceneLoadingSettings _sceneLoadingSettings;
        [SerializeField] private float allowedForbiddenTime = 10;
        private float failTime = 0;
        private bool droneInForbiddenZone = false;
        private DroneSensors CurrentDroneSensors => DroneController.Instance.DroneSensors;

        private float savedSignal = -1;

        private void Awake()
        {
            savedSignal = -1;
        }

        private void Update()
        {
            if (DroneController.Instance != null)
            {
                float maxHeight = _sceneLoadingSettings.currentScenario.currentMap.maxAllowedHeight;
                if (maxHeight > 0)
                    DroneHUD.Instance.AltValueElement.MaxValue = (int) maxHeight;
                
                if (maxHeight > 0 && !droneInForbiddenZone)
                {
                    if (DroneController.Instance.transform.position.y >= maxHeight - 10 &&
                        DroneController.Instance.transform.position.y < maxHeight)
                    {
                        UpdateEndOfMapZone(ScenarioZoneWarningType.Warning);
                    }
                    else if (DroneController.Instance.transform.position.y >= maxHeight)
                    {
                        UpdateEndOfMapZone(ScenarioZoneWarningType.Unallowed);
                    }
                    else
                    {
                        UpdateEndOfMapZone(ScenarioZoneWarningType.Clear);
                    }
                }
                else if (droneInForbiddenZone)
                {
                    UpdateEndOfMapZone(ScenarioZoneWarningType.Warning);
                }
                else
                {
                    UpdateEndOfMapZone(ScenarioZoneWarningType.Clear);
                }
            }
        }

        private void UpdateEndOfMapZone (ScenarioZoneWarningType type)
        {
            switch (type)
            {
                case ScenarioZoneWarningType.Clear:
                    if (savedSignal > -1)
                    {
                        CurrentDroneSensors.CameraSignalModifier = savedSignal;
                        savedSignal = -1;
                    }
                    failTime = allowedForbiddenTime;
                    break;
                case ScenarioZoneWarningType.Warning:
                    if (savedSignal <= -1)
                        savedSignal = CurrentDroneSensors.CameraSignalModifier;
                    CurrentDroneSensors.CameraSignalModifier = 0.5f;
                    failTime = allowedForbiddenTime;
                    DroneHUD.Instance.SetMessage(MessageType.Warning, $"Вы приближаетесь к границе локации, вернитесь назад!", 0.1f, outOfLocationWarning);
                    break;
                case ScenarioZoneWarningType.Unallowed:
                    failTime -= Time.deltaTime;
                    DroneHUD.Instance.SetMessage(MessageType.Error, $"Вы покинули границу локации, сценарий будет перезапущен через {failTime.ToString("F2")} секунд", 0.1f, outOfLocation);
                    if (savedSignal <= -1)
                        savedSignal = CurrentDroneSensors.CameraSignalModifier;
                    CurrentDroneSensors.CameraSignalModifier = 0;
                    if (failTime <= 0)
                    {
                        DroneController.Instance.ResetDrone();
                        savedSignal = -1;
                        CurrentDroneSensors.CameraSignalModifier = 1;
                        CurrentDroneSensors.InputSignalModifier = 1;
                        //GameSceneManager.Instance.Replay();
                    }
                    break;
            }   
        }
    }
}