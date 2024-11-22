using System;
using System.Collections;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface;
using FishNet.Object;
using UnityEditor;
using UnityEngine;
using MessageType = Code.Internal.UserInterface.DroneHudElements.MessageType;

namespace Code.Internal.Drone
{
    [RequireComponent(typeof(Rigidbody))]
    public class DroneHealthController : NetworkBehaviour
    {
        [SerializeField] private AudioClip hitMessageClip;
        [SerializeField] private float timeOutSecs = 1f;
        
        private Rigidbody _rigidbody;

        [SerializeField] private float _damageMultiplier = 2;
        [SerializeField] private float _currentHealth = 100f;
        private float _damageThreshold = 2f;
        private float _maxImpactForce = 100f;
        
        private bool _isCanDamage = true;
        private DroneSensors CurrentDroneSensors => DroneController.Instance != null ? DroneController.Instance.DroneSensors : null;

        private DroneSettings Settings => GetComponent<DroneController>().Settings;

        public event Action OnObjectCollision, OnMenCollision, OnAnimalCollision, OnBirdCollision;

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
                var damage = CalculateDamage(_rigidbody.linearVelocity.magnitude * 3.6f);
                ApplyDamage(damage * _damageMultiplier);
                
                //TODO: вызов событий столкновения с различными объектами для статистики
                OnObjectCollision?.Invoke();
            }
        }
        
        private float CalculateDamage(float impactForce)
        {
            var normalizedForce = Mathf.Clamp01(impactForce / _maxImpactForce);
            return normalizedForce * 100f;
        }

        public void ApplyDamage(float damage)
        {
            if(!_isCanDamage)
                return;
            
            _currentHealth -= damage;
            if (hitMessageClip != null)
                DroneHUD.Instance.SetMessage(MessageType.Error, "Внимание! Произошло столкновение!", hitMessageClip.length, hitMessageClip);

            if (_currentHealth <= 0)
            {
                CurrentDroneSensors.CameraSignalModifier = 0;
                CurrentDroneSensors.InputSignalModifier = 0;
                if (!IsInvoking(nameof(DestroyDrone)))
                {
                    Invoke(nameof(DestroyDrone), 1);
                }
            }
            else
                StartCoroutine(Timer());
        }

        private void Update()
        {
            if(IsOwner && CurrentDroneSensors)
                CurrentDroneSensors.Health = _currentHealth;
        }

        private void DestroyDrone()
        {
            DroneHUD.Instance.SetMessage(MessageType.Error,"Внимание! Произошло столкновение! Задание провалено.", 1f);
            DroneController.Instance.ResetDrone();
            _currentHealth = Settings.healthPoints;
            CurrentDroneSensors.CameraSignalModifier = 1;
            CurrentDroneSensors.InputSignalModifier = 1;
        }

        private IEnumerator Timer()
        {
            _isCanDamage = false;
            yield return new WaitForSeconds(timeOutSecs);
            _isCanDamage = true;
        }
    }
}