using Code.Internal.Drone;
using UnityEngine;

namespace Code.Internal.XR
{
    public class XRPlayerCamera : MonoBehaviour
    {
        private Transform _myTransform;
        
        private void Awake()
        {
            _myTransform = transform;
        }

        private void LateUpdate()
        {
            if (DroneController.Instance!= null)
            {
                var rotation = Quaternion.LookRotation (DroneController.Instance.transform.position - _myTransform.position);
                _myTransform.rotation = Quaternion.Slerp (transform.rotation, rotation, Time.deltaTime * 5);
            }
        }
    }
}