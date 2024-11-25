using System.Collections.Generic;
using System.Linq;
using Code.Internal.Drone;
using Code.Internal.MapEditor;
using Code.Internal.UserInterface;
using Code.Internal.UserInterface.DroneHudElements;
using Code.Internal.UserInterface.Pages;
using Code.Internal.XR;
using UltimateReplay;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

namespace Code.Internal.Replays
{
    public class PlaybackReplayBehaviour : ReplayBehaviour
    {
        public static PlaybackReplayBehaviour Instance { get; private set; }

        [SerializeField] private GameObject uiPanelInTablet;
        [SerializeField] private Camera pcCamera;
        [SerializeField] private GameObject vrPlayer;

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

        public void ChangeCamera()
        {
            // Проверяем, активна ли платформа XR
            if (XRSettings.enabled && XRSettings.isDeviceActive ||
                FindAnyObjectByType<XRDeviceSimulator>(FindObjectsInactive.Include) != null)
            {
                ChangeCameraXR();
            }
            else
            {
                ChangeCameraPC();
            }
        }

        private void ChangeCameraPC()
        {
            if (_currentDroneIndex >= 0 && _currentDroneIndex < Drones.Count)
            {
                var currentDrone = Drones[_currentDroneIndex];
                SetDroneCameraPC(currentDrone, false);
            }

            _currentDroneIndex++;

            if (_currentDroneIndex >= Drones.Count)
            {
                _currentDroneIndex = -1;
                DroneHUD.Instance.ShowHUD(false);
                DroneHUD.Instance.MessageBoxElement.ClearMessage();
                return;
            }

            var newDrone = Drones[_currentDroneIndex];
            _currentDrone = newDrone;
            SetDroneCameraPC(newDrone, true);

            var droneCameraController = newDrone.GetComponent<DroneCameraController>();
            if (droneCameraController)
            {
                var playerName = droneCameraController.GetComponent<DroneReplayBehaviour>().PlayerName;
                DroneHUD.Instance.MessageBoxElement.DrawMessage(MessageType.Normal, playerName);
                DroneHUD.Instance.ShowHUD(true);
            }
        }

        private void ChangeCameraXR()
        {
            if (_currentDrone != null)
            {
                SetDroneCameraXR(_currentDrone, false);
            }

            _currentDroneIndex++;

            if (_currentDroneIndex >= Drones.Count)
            {
                _currentDroneIndex = -1;
                _currentDrone = null;
                DroneHUD.Instance.ShowHUD(false);
                DroneHUD.Instance.MessageBoxElement.ClearMessage();
                return;
            }

            _currentDrone = Drones[_currentDroneIndex];
            SetDroneCameraXR(_currentDrone, true);

            var droneCameraController = _currentDrone.GetComponent<DroneCameraController>();
            if (droneCameraController)
            {
                var playerName = droneCameraController.GetComponent<DroneReplayBehaviour>().PlayerName;
                DroneHUD.Instance.MessageBoxElement.DrawMessage(MessageType.Normal, playerName);
                DroneHUD.Instance.ShowHUD(true);
            }
        }

        private void SetDroneCameraPC(DroneController drone, bool isActive)
        {
            var droneInput = drone.GetComponent<DroneInput>();
            if (!droneInput)
                return;
            droneInput.DroneCam = isActive;

            var droneCameraController = drone.GetComponent<DroneCameraController>();
            if (!droneCameraController)
                return;

            var droneReplayBehaviour = droneCameraController.GetComponent<DroneReplayBehaviour>();
            if (droneReplayBehaviour)
                droneReplayBehaviour.IsObservable = isActive;

            var xrDisableHeadTracking = drone.GetComponent<XRDisableHeadTrackingInFPV>();
            if (xrDisableHeadTracking)
                xrDisableHeadTracking.IsViewed = isActive;

            droneCameraController.SetCamera(isActive);
        }

        private void SetDroneCameraXR(DroneController drone, bool isActive)
        {
            var droneReplayBehaviour = drone.GetComponent<DroneReplayBehaviour>();
            if (droneReplayBehaviour)
                droneReplayBehaviour.IsObservable = isActive;

            var droneCameraController = drone.GetComponent<DroneCameraController>();
            if (!droneCameraController)
                return;

            droneCameraController.UiCamera.SetActive(isActive);
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
            UIController.Instance.SetUiToTablet(uiPanelInTablet, false);
            DroneHUD.Instance.ShowHUD(false);

            // Отключаем активную камеру при завершении воспроизведения
            if (_currentDrone != null)
            {
                if (XRSettings.enabled && XRSettings.isDeviceActive ||
                    FindAnyObjectByType<XRDeviceSimulator>(FindObjectsInactive.Include) != null)
                {
                    SetDroneCameraXR(_currentDrone, false);
                }
                else
                {
                    SetDroneCameraPC(_currentDrone, false);
                }
                _currentDrone = null;
                _currentDroneIndex = -1;
            }

            if (pcCamera?.gameObject.GetComponent<MapEditorCamera>() != null)
                Destroy(pcCamera?.GetComponent<MapEditorCamera>());
            pcCamera?.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            vrPlayer?.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
    }
}