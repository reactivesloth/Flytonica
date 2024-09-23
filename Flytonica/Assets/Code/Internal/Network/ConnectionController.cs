using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Code.Internal.API;
using Code.Internal.API.Wrappers;
using Code.Internal.API.Wrappers.ReceiveModels;
using Code.Internal.Scenario;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

namespace Code.Internal.Network
{
    public class ConnectionController : NetworkBehaviour
    {
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
            TargetInitializeScenario(connection,
                JsonUtility.ToJson(new ScenarioSettingsData(scenario.name, scenario.description,
                    drones.drones.IndexOf(scenario.currentDrone), maps.maps.IndexOf(scenario.currentMap),
                    scenario.scenarioType, scenario.currentDrone.flightModes.IndexOf(scenario.currentDroneMode))));

            var drone = NetworkManager.GetComponent<PlayersSpawner>()
                .Spawn(connection, sceneSettings.currentScenario.currentDrone);
        }

        [TargetRpc]
        private void TargetInitializeScenario(NetworkConnection connection, string scenarioSettingsJson)
        {
            print(sceneSettings);
            var scenarioInfo = JsonUtility.FromJson<ScenarioSettingsData>(scenarioSettingsJson);
            var scenario = ScenarioSettings.CreateDynamicTaskScenario(0, scenarioInfo.name,
                scenarioInfo.description, scenarioInfo.typeId, maps.maps[scenarioInfo.mapId],
                drones.drones[scenarioInfo.droneId],
                drones.drones[scenarioInfo.droneId].flightModes[scenarioInfo.droneModeId]);
            InitScenario(scenario);
            /*print("StartInit");
            var scenario = scenarios.Find(scenarioId);

            if (scenario)
            {
                InitScenario(scenario);
            }
            else
            {
                HttpClient.Get(LinkConstants.MapConfigGetUrl(scenarioId), response =>
                {
                    var responseScenario = JsonUtility.FromJson<ScenarioData>(response);
                    HttpClient.Get(LinkConstants.GetFile(responseScenario.file_file_path), responseFile =>
                    {
                        var scenarioInfo = JsonUtility.FromJson<ScenarioSettingsData>(responseFile);
                        scenario = ScenarioSettings.CreateDynamicTaskScenario(responseScenario.id, scenarioInfo.name,
                            scenarioInfo.description, scenarioInfo.typeId, maps.maps[scenarioInfo.mapId],
                            drones.drones[scenarioInfo.droneId],
                            drones.drones[scenarioInfo.droneId].flightModes[scenarioInfo.droneModeId]);
                        InitScenario(scenario);
                    });
                }, error => { Debug.LogError(error); });
            }*/
        }

        private void InitScenario(ScenarioSettings scenario)
        {
            print($"Init");
            var scenarioInitializer = FindAnyObjectByType<ScenarioInitializer>();
            print($"Init {scenario.name}");
            scenarioInitializer.Initialize(scenario);
            scenario.currentDrone.currentFlightMode = scenario.currentDroneMode;
        }
    }
}