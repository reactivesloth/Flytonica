using UnityEngine;
using Code.Internal.Drone;

namespace Code.Internal.Scenario.Race
{
    public enum CheckpointType
    {
        Start,
        Checkpoint,
        Finish
    }
    
    public class Checkpoint : MonoBehaviour
    {
        public CheckpointType checkpointType = CheckpointType.Checkpoint;
        
        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponent<DroneController>())
            {
                FindAnyObjectByType<ScenarioRace>().CheckpointUpdate(this);
            }
        }
    }
}