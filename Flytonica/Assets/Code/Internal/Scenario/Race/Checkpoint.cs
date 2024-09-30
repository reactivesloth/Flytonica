using System;
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

    public enum CheckpointFlashType
    {
        None,
        Current,
        Next
    }


    public class Checkpoint : MonoBehaviour
    {
        public CheckpointType checkpointType = CheckpointType.Checkpoint;

        [SerializeField] private MeshRenderer renderer;
        [SerializeField] private Material currentCheckpointMaterial;
        [SerializeField] private Material nextCheckpointMaterial;
        [SerializeField] private Material otherCheckpointMaterial;

        public void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out DroneController drone) && drone == DroneController.Instance)
            {
                FindAnyObjectByType<ScenarioRace>().CheckpointUpdate(this);
            }
        }

        public void ChangeColor(CheckpointFlashType type)
        {
            if (renderer == null) return;

            switch (type)
            {
                case CheckpointFlashType.None:
                    if (otherCheckpointMaterial != null)
                        renderer.sharedMaterial = otherCheckpointMaterial;
                    break;
                case CheckpointFlashType.Current:
                    if (currentCheckpointMaterial != null)
                        renderer.sharedMaterial = currentCheckpointMaterial;
                    break;
                case CheckpointFlashType.Next:
                    if (nextCheckpointMaterial != null)
                        renderer.sharedMaterial = nextCheckpointMaterial;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}