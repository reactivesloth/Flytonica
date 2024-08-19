using UnityEngine;

namespace Code.Internal.Drone
{
    [CreateAssetMenu(fileName = "Propeller", menuName = "Flytoncia/Drones/Propeller", order = 1)]
    public class DronePropellerSettings : ScriptableObject
    {
        public new string name = "Drone Propeller";
        
        public float propDiameterInches = 9.4f;
        public float propPitchInches = 5.3f;
    }
}