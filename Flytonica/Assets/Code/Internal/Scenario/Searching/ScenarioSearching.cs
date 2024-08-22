using System;
using Code.Internal.Scenario.Race;
using Code.Internal.UserInterface;
using UnityEngine;

namespace Code.Internal.Scenario.Searching
{
    [Serializable]
    public class SearchingObject
    {
        public string descriptionTask;
        public GameObject[] finingObjects;
    }
    
    public class ScenarioSearching : MonoBehaviour
    {
        private RaceCondition _raceCondition = RaceCondition.Waiting;
        
        [SerializeField] private SearchingObject[] searchingObjects;
        private int currentObject = 0;
        private float _time;

        private void Start()
        {
            if (_raceCondition == RaceCondition.Waiting)
            {
                StartRace();
            }
        }

        private void Update()
        {
            if (_raceCondition == RaceCondition.Running)
            {
                _time += Time.deltaTime;

                Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
                if (Physics.Raycast(ray, out var hit, 35))
                {
                    foreach (var finingObject in searchingObjects[currentObject].finingObjects)
                    {
                        if (finingObject.GetComponent<Collider>() == hit.collider)
                        {
                            FindObject();
                        }
                    }
                }
            }
        }

        private void FindObject()
        {
            if (_raceCondition == RaceCondition.Running)
            {
                currentObject++;
                
                if (currentObject >= searchingObjects.Length)
                {
                    FinishRace();
                }
                else
                {
                    UpdateTask();
                }
            }
        }
        
        private void StartRace()
        {
            _time = 0;
            _raceCondition = RaceCondition.Running;
            var search = searchingObjects[0].descriptionTask;
            UISubtitle.Instance?.SetTextInstant($"Вам необходимо найти {searchingObjects.Length} объектов." +
                                                $"\nНайдите {search}", 3);
        }

        private void FinishRace()
        {
            _raceCondition = RaceCondition.Finished;
            UISubtitle.Instance?.SetTextInstant("Поздравляем! Ваше время: " + GetResult());
        }
        
        private void UpdateTask()
        { 
            var search = searchingObjects[currentObject].descriptionTask;
            UISubtitle.Instance.SetTextInstant($"Отлично!\nНайдите {search}", 3);
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
    }
}