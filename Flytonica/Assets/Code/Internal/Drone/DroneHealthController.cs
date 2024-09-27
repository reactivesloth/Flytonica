using System;
using System.Collections;
using Code.Internal.SceneManagement;
using UnityEditor;
using UnityEngine;

namespace Code.Internal.Drone
{
    [RequireComponent(typeof(Rigidbody))]
    public class DroneHealthController : MonoBehaviour
    {
        [SerializeField] private float timeOutSecs = 1f;
        
        private Rigidbody _rigidbody;
        
        private float _currentHealth = 100f;
        private float _damageThreshold = 2f;
        private float _maxImpactForce = 100f;
        
        private bool _isCanDamage = true;

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
            var impactForce = collision.relativeVelocity.magnitude * _rigidbody.mass;

            if (impactForce > _damageThreshold)
            {
                var damage = CalculateDamage(impactForce);
                ApplyDamage(damage);
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
                DestroyDrone();
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