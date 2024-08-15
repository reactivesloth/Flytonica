using System;
using System.Collections.Generic;
using System.Linq;
using Code.Internal.SceneManagement;
using FishNet;
using FishNet.Connection;
using FishNet.Discovery;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using UnityEngine;

namespace Code.Internal.Network
{
    public class Lobby : NetworkBehaviour
    {
        [SerializeField] public bool isSingleGame;
        
        private readonly SyncList<NetworkConnection> _activeConnections = new ();

        public event Action<List<NetworkConnection>> OnComplete;
        public event Action<NetworkConnection> OnNewUserConnected;
        public event Action<NetworkConnection> OnUserDisconnected;

        public List<NetworkConnection> ActiveConnections => new(_activeConnections.ToList());

        private void Awake()
        {
            if(isSingleGame)
                StartSingle();
        }

        private void StartSingle()
        {
            InstanceFinder.NetworkManager.GetComponent<NetworkDiscovery>().enabled = false;
            InstanceFinder.ClientManager.OnConnectedClients += ClientManagerOnOnConnectedClients;
            InstanceFinder.ServerManager.StartConnection();
            InstanceFinder.ClientManager.StartConnection();
        }

        private void ClientManagerOnOnConnectedClients(ConnectedClientsArgs obj)
        {
            GameSceneManager.Instance.LoadGame();
            InstanceFinder.ClientManager.OnConnectedClients -= ClientManagerOnOnConnectedClients;
        }

        public override void OnStartClient()
        {
            _activeConnections.OnChange += ActiveConnectionsOnOnChange;
            base.OnStartClient();
        }

        public override void OnStopClient()
        {
            _activeConnections.OnChange += ActiveConnectionsOnOnChange;
            base.OnStopClient();
        }

        public override void OnStartServer()
        {
            InstanceFinder.ServerManager.OnRemoteConnectionState += OnClientConnected;
            base.OnStartServer();
        }

        public override void OnStopServer()
        {
            InstanceFinder.ServerManager.OnRemoteConnectionState -= OnClientConnected;
            base.OnStopServer();
        }
        
        private void OnClientConnected(NetworkConnection conn, RemoteConnectionStateArgs args)
        {
            if (args.ConnectionState == RemoteConnectionState.Started)
            {
                print($"Connection! ID: {conn.ClientId}");
                _activeConnections.Add(conn);
            }
            else if (_activeConnections.Contains(conn))
            {
                print($"Disconnected! ID: {conn.ClientId}");
                _activeConnections.RemoveAt(_activeConnections.IndexOf(conn));
            }
        }
        
        private void ActiveConnectionsOnOnChange(SyncListOperation op, int index, NetworkConnection oldItem, NetworkConnection newItem, bool asServer)
        {
            
            switch (op)
            {
                case SyncListOperation.Add:
                    OnNewUserConnected?.Invoke(newItem);
                    break;
                case SyncListOperation.RemoveAt:
                    OnUserDisconnected?.Invoke(oldItem);
                    break;
                case SyncListOperation.Complete:
                    OnComplete?.Invoke(ActiveConnections);
                    break;
                case SyncListOperation.Insert:
                case SyncListOperation.Set:
                case SyncListOperation.Clear:
                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, null);
            }
        }
    }
}