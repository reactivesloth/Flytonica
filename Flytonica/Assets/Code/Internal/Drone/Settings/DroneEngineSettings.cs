using UnityEngine;

namespace Code.Internal.Drone
{
    [CreateAssetMenu(fileName = "Engine", menuName = "Flytoncia/Drones/Engine", order = 1)]
    public class DroneEngineSettings : ScriptableObject
    {
        public new string name = "Drone Engine";
        
        public enum DroneEngineType
        {
            Brushless,
            Brushed
        }

        public DroneEngineType droneEngineType = DroneEngineType.Brushless;
        public int model = 2008;
        public float kv = 1400;
    }
}