using Code.Internal.SceneManagement;
using Code.Internal.UserInterface;
using UnityEngine;

namespace Code.Internal.Scenario.Tutorial
{
    public class ScenarioTutorial : ScenarioBase
    {
        private float _timeTakeoff, _timeRacing;
        
        private void Start()
        {
            DroneHUD.Instance?.SetTask("Выполняйте задания обучения");
        }

        protected override void Update()
        {
            base.Update();
#if UNITY_EDITOR
            if (UnityEngine.Input.GetKeyDown(KeyCode.Tab))
            {
                FinishRace();
            }
#endif
        }

        public override void Initialize(ScenarioSettings scenario)
        {
            base.Initialize(scenario);
            
            if (ScenarioCondition == ScenarioCondition.Waiting)
            {
                StartRace();
            }
        }

        protected override void StartRace()
        {
            base.StartRace();
            _timeTakeoff = 0;
            _timeRacing = 0;

            var scenarioRoutine = GetComponentInChildren<ScenarioTutorialRoutine>();
            if (scenarioRoutine != null)
                scenarioRoutine.Initialize();
        }

        public void LeaveTutorial()
        {
            FinishRace(false);
        }

        public void CompleteTutorial()
        {
            FinishRace();
        }
        
        protected override void FinishRace(bool success = true)
        {
            base.FinishRace(success);
        }

        protected override void AddStatistic()
        {
            base.AddStatistic();

            var resultBuilder = ReportBuilder.Instance;
            resultBuilder.AddParameter($"Время на взлёт", GetTime(_timeTakeoff));
            resultBuilder.AddParameter($"Время прохождения задания", GetTime(_timeRacing));
        }
    }
}