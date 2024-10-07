using System;
using System.Collections.Generic;
using Code.Internal.Scenario.Race;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface;
using Code.Internal.UserInterface.DroneHudElements;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Internal.Scenario.Searching
{
    [Serializable]
    public class SearchingObject
    {
        public string descriptionTask;
        public GameObject finingObject;
        public bool finded = false;

        public SearchingObject(string descriptionTask, GameObject finingObject)
        {
            this.descriptionTask = descriptionTask;
            this.finingObject = finingObject;
            finded = false;
        }
    }
    
    public class ScenarioSearching : MonoBehaviour
    {
        private RaceCondition _raceCondition = RaceCondition.Waiting;

        [SerializeField] private string collectionName;
        [SerializeField] private List<SearchingObject> searchingObjects;
        [SerializeField] private float timer = 300f;
        private float _timer;
        private float _counter;

        private float _gazeTime;
        private float _gazeTimeNotResponceTime;
        private int _findedCount = 0;
        
        private void Update()
        {
            #if UNITY_EDITOR
            if (UnityEngine.Input.GetKeyDown(KeyCode.Tab))
            {
                FinishRace(true);
            }
            #endif
            
            if (Camera.main == null) return;
            
            if (_raceCondition == RaceCondition.Running)
            {
                _counter += Time.deltaTime;
                if (timer > 0)
                {
                    _timer -= Time.deltaTime;

                    if (_timer <= 0)
                    {
                        FinishRace(false);
                    }
                }

                Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
                if (searchingObjects.Count > 0)
                {
                    if (Physics.Raycast(ray, out var hit, 15))
                    {
                        foreach (var searchingObject in searchingObjects)
                        {
                            if (!searchingObject.finded)
                            {
                                if (searchingObject.finingObject.GetComponent<Collider>() == hit.collider)
                                {
                                    FindObject(searchingObject);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            CancelFinding();
            
            if (timer > 0)
                DroneHUD.Instance?.SetTime(GetResult (_timer));
            else
                DroneHUD.Instance?.SetTime(GetResult (_counter));
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
                        
                        if (_findedCount == searchingObjects.Count)
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
            if (timer > 0)
            {
                _timer = timer;
            }

            _raceCondition = RaceCondition.Running;
            
            DroneHUD.Instance.SetMessage(MessageType.Normal,$"Вам необходимо сфотографировать {searchingObjects.Count} объектов." + $"\nНайдите {collectionName}." + "\nКамера работает с 15 метров.", 3);
            DroneHUD.Instance.SetTask($"Найти и сфотографировать объекты [{_findedCount}/{searchingObjects.Count}]");
            
        }

        private void FinishRace(bool success)
        {
            _raceCondition = RaceCondition.Finished;
            DroneHUD.Instance.ClearMessage();
            DroneHUD.Instance.SetTask(success ? "Задание выполнено!" : "Задание провалено!");

            string ojbectResult = string.Empty;
            foreach (var searchingObject in searchingObjects)
            {
                ojbectResult += "\n" + searchingObject.descriptionTask + (searchingObject.finded ? ": Найден" : ": Не найден");
            }
            
            PopupPanel.ConfigurePopup(success ? "Уровень пройден!" : "Время вышло!", success ? $"Подздравляем! Вы нашли все объекты: {ojbectResult} \n Время выполнения: {GetResult(_counter)}" : $"Вы нашли [{_findedCount}/{searchingObjects.Count}] объектов: {ojbectResult}",
                null, "Выйти в главное меню", Color.red, Color.white, () =>
                {
                    ScenarioSwitcherController.Instance.EndSession();
                }, 
                null, "Продолжить", Color.green, Color.black, () =>
                {
                    var resultBuilder = ReportBuilder.Instance;

                    resultBuilder.AddParameter($"{SceneManager.GetActiveScene().name}_Время", GetResult(_counter));
                    foreach (var searchingObject in searchingObjects)
                    {
                        resultBuilder.AddParameter($"{SceneManager.GetActiveScene().name}_" + searchingObject.finingObject.name.Replace("(Clone)", ""), searchingObject.finded ? "Найден" : "Не найден");
                    }
                    
                    ScenarioSwitcherController.Instance.NextOrEnd();
                });
        }
        
        private void UpdateTask()
        {
            DroneHUD.Instance.SetTask($"Найти и сфотографировать объекты [{_findedCount}/{searchingObjects.Count}]");
        }
        
        public string GetResult(float t)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(t);
            DateTime dateTime = DateTime.Today.Add(timeSpan);
            return dateTime.ToString("mm:ss");
        }

        public void Initialize()
        {
            var objects = GetComponentsInChildren<SpawnableObject>();
            searchingObjects = new List<SearchingObject>();

            foreach (var o in objects)
            {
                 if (o.Type == MapEditorObjectType.SearchingObject)
                     searchingObjects.Add(new SearchingObject(o.name.Replace("(Clone)", ""), o.gameObject));
            }
            
            if (_raceCondition == RaceCondition.Waiting)
            {
                StartRace();
            }
        }
    }
}