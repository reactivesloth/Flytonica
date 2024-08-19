using UnityEngine;

namespace Code.Internal.Scenario.Race
{
    public enum WaypointType
    {
        Start,
        Checkpoint,
        Finish
    }
    
    public class Waypoint : MonoBehaviour
    {
        public WaypointType waypointType = WaypointType.Checkpoint;
        
        public void OnTriggerEnter(Collider other)
        {
            
        }
    }
}