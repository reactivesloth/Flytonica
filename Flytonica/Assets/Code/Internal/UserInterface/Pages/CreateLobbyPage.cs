using FishNet;
using FishNet.Discovery;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class CreateLobbyPage : Page
    {
        [Header("UI Elements")]
        [SerializeField] private Button createLobbyButton;
        [SerializeField] private LobbyPage lobbyPage;

        private NetworkDiscovery _networkDiscovery;

        private void OnEnable()
        {
            createLobbyButton.onClick.AddListener(CreateLobby);
            _networkDiscovery ??= InstanceFinder.NetworkManager.GetComponent<NetworkDiscovery>();
        }

        private void OnDisable()
        {
            createLobbyButton.onClick.RemoveListener(CreateLobby);
        }

        private void CreateLobby()
        {
            InstanceFinder.ServerManager.StartConnection();
            InstanceFinder.ClientManager.StartConnection();
            _networkDiscovery.AdvertiseServer();
            lobbyPage.Open();
            Close();
        }
    }
}