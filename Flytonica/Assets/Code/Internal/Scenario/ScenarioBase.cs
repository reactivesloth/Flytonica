using System;
using System.Globalization;
using Code.Internal.API;
using Code.Internal.Drone;
using Code.Internal.SceneManagement;
using UnityEngine;

namespace Code.Internal.Scenario
{
    public abstract class ScenarioBase : MonoBehaviour
    {
        protected float TotalTime;
        protected ScenarioSettings CurrentScenario;

        protected float FinalScore = 100;
        private bool _isSuccess = false;
        private bool _droneControllerInitialized = false;
        
        //Штрафы
        private float _dischargeCount,
            _collisionWithObjectsCount,
            _signalLossesPliCount,
            _signalLossesRebCount,
            _signalLossesWallsCount,
            _signalLossesFarCount,
            _collisionWithMensCount,
            _collisionWithAnimalsCount,
            _collisionWithBirdsCount;
        
        public float TotalBasePenalties => 
            _dischargeCount * 10 + 
            _collisionWithObjectsCount * 10 + 
            _signalLossesPliCount * 10 + 
            _signalLossesRebCount * 10 + 
            _signalLossesWallsCount * 10 + 
            _signalLossesFarCount * 10 + 
            _collisionWithMensCount * 20 + 
            _collisionWithAnimalsCount * 10 + 
            _collisionWithBirdsCount * 10;
        
        public bool IsStarting { get; protected set; }
        

        protected virtual void Update()
        {
            if (IsStarting)
            {
                TotalTime += Time.deltaTime;
            }
            
            if (!_droneControllerInitialized && DroneController.Instance != null)
            {
                OnDroneControllerInitialized();
                _droneControllerInitialized = true;
            }
        }
        
        public virtual void Initialize(ScenarioSettings scenario)
        {
            CurrentScenario = scenario;
            FinalScore = 100f;
            
            // Обнуляем все штрафы
            _dischargeCount = _collisionWithObjectsCount = _signalLossesPliCount = _signalLossesRebCount = 
                _signalLossesWallsCount = _signalLossesFarCount = _collisionWithMensCount = 
                    _collisionWithAnimalsCount = _collisionWithBirdsCount = 0;
        }

        protected virtual void StartRace()
        {
            // Фиксируем дату и время начала прохождения 
            ReportBuilder.Instance.AddParameter("Время и дата начала сценария",
                DateTime.Now.ToString(CultureInfo.InvariantCulture));
            IsStarting = true;
        }

        protected virtual void FinishRace(bool success = true)
        {
            _isSuccess = success;
            IsStarting = false;
        }

        protected virtual void AddStatistic()
        {
            FinalScore -= TotalBasePenalties;
            if (FinalScore < 0f)
                FinalScore = 0;

            ScenarioSwitcherController.Instance.ScoreSum += FinalScore;
            
            var resultBuilder = ReportBuilder.Instance;
                
            //Фиксируем другие неоцениваемые параметры
            resultBuilder.AddParameter("Название сценария", CurrentScenario.name);
            resultBuilder.AddParameter("Тип сценария", CurrentScenario.scenarioType.GetName());
            resultBuilder.AddParameter("Модель дрона", CurrentScenario.currentDrone?.name);
            resultBuilder.AddParameter("Успешность сценария", _isSuccess ? "Да" : "Нет");
            resultBuilder.AddParameter("Время выполнения задания", $"{(int)(TotalTime / 60):D2}:{(int)(TotalTime % 60):D2}");
            resultBuilder.AddParameter("Количество перезапусков сценария", 0);
            
            //Записываем оценку 
            resultBuilder.AddParameter("Оценка сценария", $"{FinalScore:F0}%");
            
            //Записываем общие штрафы в отчёты
            resultBuilder.AddParameter("Падения из-за разрядки", _dischargeCount.ToString(CultureInfo.InvariantCulture));
            resultBuilder.AddParameter("Столкновения с объектами", _collisionWithObjectsCount.ToString(CultureInfo.InvariantCulture));
            resultBuilder.AddParameter("Потери сигнала из-за помех ЛЭП", _signalLossesPliCount.ToString(CultureInfo.InvariantCulture));
            resultBuilder.AddParameter("Потери сигнала из-за РЭБ", _signalLossesRebCount.ToString(CultureInfo.InvariantCulture));
            resultBuilder.AddParameter("Потери сигнала из-за препятствий", _signalLossesWallsCount.ToString(CultureInfo.InvariantCulture));
            resultBuilder.AddParameter("Потери сигнала из-за отдаления", _signalLossesFarCount.ToString(CultureInfo.InvariantCulture));
            resultBuilder.AddParameter("Столкновения с людьми", _collisionWithMensCount.ToString(CultureInfo.InvariantCulture));
            resultBuilder.AddParameter("Столкновения с животными", _collisionWithAnimalsCount.ToString(CultureInfo.InvariantCulture));
            resultBuilder.AddParameter("Столкновения с птицами", _collisionWithBirdsCount.ToString(CultureInfo.InvariantCulture));
        }
        
