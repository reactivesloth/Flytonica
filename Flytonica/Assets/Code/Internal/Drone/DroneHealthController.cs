using System;
using System.Collections;
using Code.Internal.SceneManagement;
using UnityEngine;

namespace Code.Internal.Drone
{
    [RequireComponent(typeof(Rigidbody))]
    public class DroneHealthController : MonoBehaviour
    {
        [SerializeField] private float damageThreshold = 2f;
        [SerializeField] private float maxImpactForce = 100f;
        [SerializeField] private float timeOutSecs = 1f;

        private Rigidbody _rigidbody;
        private float _currentHealth = 100f;
        private bool _isCanDamage = true;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            var impactForce = collision.relativeVelocity.magnitude * _rigidbody.mass;

            if (impactForce > damageThreshold)
            {
                var damage = CalculateDamage(impactForce);
                ApplyDamage(damage);
            }
        }
        
        private float CalculateDamage(float impactForce)
        {
            var normalizedForce = Mathf.Clamp01(impactForce / maxImpactForce);
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