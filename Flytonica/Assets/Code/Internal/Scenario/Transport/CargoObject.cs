using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.Internal.Scenario.Transport
{
    public enum CargoState
    {
        NotAttached,
        Attached,
        Delivered
    }
    
    public class CargoObject: MonoBehaviour
    {
        [SerializeField] private Rigidbody rigidbody;
        
        private FixedJoint _fixedJoint;

        public CargoState State = CargoState.NotAttached;
        [NonSerialized] public float Precision = 0;

        private TakeZone _ownerTakeZone;
        
        public byte ConnectionId { get; private set; }

        public event Action<CargoObject> Delivered;

        private void OnValidate()
        {
            _fixedJoint = GetComponent<FixedJoint>();
        }

        public void Init(TakeZone takeZone)
        {
            _ownerTakeZone = takeZone;
            ConnectionId = _ownerTakeZone.ConnectionId;
        }
        
        public void SetDelivery()
        {
            if(State == CargoState.Delivered) 
                return;
            
            State = CargoState.Delivered;
            Delivered?.Invoke(this);
        }

        public void OnAttach()
        {
            State = CargoState.Attached;
        }

        public void OnDrop()
        {
            State = CargoState.NotAttached;
        }

        public void OnReset()
        {
            if(rigidbody)
                rigidbody.linearVelocity = Vector3.zero;
            
            OnDrop();
            _ownerTakeZone.OnReset();
            
        }
    }
}