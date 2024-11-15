using System.Collections;
using Code.Internal.Drone;
using UnityEngine;

namespace Code.Internal.Scenario.Tutorial
{
    public class ScenarioTutorial_12_MoveUpHigher : ScenarioTutorialRoutine
    {
        [SerializeField] private ScenarioTutorialReplique welcomeText; // Вступление
        [SerializeField] private ScenarioTutorialReplique engineInit; // Запуск двигателя квадрокоптера
        [SerializeField] private ScenarioTutorialReplique takeOff2m; // Поднятие дрона в воздух на 1-2 метра
        [SerializeField] private ScenarioTutorialReplique notRushingRecommendation; // Не торопитесь, выполняйте все действия плавно
        [SerializeField] private ScenarioTutorialReplique takeOff5m; // Поднятие дрона в воздух на 5+ метров
        [SerializeField] private ScenarioTutorialReplique windRecommendation; // Учитвайте, что на высоте на дрон влияет ветер
        [SerializeField] private ScenarioTutorialReplique landing; // Приземление
        [SerializeField] private ScenarioTutorialReplique engineOff; // Отключение двигателей квадрокоптера

        protected override IEnumerator RunRoutine()
        {
            // Вступление
            yield return StartCoroutine(PlayReqlique(welcomeText));

            // Запуск двигателя
            yield return StartCoroutine(PlayReqlique(engineInit));
            yield return new WaitUntil(() => DroneController.Instance.EnginesEnabled);

            // Поднятие дрона в воздух на 1-2 метра
            yield return StartCoroutine(PlayReqlique(takeOff2m));
            yield return new WaitUntil(() => DroneController.Instance.DroneSensors.GetHeightFromFloor() > 1 && DroneController.Instance.DroneSensors.GetHeightFromFloor() < 2);
            
            // Не торопитесь, выполняйте все действия плавно
            yield return StartCoroutine(PlayReqlique(notRushingRecommendation));
            
            // Поднятие дрона в воздух на 5 метров
            yield return StartCoroutine(PlayReqlique(takeOff5m));
            yield return new WaitUntil(() => DroneController.Instance.DroneSensors.GetHeightFromFloor() > 5);

            // Учитвайте, что на высоте на дрон влияет ветер
            yield return StartCoroutine(PlayReqlique(windRecommendation));
            
            // Приземление
            yield return StartCoroutine(PlayReqlique(landing));
            yield return new WaitUntil(() => 
                DroneController.Instance.GetComponent<Rigidbody>().linearVelocity.magnitude < 0.1f && DroneController.Instance.DroneSensors.GetHeightFromFloor() < 1);
            
            // Отключение двигателей
            yield return StartCoroutine(PlayReqlique(engineOff));
            yield return new WaitUntil(() => DroneController.Instance.EnginesEnabled == false);

            Finish();
            StopAllCoroutines();
        }
    }
}