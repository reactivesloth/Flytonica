using System.Collections.Generic;
using System.Linq;
using System.Net;
using Code.Internal.API;
using Code.Internal.SceneManagement;
using FishNet;
using FishNet.Discovery;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace Code.Internal.UserInterface.Pages
{
    public class TeacherMainMenuPage : Page
    {
        [SerializeField]
        private Button playScenarioButton, editScenarioButton, studentsButton, settingsButton;

        [SerializeField] private LoginPage loginPage;
        [SerializeField] private ScriptsPage scriptsPage;
        [SerializeField] private AvailableScenariosSettings taskScenariosSettings;
        [SerializeField] private ScenariosManagmentPage scenariosManagementPage;
        [SerializeField] private GroupsPage groupsPage;
        [SerializeField] private Page calibrationPage;

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
            playScenarioButton.interactable = !XRSettings.enabled || !XRSettings.isDeviceActive || _currentIPEndPoint != null;
            editScenarioButton.interactable = !XRSettings.enabled && !XRSettings.isDeviceActive;
            
            _discovery.ServerFoundCallback += NetworkDiscoveryOnServerFoundCallback;
            _discovery.SearchForServers();

            playScenarioButton.onClick.AddListener(OnPlayScenarioClicked);
            editScenarioButton.onClick.AddListener(OnEditScenarioClicked);
            studentsButton.onClick.AddListener(OnStudentsClicked);
            settingsButton.onClick.AddListener(OnSettingsClicked);

            if (!HttpClient.IsAuthorized)
                SetDemo();
        }

        private void OnPlayScenarioClicked()
        {
            if (XRSettings.enabled && XRSettings.isDeviceActive)
            {
                InstanceFinder.ClientManager.StartConnection(_currentIPEndPoint.Address.ToString());
            }
            else
            {
                scenariosManagementPage.Init(false);
                scenariosManagementPage.Open();
            }
        }

        private void OnEditScenarioClicked()
        {
            scenariosManagementPage.Init(true);
            scenariosManagementPage.Open();
        }
    
        private void OnStudentsClicked()
        {
            groupsPage?.Open();
        }

        private void OnSettingsClicked()
        {
            calibrationPage?.Open();
        }

        protected override void OnClose()
        {
            base.OnClose();
            playScenarioButton.onClick.RemoveListener(OnPlayScenarioClicked);
            editScenarioButton.onClick.RemoveListener(OnEditScenarioClicked);
            studentsButton.onClick.RemoveListener(OnStudentsClicked);
            settingsButton.onClick.RemoveListener(OnSettingsClicked);
        }

        private void NetworkDiscoveryOnServerFoundCallback(IPEndPoint obj)
        {
            if (!_points.Contains(obj))
                _points.Add(obj);
            playScenarioButton.interactable = _currentIPEndPoint != null;
        }

        private void SetDemo()
        {

        }
    }
}