using System;
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
        [SerializeField] private Waypoint[] _waypoints;

        private RaceCondition _raceCondition;
        private float time;
        private int currentWaypoint;
        
        private void Awake()
        {
            time = 0;
            _raceCondition = RaceCondition.Waiting;
        }

        private void Update()
        {
            if (_raceCondition == RaceCondition.Running)
                time++;
        }

        public void WaypointUpdate(Waypoint waypoint)
        {
            
        }
    }
}