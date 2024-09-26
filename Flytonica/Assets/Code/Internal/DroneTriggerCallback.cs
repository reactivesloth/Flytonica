using System;
using Code.Internal.Drone;
using UnityEngine;

namespace Code.Internal
{
    [RequireComponent(typeof(Collider))]
    public class DroneTriggerCallback : MonoBehaviour
    {
        [SerializeField] private new Collider collider;

        private bool _isDroneInZone;
        
        public event Action OnDroneEnter, OnDroneExit;

        public bool IsDroneInZone => _isDroneInZone;
        
        private void OnValidate()
        {
            collider = GetComponent<Collider>();
            collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out DroneController droneController) ||
                droneController != DroneController.Instance)
                return;
            
            OnDroneEnter?.Invoke();
            _isDroneInZone = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out DroneController droneController) ||
                droneController != DroneController.Instance) 
                return;
            
            OnDroneExit?.Invoke();
            _isDroneInZone = false;
        }
    }
}