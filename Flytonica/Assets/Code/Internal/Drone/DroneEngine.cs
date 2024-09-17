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

        private float _voltage;
        
        private void Awake()
        {
            _transform = transform;
        }

        public void InitializeEngine(DroneSettings drone, bool clockwise)
        {
            _droneSettings = drone;
            _clockwise = clockwise;
        }

        private void FixedUpdate()
        {
            if(!_rigidbody) return;
            if (!_droneSettings) return;
            
            var enterVelocity = 1;
            _maxRPM = _droneSettings.droneEngine.kv * _voltage * _droneSettings.bateteryCellCount;
            var rpm = _maxRPM * _acceleration;
            var propDiameterInches = _droneSettings.dronePropeller.propDiameterInches;
            var propPitchInches = _droneSettings.dronePropeller.propPitchInches;

            var thrust = (float) (4.392399f * Math.Pow(10, -8) * rpm * (Math.Pow(propDiameterInches, 3.5f) / Math.Sqrt(propPitchInches)) * (4.23333f * Math.Pow(10, -4) * rpm * propPitchInches * enterVelocity));
            
            var upVec = transform.up;
            upVec.x = 0;
            upVec.z = 0;
            float diff = 1 - upVec.magnitude;
            var force = _transform.up * (thrust + diff);
            
            _rigidbody.AddForce(force * Time.deltaTime, ForceMode.Impulse);

            var visualRpm = _maxRPM * _control * (_clockwise ? 1:-1);
            _transform.Rotate(new Vector3(0, visualRpm, 0) * Time.fixedDeltaTime, Space.Self);
        }

        public void UpdateEngine (Rigidbody rigidBody, float voltage, float acceleration, float control)
        {
            _rigidbody = rigidBody;
            _acceleration = acceleration;
            _control = Mathf.Abs(_acceleration + control);
            _voltage = voltage;
        }

        public float GetRPM()
        {
            return _maxRPM * _control;
        }

        public float MaxRPM => _maxRPM;
    }
}