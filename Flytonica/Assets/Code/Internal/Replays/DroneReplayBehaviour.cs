using System;
using Code.Internal.Drone;
using Code.Internal.UserInterface;
using UltimateReplay;
using UnityEngine;

namespace Code.Internal.Replays
{
    public class DroneReplayBehaviour : ReplayRecordableBehaviour
    {
        [SerializeField] private DroneSensors droneSensors;
        [SerializeField] private DroneCamera droneCamera;

        // Переменные для хранения текущих значений
        private float _cameraSignal;
        private float _inputSignal;
        private float _health;
        private float _altitude;
        private float _speed;
        private float _batteryLevel;
        private float _batteryVoltage;
        private float _pitch;
        private float _roll;
        private string _modeName = string.Empty;
        private float _cameraAngle;

        // Переменные для интерполяции (предыдущие и следующие значения)
        private float _cameraSignalPrev, _cameraSignalNext;
        private float _inputSignalPrev, _inputSignalNext;
        private float _healthPrev, _healthNext;
        private float _altitudePrev, _altitudeNext;
        private float _speedPrev, _speedNext;
        private float _batteryLevelPrev, _batteryLevelNext;
        private float _batteryVoltagePrev, _batteryVoltageNext;
        private float _pitchPrev, _pitchNext;
        private float _rollPrev, _rollNext;
        private float _cameraAnglePrev, _cameraAngleNext;

        private void OnValidate()
        {
            droneSensors = GetComponent<DroneSensors>();
            droneCamera = GetComponent<DroneCamera>();
        }

        public override void OnReplaySerialize(ReplayState state)
        {
            // Получаем текущие значения из DroneSensors и DroneCamera
            _cameraSignal = droneSensors.CameraSignal;
            _inputSignal = droneSensors.InputSignal;
            _health = droneSensors.Health;
            _altitude = droneSensors.Altitude;
            _speed = droneSensors.Speed;
            _batteryLevel = droneSensors.BatteryLevel;
            _batteryVoltage = droneSensors.BatteryVoltage;
            _pitch = droneSensors.Pitch;
            _roll = droneSensors.Roll;
            _modeName = droneSensors.ModeName;
            _cameraAngle = droneCamera.CurrentAngle;

            // Записываем значения в состояние
            state.Write(_cameraSignal);
            state.Write(_inputSignal);
            state.Write(_health);
            state.Write(_altitude);
            state.Write(_speed);
            state.Write(_batteryLevel);
            state.Write(_batteryVoltage);
            state.Write(_pitch);
            state.Write(_roll);
            state.Write(_modeName);
            state.Write(_cameraAngle);
        }

        public override void OnReplayDeserialize(ReplayState state)
        {
            // Сдвигаем предыдущие значения и читаем новые значения для интерполяции
            _cameraSignalPrev = _cameraSignalNext;
            _cameraSignalNext = state.ReadSingle();

            _inputSignalPrev = _inputSignalNext;
            _inputSignalNext = state.ReadSingle();

            _healthPrev = _healthNext;
            _healthNext = state.ReadSingle();

            _altitudePrev = _altitudeNext;
            _altitudeNext = state.ReadSingle();

            _speedPrev = _speedNext;
            _speedNext = state.ReadSingle();

            _batteryLevelPrev = _batteryLevelNext;
            _batteryLevelNext = state.ReadSingle();

            _batteryVoltagePrev = _batteryVoltageNext;
            _batteryVoltageNext = state.ReadSingle();

            _pitchPrev = _pitchNext;
            _pitchNext = state.ReadSingle();

            _rollPrev = _rollNext;
            _rollNext = state.ReadSingle();

            _modeName = state.ReadString();

            _cameraAnglePrev = _cameraAngleNext;
            _cameraAngleNext = state.ReadSingle();
        }

        protected override void OnReplayReset()
        {
            // Сбрасываем предыдущие и следующие значения, чтобы избежать артефактов при перемотке
            _cameraSignalPrev = _cameraSignalNext;
            _inputSignalPrev = _inputSignalNext;
            _healthPrev = _healthNext;
            _altitudePrev = _altitudeNext;
            _speedPrev = _speedNext;
            _batteryLevelPrev = _batteryLevelNext;
            _batteryVoltagePrev = _batteryVoltageNext;
            _pitchPrev = _pitchNext;
            _rollPrev = _rollNext;
            _cameraAnglePrev = _cameraAngleNext;
        }

        protected override void OnReplayUpdate(float t)
        {
            if (!IsReplaying) return;

            // Выполняем интерполяцию между предыдущими и следующими значениями
            _cameraSignal = Mathf.Lerp(_cameraSignalPrev, _cameraSignalNext, t);
            _inputSignal = Mathf.Lerp(_inputSignalPrev, _inputSignalNext, t);
            _health = Mathf.Lerp(_healthPrev, _healthNext, t);
            _altitude = Mathf.Lerp(_altitudePrev, _altitudeNext, t);
            _speed = Mathf.Lerp(_speedPrev, _speedNext, t);
            _batteryLevel = Mathf.Lerp(_batteryLevelPrev, _batteryLevelNext, t);
            _batteryVoltage = Mathf.Lerp(_batteryVoltagePrev, _batteryVoltageNext, t);
            _pitch = Mathf.Lerp(_pitchPrev, _pitchNext, t);
            _roll = _rollNext;
            _cameraAngle = Mathf.Lerp(_cameraAnglePrev, _cameraAngleNext, t);

            // Обновляем HUD и камеру
            SetHud();
            droneCamera.SetCameraAngle(_cameraAngle);
        }

        private void SetHud()
        {
            if (DroneHUD.Instance == null || !DroneHUD.Instance.IsShowing()) return;

            // Обновляем элементы HUD с использованием интерполированных значений
            DroneHUD.Instance.AltValueElement.Set(_altitude);
            DroneHUD.Instance.SpeedValueElement.Set(_speed);

            DroneHUD.Instance.BatteryElement.SetVoltage(_batteryVoltage);
            DroneHUD.Instance.BatteryElement.SetСharge(_batteryLevel);
            DroneHUD.Instance.HorizonElement.SetPitch(_pitch);
            DroneHUD.Instance.HorizonElement.SetRoll(_roll);

            DroneHUD.Instance.CameraSignalElement.SetSignal(_cameraSignal);
            DroneHUD.Instance.InputSignalElement.SetSignal(_inputSignal);

            DroneHUD.Instance.HealthValueElement.Set(_health);

            if (!string.IsNullOrEmpty(_modeName))
                DroneHUD.Instance.SetMode(_modeName);
        }
    }
}
