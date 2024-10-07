using System;
using System.Collections.Generic;
using System.Linq;
using Code.Internal.UserInterface;
using Code.Internal.UserInterface.DroneHudElements;
using UnityEngine;

namespace Code.Internal.Scenario.Race
{
    public enum RaceCondition
    {
        Waiting,
        Running,
        Finished
    }
    
    public class ScenarioRace : MonoBehaviour
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
            DroneHUD.Instance?.SetMessage(MessageType.Normal,"Пролетите через стартовое кольцо чтобы начать гонку", 3);
        }

        private void Update()
        {
            if (_raceCondition == RaceCondition.Running)
            {
                _time += Time.deltaTime;
            }
            DroneHUD.Instance?.SetTime(GetResult ());
        }

        public void CheckpointUpdate(Checkpoint checkpoint)
        {
            if (_raceCondition != RaceCondition.Running) return;
            
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

        private void StartRace()
        {
            _time = 0;
            _raceCondition = RaceCondition.Running;
            DroneHUD.Instance?.SetTask("Выполняйте пролет через зеленые кольца");
            
            UpdateCheckpointColors();
        }

        private void FinishRace ()
        {
            _raceCondition = RaceCondition.Finished;
            DroneHUD.Instance?.SetTask("Задание выполнено!");
            DroneHUD.Instance.SetMessage(MessageType.Normal,"Поздравляем! Ваше время: " + GetResult());
            
            foreach (var cp in checkpoints)
            {
                cp.ChangeColor(CheckpointFlashType.Current);
            }
            
            //ScenarioSwitcherController.Instance.NextOrEnd();
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
                    Gizmos.DrawLine(checkpoints[i].transform.position + Vector3.up, checkpoints[i+1].transform.position + Vector3.up);
            }
        }

        public void Initialize()
        {
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
            
            if (_raceCondition == RaceCondition.Waiting)
            {
                StartRace();
            }
        }
    }
}