using System;
using UnityEngine;

namespace Code.Internal.Scenario.Transport
{
    public enum CargoState
    {
        NotAttached,
        Attached,
        Delivered,
        Lost
    }

    public class CargoObject : MonoBehaviour
    {
        [SerializeField] private new Rigidbody rigidbody;

        public CargoState state = CargoState.NotAttached;

        private TakeZone _ownerTakeZone;

        public byte ConnectionId { get; private set; }

        public float collisionForceThreshold = 1f;

        private void OnValidate()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision other)
        {
            float collisionForce = other.relativeVelocity.magnitude;

            switch (state)
            {
                case CargoState.NotAttached:
                    if (other.gameObject.TryGetComponent(out DropZone _) ||
                        other.gameObject.TryGetComponent(out TakeZone _))
                    {
                        if (collisionForce > collisionForceThreshold)
                            _ownerTakeZone.CargoFall();
                        break;
                    }

                    _ownerTakeZone.CargoFall();
                    Lost();

                    break;
                case CargoState.Attached:
                    _ownerTakeZone.CargoCollision();
                    break;
                case CargoState.Delivered:
                    break;
                case CargoState.Lost:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Lost()
        {
            state = CargoState.Lost;
            _ownerTakeZone.Respawn();
        }

        public void Init(TakeZone takeZone)
        {
            _ownerTakeZone = takeZone;
            ConnectionId = _ownerTakeZone.ConnectionId;
        }

        public void SetDelivery()
        {
            if (state == CargoState.Delivered)
                return;
            state = CargoState.Delivered;
        }

        public void OnAttach()
        {
            //rigidbody.useGravity = false;
            state = CargoState.Attached;
        }

        public void OnDrop()
        {
            //rigidbody.useGravity = true;
            state = CargoState.NotAttached;
        }

        public void OnReset()
        {
            if (rigidbody)
                rigidbody.linearVelocity = Vector3.zero;

            OnDrop();
            _ownerTakeZone.OnReset();
        }
    }
}