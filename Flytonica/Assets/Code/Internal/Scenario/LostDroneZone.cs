using Code.Internal.Drone;
using Code.Internal.UserInterface;
using Code.Internal.UserInterface.DroneHudElements;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.Internal.Scenario
{
    public class LostDroneZone : MonoBehaviour
    {
        [SerializeField] private AudioClip lowSignal, lostSignal;
        [SerializeField] private bool showZoneOnMap = false;
        [SerializeField] private string warningText, errorText;

        [SerializeField] private DroneTriggerCallback warning, danger;
        [Range(0, 1)] [SerializeField] private float cameraLostPercent = 0.5f, controlLostPercent = 0.5f;

        private DroneSensors CurrentDroneSensors => DroneController.Instance?.DroneSensors;

        private void Awake()
        {
            warning.OnDroneEnter += OnWarningZoneEnter;
            warning.OnDroneExit += OnWarningZoneExit;
            danger.OnDroneEnter += OnDangerZoneEnter;
            danger.OnDroneExit += OnDangerZoneExit;
        }

        private void Start() => InitMinimapEntity();

        private void OnWarningZoneEnter()
        {
            if (DroneHUD.Instance != null)
                DroneHUD.Instance.SetMessage(MessageType.Warning, warningText, lowSignal.length + 1, lowSignal);
            RandomEffect(0.5f);
        }

        private void OnWarningZoneExit()
        {
            if (DroneHUD.Instance != null)
                DroneHUD.Instance.ClearMessage();
            if(!DroneController.Instance)
                return;
            CurrentDroneSensors.CameraSignalModifier = 1;
            CurrentDroneSensors.InputSignalModifier = 1;
        }

        private void OnDangerZoneEnter()
        {
            if (DroneHUD.Instance != null)
                DroneHUD.Instance.SetMessage(MessageType.Error, errorText, lowSignal.length + 1, lostSignal);
            RandomEffect();
        }

        private void OnDangerZoneExit()
        {
            if(!DroneController.Instance)
                return;
            
            if(warning.IsDroneInZone)
                OnWarningZoneEnter();
            else
                OnWarningZoneExit();
        }

        private void RandomEffect(float targetValue = 0)
        {
            if(!DroneController.Instance)
                return;
            
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

        private void InitMinimapEntity()
        {
            var entity = GetComponent<bl_MiniMapEntity>();
            if (!entity)
                return;
            entity.enabled = showZoneOnMap;
            
            if(!showZoneOnMap)
                return;
            
            var boxCollider = warning.GetComponent<BoxCollider>();
            if (!boxCollider)
                return;
            
            var radius = Mathf.Max(boxCollider.size.x, boxCollider.size.y, boxCollider.size.z) / 2f;
            entity.CircleAreaRadius = radius;
            entity.OnUpdateItem();
        }
    }
}