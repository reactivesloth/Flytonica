using System.Collections;
using Code.Internal.Drone;
using UnityEngine;

namespace Code.Internal.Scenario.Tutorial._1_Base_input
{
    public class ScenarioTutorial_13_PithAndRoll : ScenarioTutorialRoutine
    {
        [SerializeField] private ScenarioTutorialReplique welcomeText; // Вступление
        [SerializeField] private ScenarioTutorialReplique engineInit; // Запуск двигателя квадрокоптера
        [SerializeField] private ScenarioTutorialReplique takeOff; // Поднятие дрона в воздух
        [SerializeField] private ScenarioTutorialReplique moveForward; // Движение вперед
        [SerializeField] private ScenarioTutorialReplique moveBackward; // Движение назад
        [SerializeField] private ScenarioTutorialReplique moveLeft; // Движение влево
        [SerializeField] private ScenarioTutorialReplique moveRight; // Движение вправо
        [SerializeField] private ScenarioTutorialReplique landing; // Приземление
        [SerializeField] private ScenarioTutorialReplique engineOff; // Отключение двигателей квадрокоптера

        protected override IEnumerator RunRoutine()
        {
            // Вступление
            yield return StartCoroutine(PlayReplique(welcomeText));

            // Запуск двигателя
            yield return StartCoroutine(PlayReplique(engineInit));
            yield return new WaitUntil(() => DroneController.Instance.EnginesEnabled);

            // Набор высоты
            yield return StartCoroutine(PlayReplique(takeOff));
            yield return new WaitUntil(() => DroneController.Instance.DroneSensors.GetHeightFromFloor() > 1);
            
            // Движение вперед
            yield return StartCoroutine(PlayReplique(moveForward));
            yield return new WaitUntil(() => DroneController.Instance.Pitch > 0.1f);
            yield return new WaitForSeconds(2);
            
            // Движение назад
            yield return StartCoroutine(PlayReplique(moveBackward));
            yield return new WaitUntil(() => DroneController.Instance.Pitch < -0.1f);
            yield return new WaitForSeconds(2);
            
            // Движение влево
            yield return StartCoroutine(PlayReplique(moveLeft));
            yield return new WaitUntil( () => DroneController.Instance.Roll < 0.1f);
            yield return new WaitForSeconds(2);
            
            // Движение вправо
            yield return StartCoroutine(PlayReplique(moveRight));
            yield return new WaitUntil( () => DroneController.Instance.Roll > 0.1f);
            yield return new WaitForSeconds(2);
            
            // Приземление
            yield return StartCoroutine(PlayReplique(landing));
            yield return new WaitUntil(() => 
                DroneController.Instance.GetComponent<Rigidbody>().linearVelocity.magnitude < 0.1f && DroneController.Instance.DroneSensors.GetHeightFromFloor() < 1);
            
            // Отключение двигателей
            yield return StartCoroutine(PlayReplique(engineOff));
            yield return new WaitUntil(() => DroneController.Instance.EnginesEnabled == false);

            Finish();
            StopAllCoroutines();
        }
    }
}