        private void OnDroneControllerInitialized()
        {
            var droneController = DroneController.Instance;
            var droneSensors = droneController.DroneSensors;
            var droneHealth = droneController.GetComponent<DroneHealthController>();
            
            //Удары
            droneHealth.OnObjectCollision += IncrementCollisionWithObjectsCount;
            droneHealth.OnMenCollision += IncrementCollisionWithMensCount;
            droneHealth.OnAnimalCollision += IncrementCollisionWithAnimalsCount;
            droneHealth.OnBirdCollision += IncrementCollisionWithBirdsCount;
            
            //Потеря связи
            droneSensors.BatteryDepleted += IncrementDischargeCount;
            droneSensors.SignalLostDueToDistance += IncrementSignalLossesFarCount;
            droneSensors.SignalLostDueToObstacles += IncrementSignalLossesWallsCount;
            droneSensors.SignalLostDueToPowerLine += IncrementSignalLossesPliCount;
            droneSensors.SignalLostDueToElectronicWarfare += IncrementSignalLossesRebCount;
        }

        private void IncrementDischargeCount() => _dischargeCount++;
        private void IncrementCollisionWithObjectsCount() => _collisionWithObjectsCount++;
        private void IncrementSignalLossesPliCount() => _signalLossesPliCount++;
        private void IncrementSignalLossesRebCount() => _signalLossesRebCount++;
        private void IncrementSignalLossesWallsCount() => _signalLossesWallsCount++;
        private void IncrementSignalLossesFarCount() => _signalLossesFarCount++;
        private void IncrementCollisionWithMensCount() => _collisionWithMensCount++;
        private void IncrementCollisionWithAnimalsCount() => _collisionWithAnimalsCount++;
        private void IncrementCollisionWithBirdsCount() => _collisionWithBirdsCount++;
        
        private void OnDestroy()
        {
            var droneController = DroneController.Instance;
            if(!droneController) 
                return;
            
            var droneSensors = droneController.DroneSensors;
            var droneHealth = droneController.GetComponent<DroneHealthController>();
            
            //Удары
            droneHealth.OnObjectCollision -= IncrementCollisionWithObjectsCount;
            droneHealth.OnMenCollision -= IncrementCollisionWithMensCount;
            droneHealth.OnAnimalCollision -= IncrementCollisionWithAnimalsCount;
            droneHealth.OnBirdCollision -= IncrementCollisionWithBirdsCount;
            
            //Потеря связи
            droneSensors.BatteryDepleted -= IncrementDischargeCount;
            droneSensors.SignalLostDueToDistance -= IncrementSignalLossesFarCount;
            droneSensors.SignalLostDueToObstacles -= IncrementSignalLossesWallsCount;
            droneSensors.SignalLostDueToPowerLine -= IncrementSignalLossesPliCount;
            droneSensors.SignalLostDueToElectronicWarfare -= IncrementSignalLossesRebCount;
        }
    }
}