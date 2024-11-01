using System;
using System.Threading.Tasks;
using Code.Internal.API;
using Code.Internal.API.Wrappers;
using Code.Internal.Scenario;
using Code.Internal.SceneManagement;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Internal.Network
{
    public class ResetController: NetworkBehaviour
    {
        public static ResetController Instance { get; private set; }
        
        /*[SerializeField] private int reconnectTimeOutInMs = 10000;
        [SerializeField] private int reconnectTickInMs = 500;
        
        public string _currentServerIp;
        public ushort _currentServerPort;
        */

        private void Awake()
        {
            Instance = this;
        }

        /*public override void OnStartClient()
        {
            base.OnStartClient();
            _currentServerIp = NetworkManager.TransportManager.Transport.GetClientAddress();
            _currentServerPort = NetworkManager.TransportManager.Transport.GetPort();
        }*/

        public void RestartServerRequest()
        {
            if(HttpClient.IsAuthorized && HttpClient.UserData.type == UserType.Teacher)
                ResetServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void ResetServerRpc()
        {
            ReconnectBehaviour();
            ResetServer();
        }

        public async void ResetServer()
        {
            await Task.Delay(1000);
            ScenarioSwitcherController.Instance.EndSession();
            while (ServerManager.Started)
                await Task.Delay(100);
            ServerManager.StartConnection();
            ClientManager.StartConnection();
        }

        [ObserversRpc(ExcludeServer = true)]
        public void ReconnectBehaviour()
        {
            ScenarioSwitcherController.Instance.EndSession();
        }
    }
}