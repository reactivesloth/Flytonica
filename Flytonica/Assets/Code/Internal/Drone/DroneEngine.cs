using System;
using UnityEngine;

namespace Code.Internal.Drone
{
    public class DroneEngine : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        private Transform _transform;
        
        private DroneSettings _droneSettings;
        
        private float _acceleration = 0;
        private float _control = 0;
        private bool _clockwise;
        private float _maxRPM = 0;
        private Vector3 lastError;
        private Vector3 totalError;
        
        private void Awake()
        {
            _transform = transform;
        }

        public void InitializeEngine(DroneSettings drone, bool clockwise)
        {
            _droneSettings = drone;
            _maxRPM = drone.droneEngine.kv * drone.batteryVoltageV;
            _clockwise = clockwise;
        }

        private void FixedUpdate()
        {
            if(!_rigidbody) return;
            if (!_droneSettings) return;
            
            var enterVelocity = 1;
            var rpm = _maxRPM * _acceleration;
            var propDiameterInches = _droneSettings.dronePropeller.propDiameterInches;
            var propPitchInches = _droneSettings.dronePropeller.propPitchInches;

            var thrust = (float) (4.392399f * Math.Pow(10, -8) * rpm * (Math.Pow(propDiameterInches, 3.5f) / Math.Sqrt(propPitchInches)) * (4.23333f * Math.Pow(10, -4) * rpm * propPitchInches * enterVelocity));
            var force = _transform.up * thrust;

            _rigidbody.AddForce(force, ForceMode.Force);

            var visualRpm = _maxRPM * _control * (_clockwise ? 1:-1);
            _transform.Rotate(new Vector3(0, visualRpm, 0) * Time.fixedDeltaTime, Space.Self);
        }

        public void UpdateEngine (Rigidbody rigidBody, float acceleration, float control)
        {
            _rigidbody = rigidBody;
            _acceleration = acceleration;
            _control = (_acceleration + control)/2;
        }
    }
}