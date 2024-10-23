using System;
using UnityEngine;

namespace Code.Internal.Scenario.Transport
{
    public enum DropZoneType
    {
        CenterTarget,
        BasketTarget
    }

    public class DropZone : MonoBehaviour
    {
        [SerializeField] private DropZoneType type;
        [SerializeField] private byte connectionId;

        private Collider _platformCollider;

        public byte ConnectionId => connectionId;

        private void Awake()
        {
            _platformCollider = GetComponent<Collider>();
        }

        private void OnCollisionEnter(Collision other)
        {
            print(other.gameObject.name);
            print($"{other.gameObject.TryGetComponent(out CargoObject c)}");
            print($"{connectionId}=={c.ConnectionId}");

            if (!other.gameObject.TryGetComponent(out CargoObject cargoObject) ||
                cargoObject.ConnectionId != connectionId) return;
            // Get the object's collider to determine its shape and size
            Collider objectCollider = other.collider;

            // Calculate the center point of the object at the time of collision
            Vector3 objectCenter = other.transform.position;

            cargoObject.Precision = type switch
            {
                DropZoneType.CenterTarget => CalculateCenterTarget(objectCenter, objectCollider),
                DropZoneType.BasketTarget => CalculateBasketTarget(objectCenter, objectCollider),
                _ => throw new ArgumentOutOfRangeException()
            };

            cargoObject.SetDelivery();
        }

        private float CalculateCenterTarget(Vector3 objectCenter, Collider objectCollider)
        {
            Vector3 platformCenter = transform.position;

            // Calculate the effective distance by adjusting for the object's size
            float distance = CalculateEffectiveDistance(platformCenter, objectCenter, objectCollider);

            // Get the maximum possible distance from the center to the edge, adjusting for object size
            float maxDistance = GetMaxDistance(objectCollider);

            // Normalize the distance
            float normalizedDistance = Mathf.Clamp01(distance / maxDistance);

            // For CenterTarget, precision is higher when closer to the center
            return 1 - normalizedDistance;
        }

        private float CalculateBasketTarget(Vector3 objectCenter, Collider objectCollider)
        {
            Vector3 platformCenter = transform.position;

            float distance = CalculateEffectiveDistance(platformCenter, objectCenter, objectCollider);

            float maxDistance = GetMaxDistance(objectCollider);

            float normalizedDistance = Mathf.Clamp01(distance / maxDistance);

            // For EdgeTarget, precision is higher when closer to the edge
            return normalizedDistance;
        }

        private float CalculateEffectiveDistance(Vector3 platformCenter, Vector3 objectCenter, Collider objectCollider)
        {
            // Calculate horizontal distance (ignore y-axis)
            Vector2 platformCenter2D = new Vector2(platformCenter.x, platformCenter.z);
            Vector2 objectCenter2D = new Vector2(objectCenter.x, objectCenter.z);
            float distance = Vector2.Distance(platformCenter2D, objectCenter2D);

            // Adjust the distance based on the object's size
            float objectRadius = GetObjectRadius(objectCollider);

            // Subtract the object's radius to get the distance from the platform center to the edge of the object
            float effectiveDistance = Mathf.Max(0, distance - objectRadius);

            return effectiveDistance;
        }

        private float GetMaxDistance(Collider objectCollider)
        {
            // Get the platform's maximum radius (half of the diagonal of its top surface)
            Vector3 platformSize = ((BoxCollider)_platformCollider).size;
            float platformWidth = platformSize.x * transform.localScale.x;
            float platformDepth = platformSize.z * transform.localScale.z;
            float platformRadius = Mathf.Sqrt(platformWidth * platformWidth + platformDepth * platformDepth) * 0.5f;

            // Adjust the maximum distance by subtracting the object's radius
            float objectRadius = GetObjectRadius(objectCollider);

            return Mathf.Max(0, platformRadius - objectRadius);
        }

        private float GetObjectRadius(Collider objectCollider)
        {
            if (objectCollider is SphereCollider sphereCollider)
            {
                // For spheres, use the radius and adjust for scaling
                return sphereCollider.radius * Mathf.Max(
                    objectCollider.transform.localScale.x,
                    objectCollider.transform.localScale.y,
                    objectCollider.transform.localScale.z);
            }
            else if (objectCollider is BoxCollider boxCollider)
            {
                // For boxes, approximate the radius as half the diagonal of the box's horizontal dimensions
                Vector3 size = boxCollider.size;
                float width = size.x * objectCollider.transform.localScale.x;
                float depth = size.z * objectCollider.transform.localScale.z;
                return Mathf.Sqrt(width * width + depth * depth) * 0.5f;
            }
            else
            {
                // For other collider types, you might need a different approach
                return 0f;
            }
        }
    }
}