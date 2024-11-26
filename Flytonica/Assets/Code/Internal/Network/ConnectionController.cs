using System.Collections.Generic;
using System.Threading.Tasks;
using Code.Internal.API;
using Code.Internal.API.Wrappers;
using Code.Internal.Avatars;
using Code.Internal.Replays;
using Code.Internal.Scenario;
using Code.Internal.SceneManagement;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

namespace Code.Internal.Network
{
    public class ConnectionController : NetworkBehaviour
    {
        public static ConnectionController Instance { get; private set; }

        [SerializeField] private GameObject locomotion;
        [SerializeField] private List<GameObject> hostControls;
        [SerializeField] private List<GameObject> clientControls;

        [SerializeField] private SceneLoadingSettings sceneSettings;
        [SerializeField] private AvailableScenariosSettings scenarios;
        [SerializeField] private AvailableMapsSettings maps;
        [SerializeField] private AvailableDronesSettings drones;

        private bool _sceneLoaded;
        private readonly Dictionary<NetworkConnection, UserData> _pendingConnections = new();

        private struct UserData
        {
            public UserType UserType;
            public int AvatarId;

            public UserData(UserType userType, int avatarId)
            {
                UserType = userType;
                AvatarId = avatarId;
            }
        }

        private void Awake()
        {
            Instance = this;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            //ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
            print(sceneSettings.currentScenario.currentMap);
            GameSceneManager.Instance.LoadGlobalScene(sceneSettings.currentScenario.currentMap, OnSceneLoaded);
            locomotion.SetActive(true);
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            //ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
            _sceneLoaded = false;
            locomotion.SetActive(false);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            var isTeacher = HttpClient.IsAuthorized && HttpClient.UserData.type == UserType.Teacher;
            clientControls.ForEach(o => o.SetActive(!isTeacher));
            hostControls.ForEach(o => o.SetActive(isTeacher));

            ServerConnectionHandle(ClientManager.Connection, (int)HttpClient.UserData.type,
                PlayerPrefs.GetInt("Avatar"));
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            hostControls.ForEach(o => o.SetActive(false));
            clientControls.ForEach(o => o.SetActive(false));
            ServerDisconnectionHandle(ClientManager.Connection, (int)HttpClient.UserData.type);
            if(!ServerManager.Started)
                OnServerDisconnected();
        }

        /*private void OnRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs args)
        {
            switch (args.ConnectionState)
            {
                case RemoteConnectionState.Started:
                    if (_sceneLoaded)
                        OnConnectedPlayer(connection);
                    else
                        _pendingConnections.Add(connection);
                    break;
                case RemoteConnectionState.Stopped:
                    OnDisconnectedPlayer(connection);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }*/

        [ServerRpc(RequireOwnership = false)]
        public void ServerConnectionHandle(NetworkConnection connection, int userTypeInt, int avatarId)
        {
            var userType = (UserType)userTypeInt;
            print($"Server handle {userType}");
            if (_sceneLoaded)
                OnConnectedPlayer(connection, userType, avatarId);
            else
                _pendingConnections.Add(connection, new UserData(userType, avatarId));
        }

        [ServerRpc(RequireOwnership = false)]
        public void ServerDisconnectionHandle(NetworkConnection connection, int userTypeInt)
        {
            OnDisconnectedPlayer(connection);
        }

        private async void OnConnectedPlayer(NetworkConnection connection, UserType userType, int avatarId)
        {
            print("PlayerConnected");

            while (!Observers.Contains(connection))
                await Task.Delay(100);


            var scenario = sceneSettings.currentScenario;
            var currentScenario = new ScenarioSettingsData(scenario.name, scenario.description,
                drones.drones.IndexOf(scenario.currentDrone), maps.maps.IndexOf(scenario.currentMap),
                scenario.scenarioType, scenario.currentDrone.flightModes.IndexOf(scenario.currentDroneMode),
                scenario.cameraThirdPerson, scenario.cameraSwitchAllowed, windLayers: scenario.windSettings,objects: scenario.objects);

            if (sceneSettings.isNet)
                TargetInitializeScenario(connection, JsonUtility.ToJson(currentScenario));

            var isTeacher = userType == UserType.Teacher;

            if (isTeacher)
            {
            }
            else
            {
                var drone = NetworkManager.GetComponent<PlayersSpawner>()
                    .Spawn(connection, sceneSettings.currentScenario.currentDrone);
                AvatarController.Instance.SpawnAvatar(connection, avatarId);
                print(sceneSettings.isNet);
                if (sceneSettings.isNet)
                    StartRecording(connection);
            }
        }

        private void OnDisconnectedPlayer(NetworkConnection connection)
        {
            _pendingConnections.Remove(connection);
            NetworkManager.GetComponent<PlayersSpawner>().Despawn(connection);
        }

        private void OnSceneLoaded()
        {
            _sceneLoaded = true;
            FindAnyObjectByType<ScenarioInitializer>().Initialize(sceneSettings.currentScenario);

            foreach (var connection in _pendingConnections)
            {
                OnConnectedPlayer(connection.Key, connection.Value.UserType, connection.Value.AvatarId);
            }

            _pendingConnections.Clear();
        }

        [TargetRpc]
        public void TargetInitializeScenario(NetworkConnection connection, string scenarioSettingsJson)
        {
            var scenarioInfo = JsonUtility.FromJson<ScenarioSettingsData>(scenarioSettingsJson);
            var scenario = ScenarioSettings.CreateDynamicTaskScenario(0, scenarioInfo.name,
                scenarioInfo.description, scenarioInfo.typeId, maps.maps[scenarioInfo.mapId],
                drones.drones[scenarioInfo.droneId],
                drones.drones[scenarioInfo.droneId].flightModes[scenarioInfo.droneModeId],
                scenarioInfo.cameraThirdPerson, scenarioInfo.cameraAllowedSwitchModeId, scenarioInfo.objects,
                scenarioInfo.windLayers);

            InitScenario(scenario);
        }

        private void InitScenario(ScenarioSettings scenario)
        {
            sceneSettings.currentScenario = scenario;

            var scenarioInitializer = FindAnyObjectByType<ScenarioInitializer>();
            scenarioInitializer.Initialize(sceneSettings.currentScenario);

            sceneSettings.currentScenario.currentDrone.currentFlightMode =
                sceneSettings.currentScenario.currentDroneMode;
            print(sceneSettings.currentScenario.currentDrone.currentFlightMode.name);
        }

        public void MovePlayerSignal(NetworkConnection conn, Vector3 position, Quaternion rotation) => MovePlayerRpc(conn, position, rotation);

        [TargetRpc]
        public void MovePlayerRpc(NetworkConnection conn, Vector3 position, Quaternion rotation) => MovePlayer(position, rotation);

        public void MovePlayer(Vector3 position, Quaternion rotation)
        {
            var player = GameObject.FindWithTag("Player");
            player.transform.SetPositionAndRotation(position, rotation);
        }

        [TargetRpc]
        public void StartRecording(NetworkConnection connection) => ReplayController.Instance.StartRecording(-1);
        
        public void OnServerDisconnected() => ScenarioSwitcherController.Instance.EndSession();
    }
}