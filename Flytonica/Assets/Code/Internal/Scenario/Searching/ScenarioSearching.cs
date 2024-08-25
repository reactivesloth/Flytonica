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

        private float _gazeTime;
        private float _gazeTimeNotResponceTime;
        
        private void Start()
        {
            if (_raceCondition == RaceCondition.Waiting)
            {
                StartRace();
            }
        }

        private void Update()
        {
            if (Camera.main == null) return;
            
            if (_raceCondition == RaceCondition.Running)
            {
                _time += Time.deltaTime;

                Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
                if (searchingObjects[currentObject] != null)
                {
                    if (Physics.Raycast(ray, out var hit, 15))
                    {
                        foreach (var finingObject in searchingObjects[currentObject].finingObjects)
                        {
                            if (finingObject.GetComponent<Collider>() == hit.collider)
                            {
                                FindObject();
                                break;
                            }
                        }
                    }
                }
            }
            CancelFinding();
            DroneHUD.Instance?.SetTime(GetResult ());
        }

        private void FindObject()
        {
            if (_raceCondition == RaceCondition.Running)
            {
                if (_gazeTime > 3)
                {
                    DroneHUD.Instance.AimElement.Flash(Color.green, 1, () =>
                    {
                        if (currentObject + 1 >= searchingObjects.Length)
                        {
                            FinishRace();
                        }
                        else
                        {
                            UpdateTask();
                        }
                    });
                }
                else
                {
                    _gazeTime += Time.deltaTime;
                    _gazeTimeNotResponceTime = 0;
                    DroneHUD.Instance.AimElement.SetProgressValue(_gazeTime/3);
                }
            }
        }

        private void CancelFinding()
        {
            switch (_gazeTime)
            {
                case 0:
                    _gazeTimeNotResponceTime = 0;
                    break;
                case > 0:
                    _gazeTimeNotResponceTime += Time.deltaTime;
                    break;
            }

            if (!(_gazeTimeNotResponceTime > 2)) return;
            _gazeTimeNotResponceTime = 0;
                
            DroneHUD.Instance.AimElement.Flash(Color.red, 1, () =>
            {
                _gazeTime = 0;
            });
        }
        
        private void StartRace()
        {
            _time = 0;
            _raceCondition = RaceCondition.Running;
            var search = searchingObjects[0].descriptionTask;
            
            UISubtitle.Instance.SetTextInstant($"Вам необходимо сфотографировать {searchingObjects.Length} объектов." + $"\nНайдите {search}." + "\nКамера работает с 15 метров.", 3);
            DroneHUD.Instance.SetTask($"Найдите и сфотографируйте объект: {search}");
            
        }

        private void FinishRace()
        {
            _raceCondition = RaceCondition.Finished;
            UISubtitle.Instance.SetTextInstant("Поздравляем! Ваше время: " + GetResult());
            DroneHUD.Instance.SetTask("Задание выполнено!");
        }
        
        private void UpdateTask()
        {
            currentObject++;
            var search = searchingObjects[currentObject].descriptionTask;
            UISubtitle.Instance.SetTextInstant($"Отличная работа! А теперь найдите {search}");
            DroneHUD.Instance.SetTask($"Найдите и сфотографируйте объект: {search}");
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