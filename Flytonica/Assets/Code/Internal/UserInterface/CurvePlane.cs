using System;
using UnityEngine;

namespace Code.Internal.UserInterface
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class CurvePlane : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshCollider meshCollider;
        [SerializeField] private int xSize, ySize;
        [SerializeField] private float radius;

        private Vector3[] _vertices;
        private Mesh _mesh;

        private void OnValidate()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshCollider = GetComponent<MeshCollider>();
            Generate();
        }

        private void Start()
        {
            Generate();
        }

        private void FixedUpdate()
        {
            Generate();
        }

        private void Generate()
        {
            meshFilter.mesh = _mesh = new Mesh();
            _mesh.name = "Procedural Grid";

            _vertices = new Vector3[(xSize + 1) * (ySize + 1)];
            var uv = new Vector2[_vertices.Length];

            float maxY = ySize > 0 ? (float)ySize : 1; // Avoid division by zero
            float maxX = xSize > 0 ? (float)xSize : 1; // Normalize X axis size

            for (int i = 0, y = 0; y <= ySize; y++)
            {
                for (int x = 0; x <= xSize; x++, i++)
                {
                    float xPos = (float)x / maxX; // Normalize x value to be between 0 and 1
                    float angle = Mathf.PI * (xPos - 0.5f);
                    float curveZ = -radius * (1 - Mathf.Cos(angle));
                    float normalizedY = (float)y / maxY; // Normalize y value to be between 0 and 1

                    _vertices[i] = new Vector3(xPos, normalizedY, curveZ);
                    uv[i] = new Vector2((float)x / xSize, (float)y / ySize);
                }
            }

            _mesh.vertices = _vertices;
            _mesh.uv = uv;

            var triangles = new int[xSize * ySize * 6];

            for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)
            {
                for (int x = 0; x < xSize; x++, ti += 6, vi++)
                {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                    triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                    triangles[ti + 5] = vi + xSize + 2;
                }
            }

            _mesh.triangles = triangles;
            _mesh.RecalculateNormals();
            meshCollider.sharedMesh = _mesh;
        }
    }
}