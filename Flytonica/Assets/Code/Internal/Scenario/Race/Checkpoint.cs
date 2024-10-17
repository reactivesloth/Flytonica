using System;
using UnityEngine;
using Code.Internal.Drone;
using System.Collections.Generic;

namespace Code.Internal.Scenario.Race
{
    public enum CheckpointType
    {
        Start,
        Checkpoint,
        Finish
    }

    public enum CheckpointStatus
    {
        None,
        Current,
        Next,
        Passed
    }

    public class Checkpoint : MonoBehaviour
    {
        public CheckpointType checkpointType = CheckpointType.Checkpoint;
        public CheckpointStatus checkpointStatus { get; private set; } = CheckpointStatus.None;
        public float deviationFromCentre = -1f;

        [SerializeField] private MeshRenderer renderer;
        [SerializeField] private Material currentCheckpointMaterial;
        [SerializeField] private Material nextCheckpointMaterial;
        [SerializeField] private Material otherCheckpointMaterial;

        public void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out DroneController drone) || drone != DroneController.Instance) 
                return;

            if (checkpointStatus == CheckpointStatus.Current)
            {
                deviationFromCentre = CalculateDeviationFromCentre(drone.transform.position);
                ChangeStatus(CheckpointStatus.Passed);
                FindAnyObjectByType<ScenarioRace>().CheckpointUpdate(this);
                print($"Status = {checkpointStatus}, deviationFromCentre = {deviationFromCentre}");
            }
        }
        
        public void ChangeStatus(CheckpointStatus type)
        {
            if (renderer == null) return;

            checkpointStatus = type;

            switch (type)
            {
                case CheckpointStatus.None:
                case CheckpointStatus.Passed:
                    if (otherCheckpointMaterial != null)
                        renderer.sharedMaterial = otherCheckpointMaterial;
                    break;
                case CheckpointStatus.Current:
                    if (currentCheckpointMaterial != null)
                        renderer.sharedMaterial = currentCheckpointMaterial;
                    break;
                case CheckpointStatus.Next:
                    if (nextCheckpointMaterial != null)
                        renderer.sharedMaterial = nextCheckpointMaterial;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void SetEndColor(bool isPassed)
        {
            renderer.sharedMaterial = isPassed ? currentCheckpointMaterial : otherCheckpointMaterial;
        }
        
        private float CalculateDeviationFromCentre(Vector3 targetPosition)
        {
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                float sizeX = boxCollider.size.x * boxCollider.transform.lossyScale.x;
                float sizeY = boxCollider.size.y * boxCollider.transform.lossyScale.y;
                float sizeZ = boxCollider.size.z * boxCollider.transform.lossyScale.z;

                var sizes = new List<(float size, int axisIndex)>
                {
                    (sizeX, 0), // 0 - ось X
                    (sizeY, 1), // 1 - ось Y
                    (sizeZ, 2)  // 2 - ось Z
                };

                sizes.Sort((a, b) => a.size.CompareTo(b.size));

                int axisA = sizes[1].axisIndex;
                int axisB = sizes[2].axisIndex;
                float sizeA = sizes[1].size;
                float sizeB = sizes[2].size;

                Vector3 worldCenter = boxCollider.transform.TransformPoint(boxCollider.center);

                Vector3[] axes = new Vector3[3];
                axes[0] = boxCollider.transform.right;
                axes[1] = boxCollider.transform.up;
                axes[2] = boxCollider.transform.forward;

                Vector3 toDrone = targetPosition - worldCenter;
                float deviationA = Vector3.Dot(toDrone, axes[axisA]);
                float deviationB = Vector3.Dot(toDrone, axes[axisB]);

                float percentDeviationA = Mathf.Abs(deviationA) / (sizeA / 2f);
                float percentDeviationB = Mathf.Abs(deviationB) / (sizeB / 2f);

                return (percentDeviationA + percentDeviationB) / 2f;
            }

            return -1;
        }
    }
}
