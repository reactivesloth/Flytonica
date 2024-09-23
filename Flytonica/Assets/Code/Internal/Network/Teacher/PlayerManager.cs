using System;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

namespace Code.Internal.Network.Teacher
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance;

        private readonly Dictionary<NetworkConnection, NetworkObject> _playerDrones = new();

        public event Action OnPlayerListUpdated;

        private void Awake()
        {
            Instance = this;
        }
        
        public void AddPlayer(NetworkConnection connection, NetworkObject drone)
        {
            if (_playerDrones.TryAdd(connection, drone))
            {
                OnPlayerListUpdated?.Invoke();
            }
        }

        public void RemovePlayer(NetworkConnection connection)
        {
            if (_playerDrones.ContainsKey(connection))
            {
                _playerDrones.Remove(connection);
                OnPlayerListUpdated?.Invoke();
            }
        }

        public Dictionary<NetworkConnection, NetworkObject> GetAllPlayers()
        {
            return _playerDrones;
        }
    }
}