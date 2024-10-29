using System.Collections.Generic;
using Code.Internal.Network.Teacher;
using Code.Internal.UserInterface.Elements;
using Code.Internal.XR;
using FishNet.Connection;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class HostUIControllerPage : MonoBehaviour
    {
        [SerializeField] private Button updateButton;
        [SerializeField] private Button thirdViewButton;
        [SerializeField] private Button leaderboardButton;
        
        [SerializeField] private Transform playerListContainer;
        [SerializeField] private Button playerListItemPrefab;

        private Dictionary<NetworkConnection, NetworkObject> playerDrones;

        private void Start()
        {
            UpdatePlayerList();
            updateButton.onClick.AddListener(UpdatePlayerList);
            thirdViewButton?.onClick.AddListener(HostCameraController.Instance.SetTeacherView);
            leaderboardButton.onClick.AddListener(OnLeaderBoardOpen);
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
            foreach (Transform child in playerListContainer)
            {
                Destroy(child.gameObject);
            }

            playerDrones = PlayerManager.Instance.AllPlayers;

            foreach (var player in playerDrones)
            {
                var listItem = Instantiate(playerListItemPrefab, playerListContainer);
                listItem.GetComponentInChildren<TMP_Text>().text = $"Player {player.Key.ClientId}";
                var connection = player.Key;
                listItem.onClick.AddListener(() => { SelectPlayer(connection); });
            }
        }

        private void SelectPlayer(NetworkConnection connection)
        {
            var drone = playerDrones[connection];
            HostCameraController.Instance.SetTargetDrone(drone);
        }

        private void OnLeaderBoardOpen()
        {
            Leaderboard.Instance.Open();
        }
    }
}