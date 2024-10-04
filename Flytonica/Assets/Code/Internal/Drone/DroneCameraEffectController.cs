using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Code.Internal.Drone
{
    public class DroneCameraEffectController : MonoBehaviour
    {
        public static DroneCameraEffectController Instance { get; private set; }
        
        [SerializeField] private Volume volume;
        [Header("Settings")] 
        [SerializeField] private float minOffsetCamera = 0.1f;
        [SerializeField] private float minGrainIntensity = .25f;
        [SerializeField] private float maxGrainIntensity = 1;
        [SerializeField] private float minChromaticIntensity = .1f, maxChromaticIntensity = 1;

        private FilmGrain _filmGrain;
        private LiftGammaGain _liftGammaGain;
        private ChromaticAberration _chromaticAberration;
        private ColorAdjustments _colorAdjustments;

        private DroneSensors CurrentDroneSensors => DroneController.Instance?.DroneSensors;

        private void Awake()
        {
            Instance = this;
            
            if (!volume) return;

            if (volume.profile.TryGet(out FilmGrain fg))
                _filmGrain = fg;

            if (volume.profile.TryGet(out LiftGammaGain lgg))
                _liftGammaGain = lgg;

            if (volume.profile.TryGet(out ChromaticAberration ca))
                _chromaticAberration = ca;
            
            if (volume.profile.TryGet(out ColorAdjustments component))
                _colorAdjustments = component;
        }
        
        private void Update()
        {
            if (!CurrentDroneSensors) return;

            if (_filmGrain)
                _filmGrain.intensity.value = Mathf.Clamp(1f - CurrentDroneSensors.CameraSignal - minOffsetCamera, minGrainIntensity,
                    maxGrainIntensity);

            if (_chromaticAberration)
                _chromaticAberration.intensity.value = Mathf.Clamp(1f - CurrentDroneSensors.CameraSignal - minOffsetCamera, minChromaticIntensity,
                    maxChromaticIntensity);

            if (_liftGammaGain)
            {
                var value = _liftGammaGain.lift.value;
                value.w = CurrentDroneSensors.CameraSignal <= 0.01 ? 1 : 0;
                _liftGammaGain.lift.value = value;
            }
        }

        public void SetIrMode(bool value) => _colorAdjustments.active = value;
    }
}