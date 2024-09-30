using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Code.Internal.API.Wrappers;
using Code.Internal.Scenario;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface.Pages;
using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.Internal.Network
{
    public class ConnectionController : NetworkBehaviour
    {
        [SerializeField] private GameObject hostControl;

        [SerializeField] private SceneLoadingSettings sceneSettings;
        [SerializeField] private AvailableScenariosSettings scenarios;
        [SerializeField] private AvailableMapsSettings maps;
        [SerializeField] private AvailableDronesSettings drones;

        private bool _sceneLoaded;
        private readonly List<NetworkConnection> _pendingConnections = new();

        public override void OnStartServer()
        {
            base.OnStartServer();
            ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
            print(sceneSettings.currentScenario.currentMap);
            GameSceneManager.Instance.LoadGlobalScene(sceneSettings.currentScenario.currentMap, OnSceneLoaded);
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
            _sceneLoaded = false;
            hostControl.SetActive(false);
        }

        private void OnRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs args)
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
        }

        private void OnConnectedPlayer(NetworkConnection connection)
        {
            InvokeTargetInitializeScenario(connection);
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
                OnConnectedPlayer(connection);
            }

            _pendingConnections.Clear();
        }

        private async void InvokeTargetInitializeScenario(NetworkConnection connection)
        {
            await Task.Delay(1000);
            var scenario = sceneSettings.currentScenario;
            if (connection.ClientId == 0 && sceneSettings.isNet)
            {
                hostControl.SetActive(true);
            }
            else
            {
                var currentScenario = new ScenarioSettingsData(scenario.name, scenario.description,
                    drones.drones.IndexOf(scenario.currentDrone), maps.maps.IndexOf(scenario.currentMap),
                    scenario.scenarioType, scenario.currentDrone.flightModes.IndexOf(scenario.currentDroneMode),
                    scenario.cameraThirdPerson, scenario.cameraSwitchAllowed, objects: scenario.objects);

                if (sceneSettings.isNet)
                    TargetInitializeScenario(connection, JsonUtility.ToJson(currentScenario));

                var drone = NetworkManager.GetComponent<PlayersSpawner>()
                    .Spawn(connection, sceneSettings.currentScenario.currentDrone);
            }

            MovePlayer(connection);
        }

        [TargetRpc]
        private void TargetInitializeScenario(NetworkConnection connection, string scenarioSettingsJson)
        {
            var scenarioInfo = JsonUtility.FromJson<ScenarioSettingsData>(scenarioSettingsJson);
            var scenario = ScenarioSettings.CreateDynamicTaskScenario(0, scenarioInfo.name,
                scenarioInfo.description, scenarioInfo.typeId, maps.maps[scenarioInfo.mapId],
                drones.drones[scenarioInfo.droneId],
                drones.drones[scenarioInfo.droneId].flightModes[scenarioInfo.droneModeId],
                scenarioInfo.cameraThirdPerson, scenarioInfo.cameraAllowedSwitchModeId, scenarioInfo.objects);
            
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

        [TargetRpc]
        private void MovePlayer(NetworkConnection connection)
        {
            var spawners = GameObject.FindGameObjectsWithTag("Player Respawn")
                .Select(o => o.transform).ToArray();

            if (spawners.Length == 0)
                return;

            var player = GameObject.FindWithTag("Player");
            var spawn = spawners[Random.Range(0, spawners.Length)];
            player.transform.position = spawn.position;
        }
    }
}