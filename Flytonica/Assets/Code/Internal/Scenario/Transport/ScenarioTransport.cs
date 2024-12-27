using System.Collections.Generic;
using System.Linq;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface;
using Code.Internal.UserInterface.DroneHudElements;
using FishNet.Object;
using UnityEngine;

namespace Code.Internal.Scenario.Transport
{
    public class ScenarioTransport : ScenarioBase
    {
        private List<TakeZone> _takeZones;
        private List<DropZone> _dropZones;

        private int _deliveredCargoCount;

        public int DeliveredCargoCount => _deliveredCargoCount;
        
        public override void Initialize(ScenarioSettings scenario)
        {
            base.Initialize(scenario);
            
            var objects = GetComponentsInChildren<SpawnableObject>();

            _takeZones = new List<TakeZone>();
            _dropZones = new List<DropZone>();

            foreach (var spawnableObject in objects)
            {
                if (spawnableObject.TryGetComponent(out TakeZone takeZone))
                {
                    _takeZones.Add(takeZone);
                    var cargo = takeZone.SpawnCargo();;
                }
                else if (spawnableObject.TryGetComponent(out DropZone dropZone))
                {
                    _dropZones.Add(dropZone);
                    dropZone.Delivered += OnCargoDelivery;
                }
            }
        }

        protected override void StartRace()
        {
            base.StartRace();
            _deliveredCargoCount = 0;

            DroneHUD.Instance?.SetMessage(MessageType.Normal, "Добро пожаловать! В этом задании вам нужно доставить посылки в несколько точек. Будьте внимательны и осторожны!", 3);
            UpdateTask();
        }

        protected override void FinishRace(bool success = true)
        {
            base.FinishRace(success);
            
            
        }

        protected override void AddStatistic()
        {
            if (_takeZones.Sum(t => t.TakeCount) - _takeZones.Count > 0)
                FinalScore -= (_takeZones.Sum(t => t.TakeCount) - _takeZones.Count) * 5f; //Штраф за лишние поднятия
            print(FinalScore);
            FinalScore -= (1f - (float)_deliveredCargoCount / _dropZones.Count) * 100; //Штраф за недоставленные грузы
            print(FinalScore);
            
            print(_takeZones.Sum(c => c.FallCount));
            FinalScore -= _takeZones.Sum(c => c.FallCount) * 10f; // Штраф за падения груза
            print(FinalScore);
            FinalScore -= _takeZones.Sum(c => c.CollisionCount) * 5f; // Штраф за столкновение груза
            print(FinalScore);
            FinalScore -= 0.1f * (_dropZones.Sum(c => c.Precision) / _dropZones.Count) * 100f; //Штрфа за неточность
            print(FinalScore);
            float penaltyTime = 0;
            if (TotalTime > 10f * 60f)
                penaltyTime = 10;
            if (TotalTime > 15f * 60f)
                penaltyTime = 20f;
            FinalScore -= penaltyTime;
            print(FinalScore);
            base.AddStatistic();

            var resultBuilder = ReportBuilder.Instance;

            resultBuilder.AddParameter("Количесво поднятий грузов", _takeZones.Sum(t => t.TakeCount));
            resultBuilder.AddParameter("Количество падений груза", _takeZones.Sum(c => c.FallCount));
            resultBuilder.AddParameter("Количество столкновений груза", _takeZones.Sum(c => c.CollisionCount));

            resultBuilder.AddParameter($"Количество доставленых грузов", $"{_deliveredCargoCount}/{_dropZones.Count}");

            foreach (var dropZone in _dropZones)
            {
                var cargoResultText = dropZone.Precision > 0f
                    ? $"Доставлен. \nТочность сброса: {(dropZone.Precision * 100f):F0}%"
                    : "Не доставлен.";

                resultBuilder.AddParameter($"Груз \"{dropZone.GetComponent<SpawnableObject>()?.displayName}\"",
                    cargoResultText);
            }
        }

        private void OnCargoDelivery(CargoObject cargo)
        {
            _deliveredCargoCount++;
            _takeZones.FirstOrDefault(z => z.ConnectionId == cargo.ConnectionId)?.SetDelivery();
            UpdateTask();
        }

        private void UpdateTask()
        {
            if (_deliveredCargoCount >= _dropZones.Count)
            {
                DroneHUD.Instance.SetTask($"Вы доставили все предметы");
                FinishRace();
            }
            else
            {
                var taskText = string.Empty;
                taskText += $"Доставьте предметы {_deliveredCargoCount}/{_dropZones.Count} \n";
                
                /*foreach (var dropZone in _dropZones)
                {
                    var spawnable = dropZone.GetComponent<SpawnableObject>();

                    taskText += $"{spawnable.displayName} - {dropZone.}";
                }*/
                
                DroneHUD.Instance.SetTask(taskText);
            }
        }
    }
}