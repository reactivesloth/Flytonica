using System;
using System.Collections.Generic;
using Code.Internal.Drone;
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
        public GameObject finingObject;
        public bool finded = false;

        public SearchingObject(string descriptionTask, GameObject finingObject)
        {
            this.descriptionTask = descriptionTask;
            this.finingObject = finingObject;
            finded = false;
        }
    }

    public class ScenarioSearching : ScenarioBase
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

        // Добавлены переменные для подсчета сканирований
        private int _totalScanAttempts = 0;

        protected override void Update()
        {
            base.Update();
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
        }

        private void FindObject(SearchingObject o)
        {
            if (_raceCondition == RaceCondition.Running)
            {
                if (_gazeTime > 3)
                {
                    DroneHUD.Instance.AimElement.Flash(Color.green, 1, () =>
                    {
                        _gazeTime = 0;
                        _totalScanAttempts++;
                        
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
                _totalScanAttempts++;
            });
        }

        protected override void StartRace()
        {
            base.StartRace();

            _counter = 0;
            if (timer > 0)
            {
                _timer = timer;
            }

            _raceCondition = RaceCondition.Running;

            DroneHUD.Instance.SetMessage(MessageType.Normal,
                $"Вам необходимо сфотографировать {searchingObjects.Count} объектов.\nНайдите {collectionName}.\nКамера работает с 15 метров.", 3);
            DroneHUD.Instance.SetTask($"Найти и сфотографировать объекты [{_findedCount}/{searchingObjects.Count}]");
        }

        protected override void FinishRace(bool success = true)
        {
            base.FinishRace(success);

            _raceCondition = RaceCondition.Finished;
            DroneHUD.Instance.ClearMessage();
            DroneHUD.Instance.SetTask(success ? "Задание выполнено!" : "Задание провалено!");
            DroneInput.Instance.MenuCameraHandle(true);

            string objectResult = string.Empty;
            foreach (var searchingObject in searchingObjects)
            {
                objectResult += "\n" + searchingObject.descriptionTask +
                                (searchingObject.finded ? ": Найден" : ": Не найден");
            }

            string title = success ? "Уровень пройден!" : "Время вышло!";
            string message = success
                ? $"Поздравляем! Вы нашли все объекты:{objectResult}\nВремя выполнения: {GetTimeWithMs(_counter)}\n" +
                  $"Общее количество попыток сканирования: {_totalScanAttempts}\n"
                : $"Вы нашли [{_findedCount}/{searchingObjects.Count}] объектов:{objectResult}\n" +
                  $"Общее количество попыток сканирования: {_totalScanAttempts}\n";

            PopupPanel.ConfigurePopup(title, message,
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
            FinalScore -= (_totalScanAttempts - _findedCount) * 5f; // Штраф за дополнительные сканирования
            FinalScore -= (1f - (float)_findedCount / searchingObjects.Count) * 100; // Штраф за ненайденные объекты

            float penaltyTime = 0;
            if (TotalTime > 15f * 60f)
                penaltyTime = 20;
            else if (TotalTime > 25f * 60f)
                penaltyTime = 50;
            FinalScore -= penaltyTime;

            base.AddStatistic();

            var resultBuilder = ReportBuilder.Instance;

            resultBuilder.AddParameter($"Количество попыток сканирования", _totalScanAttempts.ToString());
            resultBuilder.AddParameter($"Количество найденных объектов", $"{_findedCount}/{searchingObjects.Count}");

            foreach (var searchingObject in searchingObjects)
            {
                string objectName = searchingObject.finingObject.name.Replace("(Clone)", "");
                resultBuilder.AddParameter($"{objectName}", searchingObject.finded ? "Найден" : "Не найден");
            }
        }

        private void UpdateTask()
        {
            DroneHUD.Instance.SetTask($"Найти и сфотографировать объекты [{_findedCount}/{searchingObjects.Count}]");
        }

        public override void Initialize(ScenarioSettings scenario)
        {
            base.Initialize(scenario);
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
