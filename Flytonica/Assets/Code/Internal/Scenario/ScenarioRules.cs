using System;
using Code.Internal.Drone;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface;
using Code.Internal.UserInterface.DroneHudElements;
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
        [SerializeField] private SceneLoadingSettings _sceneLoadingSettings;
        [SerializeField] private float allowedForbiddenTime = 10;
        private float failTime = 0;
        private bool droneInForbiddenZone = false;

        private void Update()
        {
            if (DroneController.Instance != null)
            {
                float maxHeight = _sceneLoadingSettings.currentMap.maxAllowedHeight;

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
                    failTime = allowedForbiddenTime;
                    break;
                case ScenarioZoneWarningType.Warning:
                    failTime = allowedForbiddenTime;
                    DroneHUD.Instance.SetMessage(MessageType.Warning, $"Вы приближаетесь к границе локации, вернитесь назад!", 0.1f);
                    break;
                case ScenarioZoneWarningType.Unallowed:
                    failTime -= Time.deltaTime;
                    DroneHUD.Instance.SetMessage(MessageType.Error, $"Вы покинули границу локации, сценарий будет перезапущен через {failTime.ToString("F2")} секунд", 0.1f);
                    if (failTime <= 0)
                    {
                        // reset;
                    }
                    break;
            }   
        }
    }
}