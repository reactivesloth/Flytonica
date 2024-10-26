using System.Collections.Generic;
using Code.Internal.Drone;
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
            playerFpvCameraControllers.ForEach(p => p.IsViewed = true);
        }

        public void SetTeacherView()
        {
            playerFpvCameraControllers.ForEach(p => p.IsViewed = true);
            if(!targetDrone) return;
            var fpvCamController = targetDrone.GetComponent<XRDisableHeadTrackingInFPV>();
            if(!fpvCamController) return;
            fpvCamController.IsViewed = false;
        }
        
        public void SetTargetDrone(NetworkObject drone)
        {
            var fpvCamController = drone.GetComponent<XRDisableHeadTrackingInFPV>();
            if(!fpvCamController)
                return;

            targetDrone = drone;
            fpvCamController.IsViewed = true;
            playerFpvCameraControllers.ForEach(p => p.IsViewed = false);
        }

        private void OnDisable()
        {
            var fpvCamController = targetDrone?.GetComponent<XRDisableHeadTrackingInFPV>();
            if(fpvCamController)
                fpvCamController.IsViewed = false;
            playerFpvCameraControllers.ForEach(p => p.IsViewed = true);
        }
    }
}