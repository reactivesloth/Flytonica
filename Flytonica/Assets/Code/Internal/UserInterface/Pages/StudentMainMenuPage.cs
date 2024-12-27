using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Code.Internal.API;
using Code.Internal.API.Wrappers;
using Code.Internal.API.Wrappers.ReceiveModels;
using Code.Internal.Avatars.Settings;
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

        [SerializeField] private Image avatarImage;
        [SerializeField] private Button tasksButton,
            singleScriptsButton,
            toRoomButton,
            settingsButton,
            avatarSettingsButton;

        [SerializeField] private Page loginPage;
        [SerializeField] private ScriptsPage scriptsPage;
        [SerializeField] private Page settingsPage;
        [SerializeField] private Page avatarSettingsPage;

        [SerializeField] private AvatarsList avatarsList;
        [SerializeField] private AvailableScenariosSettings singleScenariosSettings;
        [SerializeField] private AvailableScenariosSettings taskScenariosSettings;
        [SerializeField] private AvailableMapsSettings maps;
        [SerializeField] private AvailableDronesSettings drones;

        private List<IPEndPoint> _points = new();
        private IPEndPoint _currentIPEndPoint => _points.LastOrDefault();
        private NetworkDiscovery _discovery => InstanceFinder.NetworkManager.GetComponent<NetworkDiscovery>();

        private Coroutine _buttonStateCoroutine;

        private void OnDisable()
        {
            _discovery.StopSearchingOrAdvertising();
            _discovery.ServerFoundCallback -= OnServerFound;
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            _points.Clear();
            toRoomButton.interactable = false;
            
            _discovery.ServerFoundCallback += OnServerFound;
            _discovery.SearchForServers();

            _buttonStateCoroutine = StartCoroutine(UpdateButtonStateCoroutine());

            int avatarIndex = 0;
            try
            {
                avatarIndex = PlayerPrefs.GetInt("Avatar");
            }
            catch (PlayerPrefsException)
            {
                PlayerPrefs.SetInt("Avatar", 0);
            }

            avatarImage.sprite = avatarsList.avatarsIksList[avatarIndex].icon;

            singleScriptsButton.onClick.AddListener(OnSingleScripts);
            tasksButton.onClick.AddListener(OnTaskScripts);
            toRoomButton.onClick.AddListener(OnConnect);
            settingsButton.onClick.AddListener(OnSettings);
            avatarSettingsButton.onClick.AddListener(OnAvatarSettings);

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

            // Останавливаем корутину обновления кнопки
            if (_buttonStateCoroutine != null)
            {
                StopCoroutine(_buttonStateCoroutine);
                _buttonStateCoroutine = null;
            }

            singleScriptsButton.onClick.RemoveListener(OnSingleScripts);
            tasksButton.onClick.RemoveListener(OnTaskScripts);
            toRoomButton.onClick.RemoveListener(OnConnect);
            settingsButton.onClick.RemoveListener(OnSettings);
            avatarSettingsButton.onClick.RemoveListener(OnAvatarSettings);
        }

        private IEnumerator UpdateButtonStateCoroutine()
        {
            while (true)
            {
                toRoomButton.interactable = _currentIPEndPoint != null;
                yield return new WaitForSeconds(0.1f);
            }
        }

        private void OnServerFound(IPEndPoint endpoint)
        {
            if (!_points.Contains(endpoint))
                _points.Add(endpoint);
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

        private void OnSettings()
        {
            settingsPage.Open();
        }

        private void OnConnect()
        {
            InstanceFinder.ClientManager.StartConnection(_currentIPEndPoint.Address.ToString());
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
                    (s, l) => Debug.LogError(s));
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
            HttpClient.SetUserData(new UserData { name = "Гость", type = UserType.Guest });
            SetData();
        }

        private void OnAvatarSettings()
        {
            avatarSettingsPage.Open();
        }
        
        protected override void OnBackClick()
        {
            HttpClient.Logout();
            loginPage?.Open();
        }
    }
}
