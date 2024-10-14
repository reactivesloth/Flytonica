using Code.Internal.Drone;
using UnityEngine;

namespace Code.Internal.XR
{
    public class XRPlayerCamera : MonoBehaviour
    {
        private Transform _myTransform;
        private Transform target;
        
        private void Awake()
        {
            _myTransform = transform;
        }

        private void LateUpdate()
        {
            if (target == null)
                target = FindAnyObjectByType<DroneController>().transform;
            if (target != null)
            {
                var rotation = Quaternion.LookRotation (target.position - _myTransform.position);
                _myTransform.rotation = Quaternion.Slerp (transform.rotation, rotation, Time.deltaTime * 5);
            }
        }
    }
}