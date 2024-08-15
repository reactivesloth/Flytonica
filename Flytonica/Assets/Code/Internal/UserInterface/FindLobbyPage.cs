using System;
using System.Collections.Generic;
using System.Net;
using FishNet;
using FishNet.Discovery;
using FishNet.Transporting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface
{
    public class FindLobbyPage : Page
    {
        [Header("UI Elements")] 
        
        [SerializeField] private Button refreshButton;
        [SerializeField] private Transform buttonsParent;
        [SerializeField] private Button connectToLobbyButtonPrefab;
        [SerializeField] private LobbyPage lobbyPage;
        
        private NetworkDiscovery _networkDiscovery;

        private readonly List<Button> _currentServersButton = new();

        public override void Open()
        {
            base.Open();
            _networkDiscovery ??= InstanceFinder.NetworkManager.GetComponent<NetworkDiscovery>();
            refreshButton.onClick.AddListener(OnRefreshServersList);
            _networkDiscovery.SearchForServers();
            _networkDiscovery.ServerFoundCallback += OnServerFound;
        }

        public override void Close()
        {
            base.Close();
            refreshButton.onClick.RemoveListener(OnRefreshServersList);
            _networkDiscovery.StopSearchingOrAdvertising();
            _networkDiscovery.ServerFoundCallback -= OnServerFound;
        }

        private void OnRefreshServersList()
        {
            _networkDiscovery.StopSearchingOrAdvertising();
            ClearServersList();
            _networkDiscovery.SearchForServers();
        }
        
        private void OnServerFound(IPEndPoint endPoint)
        {
            CreateServerButton(endPoint.Address.ToString());
        }

        private void ClearServersList()
        {
            _currentServersButton.ForEach(b => Destroy(b.gameObject));
            _currentServersButton.Clear();
        }

        private void CreateServerButton(string address)
        {
            var newButton = Instantiate(connectToLobbyButtonPrefab, buttonsParent);
            newButton.GetComponentInChildren<TMP_Text>().text = address;
            newButton.onClick.AddListener(() =>
            {
                InstanceFinder.ClientManager.StartConnection(address);
                InstanceFinder.ClientManager.OnClientConnectionState += args =>
                {
                    if (args.ConnectionState != LocalConnectionState.Started) return;
                    lobbyPage.Open();
                    Close();
                };
            });
            _currentServersButton.Add(newButton);
        }
    }
}