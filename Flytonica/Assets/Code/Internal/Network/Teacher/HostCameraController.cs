using System;
using System.Collections.Generic;
using Code.Internal.Drone;
using Code.Internal.UserInterface;
using Code.Internal.XR;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Internal.Network.Teacher
{
    public class HostCameraController : MonoBehaviour
    {
        public static HostCameraController Instance;
        
        [SerializeField] private List<XRDisableHeadTrackingInFPV> playerFpvCameraControllers;

        private NetworkObject targetDrone;
        
        private void Awake()
        {
            Instance = this;
            SetPlayerFpv(true);
        }

        private void Update()
        {
            if(targetDrone)
                targetDrone?.GetComponent<DroneSensors>()?.UpdateHud();
            
            if(Camera.allCamerasCount == 0)
                SetTeacherView();
        }

        public void SetTeacherView()
        {
            SetPlayerFpv(true);

            if (targetDrone)
                targetDrone.GetComponent<NetBridge>().IsObservable = false;   
            var fpvCamController = targetDrone.GetComponent<XRDisableHeadTrackingInFPV>();
            if(!fpvCamController) 
                return;
            targetDrone = null;
            fpvCamController.IsViewed = false;
            DroneHUD.Instance.ShowHUD(false);
        }
        
        public void SetTargetDrone(NetworkObject drone)
        {
            if (targetDrone)
            {
                targetDrone.GetComponent<NetBridge>().IsObservable = false;   
                var targetFpv = targetDrone.GetComponent<XRDisableHeadTrackingInFPV>();
                if(!targetFpv) 
                    return;
                targetDrone = null;
                targetFpv.IsViewed = false;
            }
            
            var fpvCamController = drone?.GetComponent<XRDisableHeadTrackingInFPV>();
            if(!fpvCamController)
                return;
            
            targetDrone = drone;
            targetDrone.GetComponent<NetBridge>().IsObservable = true;
            fpvCamController.IsViewed = true;
            SetPlayerFpv(false);
            DroneHUD.Instance.ShowHUD(true);
        }
        
        private void SetPlayerFpv(bool value) => playerFpvCameraControllers.ForEach(p => p.IsViewed = value);

        private void OnDisable()
        {
            var fpvCamController = targetDrone?.GetComponent<XRDisableHeadTrackingInFPV>();
            if(fpvCamController)
                fpvCamController.IsViewed = false;
            playerFpvCameraControllers.ForEach(p => p.IsViewed = true);
        }
    }
}