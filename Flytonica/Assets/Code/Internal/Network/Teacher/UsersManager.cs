using System;
using System.Collections.Generic;
using System.Linq;
using Code.Internal.API;
using Code.Internal.API.Wrappers;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using JetBrains.Annotations;

namespace Code.Internal.Network.Teacher
{
    public class UsersManager : NetworkBehaviour
    {
        public static UsersManager Instance;

        private readonly SyncDictionary<NetworkConnection, PlayerData> _playerDatas = new();
        private readonly SyncList<NetworkConnection> _teachers = new();

        //private readonly Dictionary<NetworkConnection, NetworkObject> _playerDrones = new();

        public SyncDictionary<NetworkConnection, PlayerData> AllPlayers => _playerDatas;
        public SyncList<NetworkConnection> Teachers => _teachers;

        public PlayerData[] LeaderboardResults => _playerDatas.Values.ToArray();

        public event Action OnPlayerListUpdated;

        private void Awake()
        {
            Instance = this;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            _playerDatas.Clear();
            _teachers.Clear();
        }

        public void AddPlayer(NetworkConnection connection, string playerName, NetworkObject drone)
        {
            if (_playerDatas.TryAdd(connection, new PlayerData(playerName, drone)))
                OnPlayerListUpdated?.Invoke();
        }

        [ServerRpc]
        public void AddTeacher(NetworkConnection connection)
        {
            if(!_teachers.Contains(connection))
                _teachers.Add(connection);
        }
        
        public void RemovePlayer(NetworkConnection connection)
        {
            if (_playerDatas.ContainsKey(connection))
            {
                OnPlayerListUpdated?.Invoke();
            }
        }
        
        [ServerRpc]
        public void RemoveTeacher(NetworkConnection connection)
        {
            if(_teachers.Contains(connection))
                _teachers.Remove(connection);
        }

        public void UpdateResult(NetworkConnection connection, float newTime, int newScore, bool isFinished)
        {
            if (!_playerDatas.TryGetValue(connection, out var result))
                return;

            result.Time = newTime;
            result.Score = newScore;
            result.IsFinished = isFinished;
        }
    }
    
    public class PlayerData
    {
        public string PlayerName;
        
        public NetworkObject Drone;
        public int Score;
        public float Time;
        public bool IsFinished;
        

        public PlayerData()
        {
        }

        public PlayerData(string playerName, NetworkObject drone)
        {
            PlayerName = playerName;
            Drone = drone;
        }

        public override string ToString()
        {
            return $"{PlayerName}. Time: {Time:F0}. Score: {Score}";
        }
    }
}