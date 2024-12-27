using Code.Internal.Drone;
using Code.Internal.SceneManagement;
using UnityEngine;

namespace Code.Internal.XR
{
    public class XRPlayerCamera : MonoBehaviour
    {
        private Transform _myTransform;
        private Transform _target;
        
        private void Awake()
        {
            _myTransform = transform;
        }

        private void LateUpdate()
        {
            if(!GameSceneManager.Instance.IsPlaying)
                return;
            
            var rotation = transform.rotation;
            
            if (DroneController.Instance != null)
            {
                rotation = Quaternion.LookRotation (DroneController.Instance.transform.position - _myTransform.position);
            } 
            else
            {
                var drone = FindFirstObjectByType<DroneController>();
                if(drone)
                    rotation = Quaternion.LookRotation (drone.transform.position - _myTransform.position);
            }
            
            _myTransform.rotation = Quaternion.Slerp (transform.rotation, rotation, Time.deltaTime * 5);
        }
        
        public void SetSecondTarget (Transform t) {
            _target = t;    
        }
    }
}