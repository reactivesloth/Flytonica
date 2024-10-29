using System;
using UnityEngine;
using Code.Internal.Scenario.Transport; // Добавьте этот неймспейс

namespace Code.Internal.Drone
{
    public class DroneCargoController : MonoBehaviour
    {
        [SerializeField] private new Rigidbody rigidbody;
        [SerializeField] private Transform attachPoint;
        
        private Rigidbody _currentCargo;

        public bool IsCargoAttached => _currentCargo != null;

        private void OnValidate()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (!DroneController.Instance)
                return;

            if (DroneInput.Instance.DropCargoButton)
                Drop();
        }
        

        public void Attach(Rigidbody cargo)
        {
            if (!cargo || IsCargoAttached)
                return;

            if(attachPoint)
            {
                cargo.transform.position = attachPoint.position;
                cargo.transform.rotation = attachPoint.rotation;
            }
            
            _currentCargo = cargo;
            var joint = cargo.gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = rigidbody;
            joint.connectedMassScale = 0.01f; //КОСТЫЛЬ ДЛЯ ПОЧИНКИ ФИЗИКИ
        }

        private void Drop()
        {
            if (!IsCargoAttached)
                return;
            
            var cargo = _currentCargo.GetComponent<CargoObject>();
            var fixedJoint = _currentCargo.GetComponent<FixedJoint>();
            
            if(!cargo || !fixedJoint)
                return;
            
            fixedJoint.connectedBody = null;
            Destroy(fixedJoint);
            cargo.OnDrop();

            _currentCargo = null;
        }

        public void OnReset()
        {
            if (!IsCargoAttached)
                return;
            
            var cargo = _currentCargo.GetComponent<CargoObject>();
            cargo.OnReset();
            Drop();
        }
    }
}
