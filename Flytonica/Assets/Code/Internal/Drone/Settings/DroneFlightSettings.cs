using GameKit.Dependencies.Inspectors;
using UnityEngine;

namespace Code.Internal.Drone
{
    [CreateAssetMenu(fileName = "Flight Mode", menuName = "Drones/Fligh Mode", order = 1)]
    public class DroneFlightSettings : ScriptableObject
    {
        public string modeName = "Flight Mode";

        [Header("Throttle")] 
        public ControlType throttleType = ControlType.STABILIZED;
        [ShowIf("throttleType", ControlType.STABILIZED)] public float maxAscendingSpeed = 1;
        [ShowIf("throttleType", ControlType.STABILIZED)] public float maxDescendingSpeed = 1;
        [ShowIf("throttleType", ControlType.STABILIZED)] public float maxHeight = 6000;
        [ShowIf("throttleType", ControlType.MANUAL)] public AnimationCurve accelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);
        
        
        [Header("Yaw")]
        [Range(1, 360)] public float maxAngularSpeed = 200;
        
        [Header("Roll/Pitch")]
        public ControlType rotatingType = ControlType.STABILIZED;
        public float maxStabilizedSpeed = 5;
        public float maxStabilizedAngle = 25;
        [ShowIf("rotatingType", ControlType.MIXED)] [Range(0, 1)] public float axisModeChangeValue = 0.5f;

    }

    public enum ControlType 
    {
        MANUAL,
        STABILIZED,
        MIXED
    }
}