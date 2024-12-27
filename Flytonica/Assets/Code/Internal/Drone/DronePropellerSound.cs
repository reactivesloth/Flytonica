using UnityEngine;

namespace Code.Internal.Drone
{
    public class DronePropellerSound : MonoBehaviour
    {
        [SerializeField] private float minPitch = 0.2f;
        [SerializeField] private float maxPitch = 2.0f;
        [SerializeField] private AudioClip propellerLoop;

        private DroneController _controller;
        private AudioSource _source;

        private void Awake()
        {
            _controller = gameObject.GetComponent<DroneController>();
            _source = gameObject.GetComponentInChildren<AudioSource>();
            
            _source.loop = true;
            _source.Stop();
            _source.clip = propellerLoop;
        }

        private void Update()
        {
            if (_controller == null || _source == null)
                return;

            if (!_source.isPlaying)
                _source.Play();

            var power = _controller.GetRPM() / _controller.GetMaxRPM();

            if (float.IsInfinity(power) || float.IsNaN(power))
                power = 0;
            
            if (power < minPitch)
                _source.pitch = minPitch;
            else if (power > maxPitch)
                _source.pitch = maxPitch;
            else
                _source.pitch = power;
            
            _source.volume = power;
        }
    }
}