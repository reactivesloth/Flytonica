using Code.Internal.API;
using Code.Internal.API.Wrappers;
using Code.Internal.Drone;
using Code.Internal.Network.Teacher;
using Code.Internal.Scenario;
using Code.Internal.Scenario.Race;
using Code.Internal.Scenario.Searching;
using Code.Internal.Scenario.Transport;
using Code.Internal.UserInterface;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Code.Internal.Network
{
    public class NetBridge : NetworkBehaviour
    {
        [SerializeField] private DroneSensors sensors;

        private ScenarioInitializer _scenarioInitializer;

        public bool IsObservable = false;

        public readonly SyncVar<string> PlayerNickNameSync = new();

        public bool IsTeacher => HttpClient.IsAuthorized && HttpClient.UserData.type == UserType.Teacher;

        protected override void OnValidate()
        {
            base.OnValidate();
            sensors = GetComponent<DroneSensors>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!IsOwner)
                return;

            var nickName = HttpClient.IsAuthorized ? HttpClient.UserData.name : $"Player {OwnerId}";
            SetPlayerDataInServer(Owner, nickName);
        }

        public override void OnStopClient()
        {
            RemovePlayerForServer(Owner);
            
            base.OnStopClient();
        }

        private void Update()
        {
            if (!_scenarioInitializer)
            {
                _scenarioInitializer = FindAnyObjectByType<ScenarioInitializer>();
                return;
            }

            if (!IsOwner)
                return;

            var scenario = _scenarioInitializer?.CurrentScenario;

            if (!scenario)
                return;

            int score = 0;
            SendBaseScenarioStateToServer(scenario.TotalTimeInSeconds, DroneHUD.Instance.CurrentTaskText,
                DroneHUD.Instance.CurrentWindText, DroneHUD.Instance.AltValueElement.MaxValue);
            
            SendDroneVars(sensors.ModeName, sensors.Health, sensors.CameraSignal, sensors.InputSignal,
                sensors.BatteryLevel, sensors.BatteryVoltage, DroneHUD.Instance.AimElement.Progress,
                sensors.Speed);

            switch (scenario)
            {
                case ScenarioRace scenarioRace:
                    SendRaceScenarioStateToServer(scenarioRace.GetCheckpointStatuses());
                    score = scenarioRace.PassedCheckpoints;
                    break;
                case ScenarioTransport scenarioTransport:
                    score = scenarioTransport.DeliveredCargoCount;
                    break;
                case ScenarioSearching scenarioSearching:
                    score = scenarioSearching.FindedCount;
                    break;
            }

            UpdatePlayerResultInServer(Owner, scenario.TotalTimeInSeconds, score,
                scenario.ScenarioCondition == ScenarioCondition.Finished);
        }

        [ServerRpc]
        private void UpdatePlayerResultInServer(NetworkConnection player, float time, int score, bool isFinished)
        {
            UpdatePlayerResultForTeacher(player,time, score, isFinished);
        }

        [ServerRpc]
        private void SendBaseScenarioStateToServer(float timer, string taskText, string windText, int altMaxValue)
        {
            SendBaseScenarioStateForTeacher(timer, taskText, windText, altMaxValue);
        }

        [ServerRpc]
        private void SendDroneVars(string sensorsModeName, float sensorsHealth, float sensorsCameraSignal,
            float sensorsInputSignal, float batteryCharge, float batteryVoltage, float aimProgress, float speed)
        {
            SendDroneVarsForTeacher(sensorsModeName, sensorsHealth, sensorsCameraSignal, sensorsInputSignal, batteryCharge,
                batteryVoltage, aimProgress, speed);
        }

        [ServerRpc]
        private void SendRaceScenarioStateToServer(int[] checkpointStatuses)
        {
            SendRaceScenarioStateForTeacher(checkpointStatuses);
        }

        /*[ServerRpc]
        private void SendTransportScenarioStateToServer()
        {
        }

        [ServerRpc]
        private void SendSearchingScenarioStateToServer()
        {
        }*/

        [ServerRpc]
        private void SetPlayerDataInServer(NetworkConnection sender, string value)
        {
            SetPlayerDataForTeacher(sender, value);
        }
        
        [ObserversRpc]
        private void RemovePlayerForServer(NetworkConnection sender)
        {
            RemovePlayerForTeacher(sender);
        }

        //========================================================================================
        [ObserversRpc]
        private void UpdatePlayerResultForTeacher(NetworkConnection player, float time,
            int score, bool isFinished)
        {
            if(!IsTeacher) return;
            UsersManager.Instance.UpdateResult(player, time, score, isFinished);
        }

        [ObserversRpc]
        private void SendBaseScenarioStateForTeacher(float timer, string taskText,
            string windText, int altMaxValue)
        {
            if(!IsTeacher) return;
            UpdateLocalScenarioBaseState(timer, taskText, windText, altMaxValue);
        }

        [ObserversRpc]
        private void SendDroneVarsForTeacher(string sensorsModeName, float sensorsHealth, float sensorsCameraSignal,
            float sensorsInputSignal, float batteryCharge, float batteryVoltage, float aimProgress, float speed )
        {
            if(!IsTeacher) return;
            UpdateDroneVars(sensorsModeName, sensorsHealth, sensorsCameraSignal, sensorsInputSignal, batteryCharge,
                batteryVoltage, aimProgress, speed);
        }

        [ObserversRpc]
        private void SendRaceScenarioStateForTeacher(int[] checkpointStatuses)
        {
            if(!IsTeacher) return;
            UpdateLocalScenarioRaceState(checkpointStatuses);
        }

        /*[ObserversRpc]
        private void SendTransportScenarioStateForTeacher()
        {
            if(!IsTeacher) return;
        }

        [ObserversRpc]
        private void SendSearchingScenarioStateForTeacher()
        {
            if(!IsTeacher) return;
        }*/

        [ObserversRpc]
        private void SetPlayerDataForTeacher(NetworkConnection sender, string value)
        {
            if (!IsTeacher) return;
            PlayerNickNameSync.Value = value;
            UsersManager.Instance.AddPlayer(sender, PlayerNickNameSync.Value, NetworkObject);
        }
        
        [ObserversRpc]
        private void RemovePlayerForTeacher(NetworkConnection sender)
        {
            if (!IsTeacher) return;
            UsersManager.Instance.RemovePlayer(sender);
        }

        //================================================================================================

        private void UpdateLocalScenarioBaseState(float timer, string taskText, string windText, int altMaxValue)
        {
            if (!IsObservable) return;

            DroneHUD.Instance.SetTime(ScenarioBase.GetTimeWithMs(timer));
            DroneHUD.Instance.SetTask(taskText);
            DroneHUD.Instance.SetWind(windText);
            DroneHUD.Instance.AltValueElement.MaxValue = altMaxValue;
        }

        private void UpdateDroneVars(string sensorsModeName, float sensorsHealth, float sensorsCameraSignal,
            float sensorsInputSignal, float batteryCharge, float batteryVoltage, float aimProgress, float speed)
        {
            if (!IsObservable) return;

            DroneHUD.Instance.SetMode(sensorsModeName);
            DroneHUD.Instance.HealthValueElement.Set(sensorsHealth);
            DroneHUD.Instance.SpeedValueElement.Set(speed);
            DroneHUD.Instance.CameraSignalElement.SetSignal(sensorsCameraSignal);
            DroneHUD.Instance.InputSignalElement.SetSignal(sensorsInputSignal);

            DroneHUD.Instance.BatteryElement.SetСharge(batteryCharge);
            DroneHUD.Instance.BatteryElement.SetVoltage(batteryVoltage);

            DroneHUD.Instance.AimElement.SetProgressValue(aimProgress);
            
        }

        private void UpdateLocalScenarioRaceState(int[] checkpointStatuses)
        {
            if (!IsObservable) return;

            var scenarioRace = _scenarioInitializer.CurrentScenario as ScenarioRace;
            if (scenarioRace)
                scenarioRace.SetScenarioState(checkpointStatuses);
        }
    }
}