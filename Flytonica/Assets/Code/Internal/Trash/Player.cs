using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Code.Internal.Trash
{
    public class Player: NetworkBehaviour
    {
        [SerializeField] private float speed;
        [SerializeField] private MeshRenderer rendered;

        private readonly SyncVar<Color> _color = new();

        private void Awake()
        {
            _color.OnChange += OnColorChange;
        }

        public void Move(Vector3 direction)
        {
            var move = transform.TransformDirection(direction * speed * Time.deltaTime);
            transform.position += move;
        }

        [ServerRpc]
        public void SetColor(Color newColor) => _color.Value = newColor;

        private void OnColorChange(Color prev, Color next, bool asServer)
        {
            rendered.material.color = next;
        }
    }
}