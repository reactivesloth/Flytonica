using System;
using System.Collections.Generic;
using Code.Internal.Scenario.Race;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface;
using Code.Internal.UserInterface.DroneHudElements;
using UnityEngine;

namespace Code.Internal.Scenario.Searching
{
    [Serializable]
    public class SearchingObject
    {
        public string descriptionTask;
        public GameObject[] finingObjects;
        public bool finded = false;
    }
    
    public class ScenarioSearching : MonoBehaviour
    {
        private RaceCondition _raceCondition = RaceCondition.Waiting;

        [SerializeField] private string collectionName;
        [SerializeField] private SearchingObject[] searchingObjects;
        [SerializeField] private float timer = 300f;
        private float _timer;
        private float _counter;

        private float _gazeTime;
        private float _gazeTimeNotResponceTime;
        private int _findedCount = 0;
        
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
                _counter += Time.deltaTime;
                _timer -= Time.deltaTime;

                if (_timer <= 0)
                {
                    FinishRace(false);
                }

                Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
                if (searchingObjects.Length > 0)
                {
                    if (Physics.Raycast(ray, out var hit, 15))
                    {
                        foreach (var searchingObject in searchingObjects)
                        {
                            if (!searchingObject.finded)
                            {
                                foreach (var findingObject in searchingObject.finingObjects)
                                {
                                    if (findingObject.GetComponent<Collider>() == hit.collider)
                                    {
                                        FindObject(searchingObject);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            CancelFinding();
            DroneHUD.Instance?.SetTime(GetResult (_timer));
        }

        private void FindObject(SearchingObject o)
        {
            if (_raceCondition == RaceCondition.Running)
            {
                if (_gazeTime > 3)
                {
                    DroneHUD.Instance.AimElement.Flash(Color.green, 1, () =>
                    {
                        o.finded = true;
                        _findedCount += 1;
                        DroneHUD.Instance.SetMessage(MessageType.Normal, $"Найден объект {o.descriptionTask}", 3);
                        
                        if (_findedCount == searchingObjects.Length)
                        {
                            FinishRace(true);
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
            _counter = 0;
            _timer = timer;
            _raceCondition = RaceCondition.Running;
            
            DroneHUD.Instance.SetMessage(MessageType.Normal,$"Вам необходимо сфотографировать {searchingObjects.Length} объектов." + $"\nНайдите {collectionName}." + "\nКамера работает с 15 метров.", 3);
            DroneHUD.Instance.SetTask($"Найти и сфотографировать объекты [{_findedCount}/{searchingObjects.Length}]");
            
        }

        private void FinishRace(bool success)
        {
            _raceCondition = RaceCondition.Finished;
            DroneHUD.Instance.ClearMessage();
            DroneHUD.Instance.SetTask(success ? "Задание выполнено!" : "Задание провалено!");

            string ojbectResult = string.Empty;
            foreach (var searchingObject in searchingObjects)
            {
                ojbectResult += "\n" + searchingObject.descriptionTask + (searchingObject.finded ? "Найден" : "Не найден");
            }
            
            PopupPanel.ConfigurePopup(success ? "Уровень пройден!" : "Время вышло!", success ? $"Подздравляем! Вы нашли все объекты: {ojbectResult} \n Время выполнения: {GetResult(_counter)}" : $"Вы нашли [{_findedCount}/{searchingObjects.Length}] объектов: {ojbectResult}",
                null, "Выйти в главное меню", Color.red, Color.white, () =>
                {
                    ScenarioSwitcherController.Instance.EndSession();
                }, 
                null, "Продолжить", Color.green, Color.black, () =>
                {
                    Dictionary<string, string> result = new Dictionary<string, string>();

                    
                    result.Add("Время", GetResult(_counter));
                    foreach (var searchingObject in searchingObjects)
                    {
                        result.Add(searchingObject.finingObjects[0].name, searchingObject.finded ? "Найден" : "Не найден");
                    }
                    
                    ScenarioSwitcherController.Instance.NextOrEnd(result);
                });
        }
        
        private void UpdateTask()
        {
            DroneHUD.Instance.SetTask($"Найти и сфотографировать объекты [{_findedCount}/{searchingObjects.Length}]");
        }
        
        public string GetResult(float t)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(t);
            DateTime dateTime = DateTime.Today.Add(timeSpan);
            return dateTime.ToString("mm:ss");
        }
    }
}