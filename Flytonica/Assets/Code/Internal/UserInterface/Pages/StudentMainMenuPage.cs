using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Code.Internal.SceneManagement;
using FishNet;
using FishNet.Discovery;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class StudentMainMenuPage: Page
    {
        [SerializeField] private Button tasksButton,
            singleScriptsButton,
            toRoomButton,
            deviceInfoButton,
            selectAvatarButton;
        [SerializeField] private ScriptsPage scriptsPage;
        [SerializeField] private AvailableScenariosSettings singleScenariosSettings;
        [SerializeField] private AvailableScenariosSettings taskScenariosSettings;

        private List<IPEndPoint> _points = new();
        private IPEndPoint _currentIPEndPoint => _points.LastOrDefault();
        private NetworkDiscovery _discovery => InstanceFinder.NetworkManager.GetComponent<NetworkDiscovery>();

        private void OnDisable()
        {
            _discovery.StopSearchingOrAdvertising();
            _discovery.ServerFoundCallback -= NetworkDiscoveryOnServerFoundCallback;
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            toRoomButton.interactable = _currentIPEndPoint != null;
            _discovery.ServerFoundCallback += NetworkDiscoveryOnServerFoundCallback;
            _discovery.SearchForServers();
            
            singleScriptsButton.onClick.AddListener(OnSingleScripts);
            tasksButton.onClick.AddListener(OnTaskScripts);
            deviceInfoButton.onClick.AddListener(OnDeviceInfo);
            toRoomButton.onClick.AddListener(OnConnect);
        }

        protected override void OnClose()
        {
            base.OnClose();
            singleScriptsButton.onClick.RemoveListener(OnSingleScripts);
            tasksButton.onClick.RemoveListener(OnTaskScripts);
            deviceInfoButton.onClick.RemoveListener(OnDeviceInfo);
            toRoomButton.onClick.RemoveListener(OnConnect);
        }

        private void OnSingleScripts()
        {
            scriptsPage.Init(singleScenariosSettings.scenarios);
            scriptsPage.Open();
        }

        private void OnTaskScripts()
        {
            scriptsPage.Init(taskScenariosSettings.scenarios);
            scriptsPage.Open();
        }

        private void OnDeviceInfo()
        {
            scriptsPage.Init(taskScenariosSettings.scenarios, true);
            scriptsPage.Open();
        }

        private void OnConnect()
        {
            InstanceFinder.ClientManager.StartConnection(_currentIPEndPoint.Address.ToString());
        }
        
        private void NetworkDiscoveryOnServerFoundCallback(IPEndPoint obj)
        {
            print(obj.Address);
            if(!_points.Contains(obj))  
                _points.Add(obj);
            toRoomButton.interactable = _currentIPEndPoint != null;
        }
    }
}