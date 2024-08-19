using System;
using System.Collections.Generic;
using System.Linq;
using Code.Internal.UserInterface;
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

        private void Update()
        {
            if (_raceCondition == RaceCondition.Running)
                _time += Time.deltaTime;
        }

        private void OnValidate()
        {
            checkpoints = gameObject.GetComponentsInChildren<Checkpoint>(true).ToList();
        }

        public void CheckpointUpdate(Checkpoint checkpoint)
        {
            if (_raceCondition == RaceCondition.Finished) return;
            
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
                        UISubtitle.Instance?.SetTextInstant("Чекпоинт пройден!", 1.5f);
                    }
                }
            }
            else
            {
                UISubtitle.Instance?.SetTextInstant("Неправильные ворота :(", 1.5f);
            }
        }

        private void StartRace()
        {
            _time = 0;
            _raceCondition = RaceCondition.Running;
            UISubtitle.Instance?.SetTextInstant("Гонка началась!", 1.5f);
        }

        private void FinishRace ()
        {
            _raceCondition = RaceCondition.Finished;
            UISubtitle.Instance?.SetTextInstant("Поздравляем! Ваше время: " + GetResult());
        }

        public string GetResult()
        {
            TimeSpan time = TimeSpan.FromSeconds(GetResultInSeconds());
            DateTime dateTime = DateTime.Today.Add(time);
            return dateTime.ToString("mm:ss");
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
    }
}