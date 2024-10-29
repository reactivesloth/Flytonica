using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;

namespace Code.Internal.Network.Teacher
{
    public class PlayerManager : NetworkBehaviour
    {
        public static PlayerManager Instance;

        private readonly SyncDictionary<NetworkConnection, PlayerResultInLeaderboard> _playerResults = new();

        private readonly Dictionary<NetworkConnection, NetworkObject> _playerDrones = new();

        public Dictionary<NetworkConnection, NetworkObject> AllPlayers => _playerDrones;

        public PlayerResultInLeaderboard[] LeaderboardResults => _playerResults.Values.ToArray();

        public event Action OnPlayerListUpdated;

        private void Awake()
        {
            Instance = this;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            _playerResults.Clear();
        }

        public void AddPlayer(NetworkConnection connection, NetworkObject drone)
        {
            if (_playerDrones.TryAdd(connection, drone))
            {
                OnPlayerListUpdated?.Invoke();
            }

            _playerResults.TryAdd(connection, new PlayerResultInLeaderboard($"Player {connection.ClientId}"));
        }

        public void RemovePlayer(NetworkConnection connection)
        {
            if (_playerDrones.ContainsKey(connection))
            {
                _playerDrones.Remove(connection);
                OnPlayerListUpdated?.Invoke();
            }
        }

        public void UpdateResultTime(NetworkConnection connection, float newTime, int newScore, bool isFinished)
        {
            if (!_playerResults.TryGetValue(connection, out var result))
                return;

            result.Time = newTime;
            result.Score = newScore;
            result.IsFinished = isFinished;
        }
    }

    [Serializable]
    public class PlayerResultInLeaderboard
    {
        public string PlayerName;
        public int Score;
        public float Time;
        public bool IsFinished;

        public PlayerResultInLeaderboard()
        {
        }

        public PlayerResultInLeaderboard(string playerName)
        {
            PlayerName = playerName;
        }

        public override string ToString()
        {
            return $"{PlayerName}. Time: {Time:F0}. Score: {Score}";
        }
    }
}