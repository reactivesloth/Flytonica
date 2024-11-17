using System.Collections.Generic;
using System.Linq;
using Code.Internal.Drone;
using Code.Internal.MapEditor;
using Code.Internal.UserInterface;
using Code.Internal.UserInterface.DroneHudElements;
using Code.Internal.UserInterface.Pages;
using UltimateReplay;
using Unity.VisualScripting;
using UnityEngine;

namespace Code.Internal.Replays
{
    public class PlaybackReplayBehaviour : ReplayBehaviour
    {
        public static PlaybackReplayBehaviour Instance { get; private set; }

        [SerializeField] private GameObject uiPanelInTablet;
        [SerializeField] private Camera pcCamera;

        public readonly List<DroneController> Drones = new();
        private int _currentDroneIndex = -1;
        private DroneController _currentDrone;

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        private void Update()
        {
            if (!IsReplaying)
                return;

            InputHandle();
            UpdateDrones();
        }

        private void UpdateDrones()
        {
            var allDrones = FindObjectsByType<DroneController>(FindObjectsSortMode.None);
            var newDrones = allDrones.Where(d => !Drones.Contains(d));
            var removedDrones = Drones.Where(d => d == null || !d.gameObject.activeSelf);
            
            foreach (var removedDrone in removedDrones)
            {
                Drones.Remove(removedDrone);
                
                if (removedDrone != _currentDrone) continue;
                _currentDrone = null;
                _currentDroneIndex = -1;
                DroneHUD.Instance.ShowHUD(false);
                DroneHUD.Instance.MessageBoxElement.ClearMessage();
            }

            foreach (var newDrone in newDrones)
            {
                Drones.Add(newDrone);
            }

            FindAnyObjectByType<ViewReplayPage>()?.SetPlayerList(Drones);

            if (_currentDroneIndex >= Drones.Count)
            {
                _currentDroneIndex = -1;
            }
        }

        private void InputHandle()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.C))
                ChangeCamera();
        }

        private void ChangeCamera()
        {
            // Turn off the current drone's camera if any
            if (_currentDroneIndex >= 0 && _currentDroneIndex < Drones.Count)
            {
                var currentDrone = Drones[_currentDroneIndex];
                SetDroneCamera(currentDrone, false);
            }

            // Move to the next drone
            _currentDroneIndex++;

            // If no more drones, switch to player view
            if (_currentDroneIndex >= Drones.Count)
            {
                _currentDroneIndex = -1;
                DroneHUD.Instance.ShowHUD(false);
                DroneHUD.Instance.MessageBoxElement.ClearMessage();
                return;
            }

            var newDrone = Drones[_currentDroneIndex];
            _currentDrone = newDrone;
            SetDroneCamera(newDrone, true);

            var droneCameraController = newDrone.GetComponent<DroneCameraController>();
            if (droneCameraController)
            {
                var playerName = droneCameraController.GetComponent<DroneReplayBehaviour>().PlayerName;
                DroneHUD.Instance.MessageBoxElement.DrawMessage(MessageType.Normal, playerName);
                DroneHUD.Instance.ShowHUD(true);
            }
        }

        private void SetDroneCamera(DroneController drone, bool isActive)
        {
            var droneInput = drone.GetComponent<DroneInput>();
            if (!droneInput)
                return;
            droneInput.DroneCam = isActive;
            var droneCameraController = drone.GetComponent<DroneCameraController>();
            if (!droneCameraController)
                return;
            droneCameraController.GetComponent<DroneReplayBehaviour>().IsObservable = isActive;
            droneCameraController.SetCamera(isActive);
        }

        protected override void OnReplayStart()
        {
            base.OnReplayStart();

            UIController.Instance.SetUiToTablet(uiPanelInTablet, true);
            pcCamera?.gameObject.GetOrAddComponent<MapEditorCamera>();
        }

        protected override void OnReplayEnd()
        {
            base.OnReplayEnd();
            if (!IsReplaying)
                return;
            UIController.Instance.SetUiToTablet(uiPanelInTablet, false);
            DroneHUD.Instance.ShowHUD(false);
            if (pcCamera?.gameObject.GetComponent<MapEditorCamera>() != null)
                Destroy(pcCamera?.GetComponent<MapEditorCamera>());
            pcCamera?.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
    }
}