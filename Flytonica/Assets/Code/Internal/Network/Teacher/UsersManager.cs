using System;
using System.Collections.Generic;
using System.Linq;
using Code.Internal.API;
using Code.Internal.API.Wrappers;
using Code.Internal.Drone;
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
        public event Action<NetworkConnection> OnHelpSignal;

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

        public void RemovePlayer(NetworkConnection connection)
        {
            if (_playerDatas.ContainsKey(connection))
            {
                OnPlayerListUpdated?.Invoke();
            }
        }

        public void UpdateResult(NetworkConnection connection, float newTime, int newScore, bool isFinished)
        {
            if (!_playerDatas.TryGetValue(connection, out var data))
                return;

            data.Time = newTime;
            data.Score = newScore;
            data.IsFinished = isFinished;
        }

        public void HelpSignal(NetworkConnection sender)
        {
            if (AllPlayers.ContainsKey(sender))
                OnHelpSignal?.Invoke(sender);
        }

        public void ResetPlayer(NetworkConnection connection)
        {
            if(!_playerDatas.TryGetValue(connection, out var data))return;
            
            ResetPlayerRpc(connection);
        }

        [TargetRpc]
        protected void ResetPlayerRpc(NetworkConnection connection)
        {
            DroneController.Instance?.ResetDrone();
            //ConnectionController.Instance?.MovePlayer(connection, );
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