using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Code.Internal.Drone
{
    public class DroneCameraEffectController : MonoBehaviour
    {
        [SerializeField] private Volume volume;
        [Header("Settings")] [SerializeField] private float minGrainIntensity = .25f;
        [SerializeField] private float maxGrainIntensity = 1;
        [SerializeField] private float minChromaticIntensity = .1f, maxChromaticIntensity = 1;

        private FilmGrain _filmGrain;
        private LiftGammaGain _liftGammaGain;
        private ChromaticAberration _chromaticAberration;

        private DroneSensors CurrentDroneSensors => DroneController.Instance?.DroneSensors;

        private void Awake()
        {
            if (!volume) return;

            if (volume.profile.TryGet(out FilmGrain fg))
                _filmGrain = fg;

            if (volume.profile.TryGet(out LiftGammaGain lgg))
                _liftGammaGain = lgg;

            if (volume.profile.TryGet(out ChromaticAberration ca))
                _chromaticAberration = ca;
        }
        
        private void Update()
        {
            if (!CurrentDroneSensors) return;

            if (_filmGrain)
                _filmGrain.intensity.value = Mathf.Clamp(1f - CurrentDroneSensors.CameraSignal, minGrainIntensity,
                    maxGrainIntensity);

            if (_chromaticAberration)
                _chromaticAberration.intensity.value = Mathf.Clamp(1f - CurrentDroneSensors.CameraSignal, minChromaticIntensity,
                    maxChromaticIntensity);

            if (_liftGammaGain)
            {
                var value = _liftGammaGain.lift.value;
                value.w = CurrentDroneSensors.CameraSignal <= 0.01 ? 1 : 0;
                _liftGammaGain.lift.value = value;
            }
        }
    }
}