using System;
using System.Collections.Generic;
using Code.Internal.Network;
using Code.Internal.SceneManagement;
using FishNet;
using FishNet.Connection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface
{
    public class LobbyPage : Page
    {
        [SerializeField] private Button startButton;
        [SerializeField] private GameObject playerItemPrefab;
        [SerializeField] private Transform playerListParent;
        [SerializeField] private Lobby lobby;
        
        private readonly Dictionary<NetworkConnection, GameObject> _playerItems = new ();
        
        public override void Open()
        {
            base.Open();

            lobby.OnComplete += OnUsersLoad;
            startButton.onClick.AddListener(StartGame);
        }

        public override void Close()
        {
            base.Close();
            startButton.onClick.RemoveListener(StartGame);
        }

        protected override void OnBackClick()
        {
            InstanceFinder.ClientManager.StopConnection();
            InstanceFinder.ServerManager.StopConnection(true);
            base.OnBackClick();
        }

        private void OnUserConnected(NetworkConnection conn)
        {
            var item = Instantiate(playerItemPrefab, playerListParent);
            item.GetComponent<TMP_Text>().text = $"Player {conn.ClientId}";
            _playerItems.TryAdd(conn, item);
        }

        private void OnUsersLoad(List<NetworkConnection> connections)
        {
            ClearConnections();
            connections.ForEach(OnUserConnected);
        }

        private void ClearConnections()
        {
            foreach (var playerItemsValue in _playerItems.Values)
                Destroy(playerItemsValue.gameObject);
            
            _playerItems.Clear();
        }

        private void StartGame()
        {
            GameSceneManager.Instance.LoadGame();
        }
    }
}
