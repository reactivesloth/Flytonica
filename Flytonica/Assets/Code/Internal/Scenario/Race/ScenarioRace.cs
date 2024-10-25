using System.Collections.Generic;
using System.Linq;
using Code.Internal.Drone;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface;
using Code.Internal.UserInterface.DroneHudElements;
using UnityEngine;

namespace Code.Internal.Scenario.Race
{
    public enum RaceState
    {
        None,
        Takeoff,
        Racing
    }

    public class ScenarioRace : ScenarioBase
    {
        [SerializeField] private List<Checkpoint> checkpoints;

        private int _nextCheckpoint = 0;
        private RaceState _raceState = RaceState.None;
        private float _timeTakeoff, _timeRacing;

        private void Start()
        {
            DroneHUD.Instance?.SetTask("Пролетите через стартовое кольцо чтобы начать гонку");
            DroneHUD.Instance?.SetMessage(MessageType.Normal, "Пролетите через стартовое кольцо чтобы начать гонку", 3);
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

            TimeUpdate();
        }

        private void TimeUpdate()
        {
            switch (_raceState)
            {
                case RaceState.Takeoff:
                    _timeTakeoff += Time.deltaTime;
                    break;
                case RaceState.Racing:
                    _timeRacing += Time.deltaTime;
                    break;
            }
        }

        /// <summary>
        /// Обработка пересечения чекпоинта
        /// </summary>
        /// <param name="checkpoint"></param>
        public void CheckpointUpdate(Checkpoint checkpoint)
        {
            if (checkpoint == checkpoints.First() && _raceState == RaceState.Takeoff)
            {
                _raceState = RaceState.Racing;
                DroneHUD.Instance?.SetTask("Выполняйте пролет через зеленые кольца");
                Debug.Log($"Гонка началась. Время взлёта: {GetTime(_timeTakeoff)}");Debug.Log($"Гонка началась. Время взлёта: {GetTime(_timeTakeoff)}");
            }
            
            if (checkpoint == checkpoints.Last() && _raceState == RaceState.Racing)
            {
                FinishRace();
                Debug.Log($"Гонка закончилась. Время прохождения: {GetTime(_timeRacing)}");Debug.Log($"Гонка закончилась. Время прохождения: {GetTime(_timeRacing)}");
            }
            
            if (checkpoints.IndexOf(checkpoint) == _nextCheckpoint && _nextCheckpoint + 1 < checkpoints.Count)
            {
                _nextCheckpoint++;
                
                if (ScenarioCondition == ScenarioCondition.Running)
                {
                    SetNextCheckpoints();
                }
            }
        }

        private void SetNextCheckpoints()
        {
            checkpoints[_nextCheckpoint].ChangeStatus(CheckpointStatus.Current);
            if (_nextCheckpoint + 1 < checkpoints.Count)
                checkpoints[_nextCheckpoint + 1].ChangeStatus(CheckpointStatus.Next);
        }

        public override void Initialize(ScenarioSettings scenario)
        {
            base.Initialize(scenario);

            List<SpawnableObject> objects = new List<SpawnableObject>(GetComponentsInChildren<SpawnableObject>().OrderBy(o => o.sortOrder));
            foreach (var obj in objects)
            {
                obj.transform.SetSiblingIndex(obj.sortOrder);
            }
            
            var sortOrder  = 0;
            for (var i = 0; i < objects.Count(); i++)
            {
                if (objects[i].Type == MapEditorObjectType.RacingGate)
                {
                    var gatePoints = objects[i].GetComponentsInChildren<Checkpoint>();
                    foreach (var c in gatePoints)
                    {
                        c.ChangeStatus(CheckpointStatus.None);
                        checkpoints.Add(c);
                        checkpoints.Last().sortOrder = sortOrder;
                    }

                    checkpoints.OrderBy(cp => cp.sortOrder);
                }
            }

            foreach (var o in objects)
            {
                if (o.Type == MapEditorObjectType.StartGate)
                {
                    var gatePoints = o.GetComponentsInChildren<Checkpoint>();
                    for (int i = 0; i < gatePoints.Length; i++)
                    {
                        gatePoints[i].ChangeStatus(CheckpointStatus.None);
                        checkpoints.Insert(i, gatePoints[i]);
                    }
                }

                if (o.Type == MapEditorObjectType.FinishGate)
                {
                    var gatePoints = o.GetComponentsInChildren<Checkpoint>();
                    
                    var place = checkpoints.Count;
                    foreach (var gatePoint in gatePoints)
                    {
                        gatePoint.ChangeStatus(CheckpointStatus.None);
                        checkpoints.Insert(place, gatePoint);
                        place++;
                    }
                }
            }

            
            SetNextCheckpoints();

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
            _raceState = RaceState.Takeoff;

            DroneHUD.Instance?.SetTask("Выполняйте пролет через зеленые кольца");

            SetNextCheckpoints();
        }

        protected override void FinishRace(bool success = true)
        {
            _raceState = RaceState.None;
            base.FinishRace(success);

            foreach (var cp in checkpoints)
            {
                cp.SetEndColor(cp.checkpointStatus == CheckpointStatus.Passed);
            }

            PopupPanel.ConfigurePopup("Задание выполнено!",
                $"Подздравляем! Время выполнения: {GetTimeWithMs(TotalTime)}",
                null, "Выйти в главное меню", Color.red, Color.white,
                () => { ScenarioSwitcherController.Instance.EndSession(); },
                null, "Продолжить", Color.green, Color.black, () =>
                {
                    AddStatistic();

                    ScenarioSwitcherController.Instance.NextOrEnd();
                });
        }

        protected override void AddStatistic()
        {
            var passedCount = checkpoints.Count(c =>
                c.checkpointStatus == CheckpointStatus.Passed && c.checkpointType == CheckpointType.Checkpoint);

            FinalScore -= (1f - (float)passedCount / checkpoints.Count) * 100f; //Штраф за непройденные чекпоинты

            base.AddStatistic();

            var resultBuilder = ReportBuilder.Instance;

            resultBuilder.AddParameter($"Количество пройденных чекпоинтов", $"{passedCount}/{checkpoints.Count}");
            foreach (var checkpoint in checkpoints)
            {
                var status = checkpoint.checkpointStatus == CheckpointStatus.Passed
                    ? $"Пройдена. \nОтклонение от центра: {(checkpoint.deviationFromCentre * 100):F0}%"
                    : "Не пройдена";
                resultBuilder.AddParameter($"Точка {checkpoints.IndexOf(checkpoint) + 1}", status);
            }

            resultBuilder.AddParameter($"Время на взлёт", GetTime(_timeTakeoff));
            resultBuilder.AddParameter($"Время прохождения трассы", GetTime(_timeRacing));
        }

        private void OnDrawGizmos()
        {
            for (int i = 0; i < checkpoints.Count; i++)
            {
                Gizmos.color = Color.yellow;
                if (i < checkpoints.Count - 1)
                    Gizmos.DrawLine(checkpoints[i].transform.position + Vector3.up,
                        checkpoints[i + 1].transform.position + Vector3.up);
            }
        }
    }
}