using System.Collections.Generic;
using Code.Internal.Network.Teacher;
using FishNet.Connection;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class HostUIControllerPage: Page
    {
        [SerializeField] private Transform playerListContainer;
        [SerializeField] private Button playerListItemPrefab;

        private Dictionary<NetworkConnection, NetworkObject> playerDrones;

        private void Start()
        {
            UpdatePlayerList();
        }

        private void OnEnable()
        {
            PlayerManager.Instance.OnPlayerListUpdated += UpdatePlayerList;
        }

        private void OnDisable()
        {
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.OnPlayerListUpdated -= UpdatePlayerList;
            }
        }
        
        public void UpdatePlayerList()
        {
            // Очищаем список
            foreach (Transform child in playerListContainer)
            {
                Destroy(child.gameObject);
            }

            playerDrones = PlayerManager.Instance.GetAllPlayers();

            foreach (var player in playerDrones)
            {
                var listItem = Instantiate(playerListItemPrefab, playerListContainer);
                listItem.GetComponentInChildren<TMP_Text>().text = $"Player {player.Key.ClientId}";

                var button = listItem.GetComponentInChildren<Button>();
                var connection = player.Key;

                button.onClick.AddListener(() =>
                {
                    SelectPlayer(connection);
                });
            }
        }

        private void SelectPlayer(NetworkConnection connection)
        {
            var drone = playerDrones[connection];
            HostCameraController.Instance.SetTargetDrone(drone);
        }
    }
}