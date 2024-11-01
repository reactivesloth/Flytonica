using System.Collections.Generic;
using System.Linq;
using Code.Internal.Drone;
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
        [SerializeField] private HostPlayerButton playerListItemPrefab;

        private Dictionary<NetworkConnection, PlayerData> playersWithDrones;
        private Dictionary<NetworkConnection, HostPlayerButton> playersButtons;

        private void Start()
        {
            UpdatePlayerList();
            updateButton.onClick.AddListener(UpdatePlayerList);
            thirdViewButton?.onClick.AddListener(HostCameraController.Instance.SetTeacherView);
            leaderboardButton.onClick.AddListener(OnLeaderBoardOpen);
        }

        private void OnEnable()
        {
            UsersManager.Instance.OnPlayerListUpdated += UpdatePlayerList;
            UsersManager.Instance.OnHelpSignal += Help;
        }

        private void OnDisable()
        {
            if (UsersManager.Instance != null)
            {
                UsersManager.Instance.OnPlayerListUpdated -= UpdatePlayerList;
                UsersManager.Instance.OnHelpSignal -= Help;
            }
        }

        public void UpdatePlayerList()
        {
            playersButtons = new Dictionary<NetworkConnection, HostPlayerButton>();
            foreach (var button in playersButtons.Values)
            {
                Destroy(button.gameObject);
            }

            playersWithDrones = UsersManager.Instance.AllPlayers.Where(d => d.Value.Drone != null)
                .ToDictionary(d => d.Key, d => d.Value);

            foreach (var player in playersWithDrones)
            {
                var listItem = Instantiate(playerListItemPrefab, playerListContainer);
                listItem.Init(player.Value.Drone, player.Value.PlayerName);
                playersButtons.Add(player.Key, listItem);
            }
        }

        private void SelectPlayer(NetworkConnection connection)
        {
            var drone = playersWithDrones[connection].Drone;
            HostCameraController.Instance.SetTargetDrone(drone);
        }

        private void OnLeaderBoardOpen()
        {
            if (!Leaderboard.Instance.gameObject.activeSelf)
                Leaderboard.Instance.Open();
            else
                Leaderboard.Instance.Close();
        }

        private void Help(NetworkConnection connection)
        {
            if (!playersButtons.TryGetValue(connection, out var button))
                return;

            button.Help();
        }
    }
}