using System.Collections.Generic;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface;
using UnityEngine;

namespace Code.Internal.Scenario.Transport
{
    public class ScenarioTransport: ScenarioBase
    {
        private List<TakeZone> _takeZones;
        private List<DropZone> _dropZones;
        private List<CargoObject> _cargos;

        private int deliveredCargoCount;

        public override void Initialize(ScenarioSettings scenario)
        {
            base.Initialize(scenario);
            
            var objects = GetComponentsInChildren<SpawnableObject>();
            
            _takeZones = new List<TakeZone>();
            _dropZones = new List<DropZone>();
            _cargos = new List<CargoObject>();

            foreach (var spawnableObject in objects)
            {
                if (spawnableObject.TryGetComponent(out TakeZone takeZone))
                {
                    _takeZones.Add(takeZone);
                    var cargo = takeZone.SpawnCargo();
                    _cargos.Add(cargo);
                    cargo.Delivered += OnCargoDelivery;
                }
                else if(spawnableObject.TryGetComponent(out DropZone dropZone))
                {
                    _dropZones.Add(dropZone);
                }
            }
            
            if(ScenarioCondition == ScenarioCondition.Waiting)
                StartRace();
        }

        protected override void StartRace()
        {
            base.StartRace();
            deliveredCargoCount = 0;
            
            DroneHUD.Instance.SetTask($"Доставьте посылки {deliveredCargoCount}/{_cargos.Count}");
        }

        protected override void FinishRace(bool success = true)
        {
            base.FinishRace(success);

            PopupPanel.ConfigurePopup("Задание выполнено!",
                $"Подздравляем! Время выполнения: {GetTimeWithMs(TotalTime)}",
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
            //TODO: Посчитать штрафы сценария
            
            base.AddStatistic();
            
            var resultBuilder = ReportBuilder.Instance;
            
            resultBuilder.AddParameter($"Количество доставленых предметов", $"{deliveredCargoCount}/{_cargos.Count}");
            //TODO: добавить поля отчёта
        }

        private void OnCargoDelivery(CargoObject cargo)
        {
            deliveredCargoCount++;

            if (deliveredCargoCount >= _cargos.Count)
            {
                DroneHUD.Instance.SetTask($"Вы доставили все предметы");
                FinishRace();
            }
            else
            {
                DroneHUD.Instance.SetTask($"Доставьте предметы {deliveredCargoCount}/{_cargos.Count}");
            }
            
        }
    }
}