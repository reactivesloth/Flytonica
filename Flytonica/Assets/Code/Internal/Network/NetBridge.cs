using Code.Internal.Drone;
using Code.Internal.Network.Teacher;
using Code.Internal.Scenario;
using Code.Internal.Scenario.Race;
using Code.Internal.Scenario.Searching;
using Code.Internal.Scenario.Transport;
using Code.Internal.UserInterface;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

namespace Code.Internal.Network
{
    public class NetBridge : NetworkBehaviour
    {
        [SerializeField] private DroneSensors sensors;

        private ScenarioInitializer _scenarioInitializer;

        public bool IsObservable = false;

        protected override void OnValidate()
        {
            base.OnValidate();
            sensors = GetComponent<DroneSensors>();
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

            //Пока сюда загоняем то что не реплецируется через DroneSensors.UpdateHUD()
            SendDroneVars(sensors.ModeName, sensors.Health, sensors.CameraSignal, sensors.InputSignal,
                sensors.BatteryLevel, sensors.BatteryVoltage, DroneHUD.Instance.AimElement.Progress);

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
            
            UpdatePlayerResult(Owner, scenario.TotalTimeInSeconds, score, scenario.ScenarioCondition == ScenarioCondition.Finished);
        }

        [ServerRpc]
        private void UpdatePlayerResult(NetworkConnection player, float time, int score, bool isFinished)
        {
            PlayerManager.Instance.UpdateResultTime(player, time, score, isFinished);
        }
        
        [ServerRpc]
        private void SendBaseScenarioStateToServer(float timer, string taskText, string windText, int altMaxValue)
        {
            UpdateLocalScenarioBaseState(timer, taskText, windText, altMaxValue);
        }

        [ServerRpc]
        private void SendDroneVars(string sensorsModeName, float sensorsHealth, float sensorsCameraSignal,
            float sensorsInputSignal, float batteryCharge, float batteryVoltage, float aimProgress)
        {
            UpdateDroneVars(sensorsModeName, sensorsHealth, sensorsCameraSignal, sensorsInputSignal, batteryCharge,
                batteryVoltage, aimProgress);
        }

        [ServerRpc]
        private void SendRaceScenarioStateToServer(int[] checkpointStatuses)
        {
            UpdateLocalScenarioRaceState(checkpointStatuses);
        }

        [ServerRpc]
        private void SendTransportScenarioStateToServer()
        {
        }

        [ServerRpc]
        private void SendSearchingScenarioStateToServer()
        {
        }
        
        private void UpdateLocalScenarioBaseState(float timer, string taskText, string windText, int altMaxValue)
        {
            if (!IsObservable) return;

            DroneHUD.Instance.SetTime(ScenarioBase.GetTimeWithMs(timer));
            DroneHUD.Instance.SetTask(taskText);
            DroneHUD.Instance.SetWind(windText);
            DroneHUD.Instance.AltValueElement.MaxValue = altMaxValue;
        }

        private void UpdateDroneVars(string sensorsModeName, float sensorsHealth, float sensorsCameraSignal,
            float sensorsInputSignal, float batteryCharge, float batteryVoltage, float aimProgress)
        {
            if (!IsObservable) return;

            DroneHUD.Instance.SetMode(sensorsModeName);
            DroneHUD.Instance.HealthValueElement.Set(sensorsHealth);
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