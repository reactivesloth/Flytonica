using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Code.Internal.API;
using Code.Internal.API.Wrappers;
using Code.Internal.API.Wrappers.ReceiveModels;
using Code.Internal.SceneManagement;
using FishNet;
using FishNet.Discovery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Pages
{
    public class StudentMainMenuPage : Page
    {
        [SerializeField] private TMP_Text studentNameText;

        [SerializeField] private Button tasksButton,
            singleScriptsButton,
            toRoomButton;

        [SerializeField] private ScriptsPage scriptsPage;
        [SerializeField] private AvailableScenariosSettings singleScenariosSettings;
        [SerializeField] private AvailableScenariosSettings taskScenariosSettings;
        [SerializeField] private AvailableMapsSettings maps;
        [SerializeField] private AvailableDronesSettings drones;

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
            toRoomButton.onClick.AddListener(OnConnect);

            if (HttpClient.IsAuthorized)
            {
                RequestAndSetUserData();
            }
            else
                SetDemo();
        }

        protected override void OnClose()
        {
            base.OnClose();
            singleScriptsButton.onClick.RemoveListener(OnSingleScripts);
            tasksButton.onClick.RemoveListener(OnTaskScripts);
            toRoomButton.onClick.RemoveListener(OnConnect);
        }

        // Executes the logout function
        protected override void OnBackClick()
        {
            HttpClient.Logout();
            base.OnBackClick();
        }

        private void OnSingleScripts()
        {
            scriptsPage.Init(singleScenariosSettings.scenarios);
            scriptsPage.Open();
        }

        private void OnTaskScripts()
        {
            scriptsPage.Open();
            scriptsPage.InitTasks();
        }

        private void OnConnect()
        {
            InstanceFinder.ClientManager.StartConnection(_currentIPEndPoint.Address.ToString());
        }

        private void NetworkDiscoveryOnServerFoundCallback(IPEndPoint obj)
        {
            if (!_points.Contains(obj))
                _points.Add(obj);
            toRoomButton.interactable = _currentIPEndPoint != null;
        }

        private void RequestAndSetUserData()
        {
            tasksButton.interactable = true;
            
            if (HttpClient.UserData == null)
                HttpClient.Get(LinkConstants.UserInfoUrl, data =>
                    {
                        HttpClient.SetUserData(JsonUtility.FromJson<UserData>(data));
                        SetData();
                    },
                    Debug.LogError);
            else
                SetData();
        }

        private void SetData()
        {
            var data = HttpClient.UserData;
            print(data.name);
            studentNameText.text = data.name;
        }

        private void SetDemo()
        {
            tasksButton.interactable = false;
            HttpClient.SetUserData(new UserData { name = "Гость", type = UserType.Guest});
            SetData();
        }
    }
}