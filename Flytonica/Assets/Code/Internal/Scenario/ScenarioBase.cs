using System;
using System.Globalization;
using Code.Internal.API;
using Code.Internal.SceneManagement;
using UnityEngine;

namespace Code.Internal.Scenario
{
    public abstract class ScenarioBase : MonoBehaviour
    {
        private float _totalTime;
        protected ScenarioSettings CurrentScenario;

        public bool IsStarting { get; protected set; }

        protected virtual void Update()
        {
            if (IsStarting)
            {
                _totalTime += Time.deltaTime;
            }
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
            var resultBuilder = ReportBuilder.Instance;

            //Фиксируем другие неоцениваемые параметры
            resultBuilder.AddParameter("Название сценария", CurrentScenario.name);
            resultBuilder.AddParameter("Тип сценария", CurrentScenario.scenarioType.GetName());
            resultBuilder.AddParameter("Модель дрона", CurrentScenario.currentDrone?.name);
            resultBuilder.AddParameter("Успешность сценария", success ? "Да" : "Нет");
            resultBuilder.AddParameter("Время выполнения задания", $"{(int)(_totalTime / 60):D2}:{(int)(_totalTime % 60):D2}");
            resultBuilder.AddParameter("Количество перезапусков сценария", 0);

            IsStarting = false;
        }
        
        public virtual void Initialize(ScenarioSettings scenario)
        {
            CurrentScenario = scenario;
        }
    }
}