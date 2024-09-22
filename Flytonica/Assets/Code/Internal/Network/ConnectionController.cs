using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Code.Internal.Scenario;
using Code.Internal.SceneManagement;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

namespace Code.Internal.Network
{
    public class ConnectionController : NetworkBehaviour
    {
        [SerializeField] private SceneLoadingSettings sceneSettings;
        [SerializeField] private AvailableScenariosSettings scenarios;

        private bool _sceneLoaded;
        private readonly List<NetworkConnection> _pendingConnections = new();

        public override void OnStartServer()
        {
            base.OnStartServer();
            ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
            print(sceneSettings.currentScenario.currentMap);
            GameSceneManager.Instance.LoadGlobalScene(sceneSettings.currentScenario.currentMap, OnSceneLoaded);
        }
        
        public override void OnStopServer()
        {
            base.OnStopServer();
            ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
            _sceneLoaded = false;
        }

        private void OnRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs args)
        {
            switch (args.ConnectionState)
            {
                case RemoteConnectionState.Started:
                    if (_sceneLoaded)
                        OnConnectedPlayer(connection);
                    else
                        _pendingConnections.Add(connection);
                    break;
                case RemoteConnectionState.Stopped:
                    OnDisconnectedPlayer(connection);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnConnectedPlayer(NetworkConnection connection)
        {
            var drone = NetworkManager.GetComponent<PlayersSpawner>().Spawn(connection, sceneSettings.currentScenario.currentDrone);
            InvokeTargetInitializeScenario(connection);
        }
        
        private void OnDisconnectedPlayer(NetworkConnection connection)
        {
            _pendingConnections.Remove(connection);
            NetworkManager.GetComponent<PlayersSpawner>().Despawn(connection);
        }
        
        private void OnSceneLoaded()
        {
            _sceneLoaded = true;
            FindAnyObjectByType<ScenarioInitializer>().Initialize(sceneSettings.currentScenario);

            foreach (var connection in _pendingConnections)
            {
                OnConnectedPlayer(connection);
            }

            _pendingConnections.Clear();
        }

        private async void InvokeTargetInitializeScenario(NetworkConnection connection)
        {
            await Task.Delay(1000);
            TargetInitializeScenario(connection, sceneSettings.currentScenarioCollection.name);
        }

        [TargetRpc]
        private void TargetInitializeScenario(NetworkConnection connection, string scenarioName)
        {
            Debug.Log($"Init scenario for connection {connection.ClientId}");
            
            /*var scenarioSettings = scenarios.scenarios.First(s => s.name == scenarioName);
            var scenarioInitializer = FindAnyObjectByType<ScenarioInitializer>();
            if (scenarioInitializer != null && scenarioSettings != null)
            {
                scenarioInitializer.Initialize(scenarioSettings);
            }
            else
            {
                Debug.LogError("ScenarioInitializer not found on the client.");
            }*/
        }
    }
}