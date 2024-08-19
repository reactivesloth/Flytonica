using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Internal.Drone
{
    [CreateAssetMenu(fileName = "Drone", menuName = "Flytoncia/Drones/Drone", order = 1)]
    public class DroneSettings : ScriptableObject
    {
        public GameObject prefab;
        public string modeName = "Drone";
        [Header("Body")] 
        public float weight = 0.8f;
        public AirframeType airframeType = AirframeType.QuadX;
        public DroneEngineSettings droneEngine;
        public DronePropellerSettings dronePropeller;
        
        [Header("Flight Modes")] public DroneFlightSettings[] flightModes;

        [FormerlySerializedAs("batteryVoltage_V")] [Header("Power")] 
        public float batteryVoltageV = 15.4f;
        public float batteryCapacityMah = 5000;
        public float batteryEnergyWh = 77;
        
        [Header("Safety\nLow Battery Failsafe Trigger")]
        public DroneFailsafeAction failsafeAction = DroneFailsafeAction.Warning;
        [Range(0, 100)] public int batteryWarnLevel = 15;
        [Range(0, 100)] public int batteryFailsafeLevel = 7;
        [Range(0, 100)] public int batteryEmergencyLevel = 5;

        [Header("RC Loss Failsafe Trigger")]
        public DroneFailsafeAction rCLossFailsafeTrigger = DroneFailsafeAction.Terminate;
        public float rCLossTimeout = 2;
    }

    public enum AirframeType
    {
        QuadPlus,
        QuadX,
        QuadH,
        QuadV,
        DJIMavic3
    }

    public enum DroneFailsafeAction
    {
        None,
        Warning,
        HoldMode,
        ReturnMode,
        LandMode,
        Disarm,
        Terminate
    }
}