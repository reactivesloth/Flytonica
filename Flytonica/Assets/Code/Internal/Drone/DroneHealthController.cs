using System.Collections;
using Code.Internal.SceneManagement;
using UnityEngine;

namespace Code.Internal.Drone
{
    [RequireComponent(typeof(Rigidbody))]
    public class DroneHealthController : MonoBehaviour
    {
        [SerializeField] private float timeOutSecs = 1f;
        
        private Rigidbody _rigidbody;

        [SerializeField] private float _damageMultiplier = 2;
        [SerializeField] private float _currentHealth = 100f;
        private float _damageThreshold = 2f;
        private float _maxImpactForce = 100f;
        
        private bool _isCanDamage = true;
        private DroneSensors CurrentDroneSensors => DroneController.Instance.DroneSensors;

        private DroneSettings Settings => GetComponent<DroneController>().Settings;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _currentHealth = Settings.healthPoints;
            _damageThreshold = Settings.damageThreshold;
            _maxImpactForce = Settings.maxImpactForce;
        }

        private void OnCollisionEnter(Collision collision)
        {
            var impactForce = _rigidbody.linearVelocity.magnitude;

            if (impactForce > _damageThreshold)
            {
                var damage = CalculateDamage(impactForce);
                ApplyDamage(damage * _damageMultiplier);
            }
        }
        
        private float CalculateDamage(float impactForce)
        {
            var normalizedForce = Mathf.Clamp01(impactForce / _maxImpactForce);
            return normalizedForce * 100f;
        }

        private void ApplyDamage(float damage)
        {
            if(!_isCanDamage)
                return;
            
            _currentHealth -= damage;
            Debug.Log($"Drone received {damage} damage. Current health: {_currentHealth}");

            if (_currentHealth <= 0)
            {
                CurrentDroneSensors.CameraSignalModifier = 0;
                CurrentDroneSensors.InputSignalModifier = 0;
                if (!IsInvoking("DestroyDrone"))
                {
                    Invoke("DestroyDrone", 1);
                }
            }
            else
                StartCoroutine(Timer());
        }

        private void DestroyDrone()
        {
            Debug.Log("Drone destroyed!");
            GameSceneManager.Instance.Replay();
        }

        private IEnumerator Timer()
        {
            _isCanDamage = false;
            yield return new WaitForSeconds(timeOutSecs);
            _isCanDamage = true;
        }
    }
}