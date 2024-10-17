using System;
using System.Collections.Generic;
using Code.Internal.Drone;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface;
using Code.Internal.UserInterface.DroneHudElements;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Internal.Scenario.Race
{
    public enum RaceCondition
    {
        Waiting,
        Running,
        Finished
    }

    public class ScenarioRace : ScenarioBase
    {
        [SerializeField] private List<Checkpoint> checkpoints;

        private int _nextCheckpoint = 0;
        private RaceCondition _raceCondition;
        private float _time;

        private void Awake()
        {
            _raceCondition = RaceCondition.Waiting;
        }

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
            if (_raceCondition == RaceCondition.Running)
            {
                _time += Time.deltaTime;
            }

            DroneHUD.Instance?.SetTime(GetResult());
        }

        public void CheckpointUpdate(Checkpoint checkpoint)
        {
            //if (_raceCondition != RaceCondition.Running) return;

            if (checkpoints.IndexOf(checkpoint) == _nextCheckpoint)
            {
                _nextCheckpoint++;

                if (_raceCondition == RaceCondition.Waiting)
                {
                    StartRace();
                }
                else if (_raceCondition == RaceCondition.Running)
                {
                    if (_nextCheckpoint == checkpoints.Count)
                    {
                        FinishRace();
                    }
                    else
                    {
                        UpdateCheckpointColors();

                        // Correct gate
                    }
                }
            }
            else
            {
                // Not correct gate
            }
        }

        private void UpdateCheckpointColors()
        {
            foreach (var cp in checkpoints)
            {
                cp.ChangeColor(CheckpointFlashType.None);
            }

            checkpoints[_nextCheckpoint].ChangeColor(CheckpointFlashType.Current);
            if (_nextCheckpoint + 1 < checkpoints.Count)
                checkpoints[_nextCheckpoint + 1].ChangeColor(CheckpointFlashType.Next);
        }

        protected override void StartRace()
        {
            base.StartRace();
            _time = 0;
            _raceCondition = RaceCondition.Running;
            DroneHUD.Instance?.SetTask("Выполняйте пролет через зеленые кольца");

            UpdateCheckpointColors();
        }

        protected override void FinishRace(bool success = true)
        {
            base.FinishRace(success);
            _raceCondition = RaceCondition.Finished;
            DroneHUD.Instance?.SetTask("Задание выполнено!");
            DroneHUD.Instance.SetMessage(MessageType.Normal, "Поздравляем! Ваше время: " + GetResult());
            DroneInput.Instance.MenuCameraHandle(true);

            foreach (var cp in checkpoints)
            {
                cp.ChangeColor(CheckpointFlashType.Current);
            }

            PopupPanel.ConfigurePopup("Задание выполнено!", $"Подздравляем! Время выполнения: {GetResult()}",
                null, "Выйти в главное меню", Color.red, Color.white,
                () => { ScenarioSwitcherController.Instance.EndSession(); },
                null, "Продолжить", Color.green, Color.black, () =>
                {
                    var resultBuilder = ReportBuilder.Instance;

                    resultBuilder.AddParameter($"{SceneManager.GetActiveScene().name}_Время", GetResult());
                    // foreach (отклонения от центров колец в процентах)
                    // {
                    //     resultBuilder.AddParameter($"{SceneManager.GetActiveScene().name}_" + "gateName"), "Отклонение от центра: " + "percent");
                    // }

                    ScenarioSwitcherController.Instance.NextOrEnd();
                });
        }

        public string GetResult()
        {
            TimeSpan time = TimeSpan.FromSeconds(GetResultInSeconds());
            DateTime dateTime = DateTime.Today.Add(time);
            return dateTime.ToString("mm:ss:fff");
        }

        public float GetResultInSeconds()
        {
            return _time;
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

        public override void Initialize(ScenarioSettings scenario)
        {
            base.Initialize(scenario);
            var objects = GetComponentsInChildren<SpawnableObject>();
            checkpoints = new List<Checkpoint>();

            foreach (var o in objects)
            {
                if (o.Type == MapEditorObjectType.RacingGate)
                {
                    var gatePoints = o.GetComponentsInChildren<Checkpoint>();
                    foreach (var c in gatePoints)
                    {
                        checkpoints.Add(c);
                    }
                }
            }

            foreach (var o in objects)
            {
                if (o.Type == MapEditorObjectType.StartGate)
                {
                    checkpoints.Insert(0, o.GetComponentInChildren<Checkpoint>());
                }

                if (o.Type == MapEditorObjectType.FinishGate)
                {
                    checkpoints.Insert(checkpoints.Count, o.GetComponentInChildren<Checkpoint>());
                }
            }
        }
    }
}