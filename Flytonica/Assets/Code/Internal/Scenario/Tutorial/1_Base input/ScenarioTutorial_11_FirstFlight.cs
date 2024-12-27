using System.Collections;
using Code.Internal.Drone;
using UnityEngine;

namespace Code.Internal.Scenario.Tutorial._1_Base_input
{
    public class ScenarioTutorial_11_FirstFlight : ScenarioTutorialRoutine
    {
        [SerializeField] private ScenarioTutorialReplique welcomeText; // Вступление
        [SerializeField] private ScenarioTutorialReplique engineInit; // Запуск двигателя квадрокоптера
        [SerializeField] private ScenarioTutorialReplique takeOff; // Поднятие дрона в воздух
        [SerializeField] private ScenarioTutorialReplique rotatingAround; // Вращение вокруг своей оси
        [SerializeField] private ScenarioTutorialReplique landing; // Приземление
        [SerializeField] private ScenarioTutorialReplique engineOff; // Отключение двигателей квадрокоптера
        
        protected override IEnumerator RunRoutine()
        {
            // Вступление
            yield return StartCoroutine(PlayReplique(welcomeText));
            
            // Запуск двигателя
            yield return StartCoroutine(PlayReplique(engineInit));
            yield return new WaitUntil(() => DroneController.Instance.EnginesEnabled);

            // Поднятие дрона в воздух
            yield return StartCoroutine(PlayReplique(takeOff));
            yield return new WaitUntil(() => DroneController.Instance.GetComponent<Rigidbody>().linearVelocity.y > 1);
            
            // Вращение дрона в воздухе
            yield return StartCoroutine(PlayReplique(rotatingAround));
            yield return new WaitUntil(()=> Mathf.Abs(DroneInput.Instance.Yaw) > 0.2f);
            
            // Приземление
            yield return StartCoroutine(PlayReplique(landing));
            yield return new WaitUntil(()=> DroneController.Instance.GetComponent<Rigidbody>().linearVelocity.magnitude < 0.1f && DroneController.Instance.DroneSensors.GetHeightFromFloor() < 1);
            
            // Отключение двигателей
            yield return StartCoroutine(PlayReplique(engineOff));
            yield return new WaitUntil(() => DroneController.Instance.EnginesEnabled == false);
            
            Finish();
            StopAllCoroutines();
        }
    }
}