using System.Collections;
using Code.Internal.Drone;
using UnityEngine;

namespace Code.Internal.Scenario.Tutorial
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
            yield return StartCoroutine(PlayReqlique(welcomeText));

            // Запуск двигателя
            yield return StartCoroutine(PlayReqlique(engineInit));
            yield return new WaitUntil(() => DroneController.Instance.EnginesEnabled);

            // Набор высоты
            yield return StartCoroutine(PlayReqlique(takeOff));
            yield return new WaitUntil(() => DroneController.Instance.DroneSensors.GetHeightFromFloor() > 1);
            
            // Движение вперед
            yield return StartCoroutine(PlayReqlique(moveForward));
            yield return new WaitUntil(() => DroneController.Instance.Pitch > 0.1f);
            yield return new WaitForSeconds(2);
            
            // Движение назад
            yield return StartCoroutine(PlayReqlique(moveBackward));
            yield return new WaitUntil(() => DroneController.Instance.Pitch < -0.1f);
            yield return new WaitForSeconds(2);
            
            // Движение влево
            yield return StartCoroutine(PlayReqlique(moveLeft));
            yield return new WaitUntil( () => DroneController.Instance.Roll < 0.1f);
            yield return new WaitForSeconds(2);
            
            // Движение вправо
            yield return StartCoroutine(PlayReqlique(moveRight));
            yield return new WaitUntil( () => DroneController.Instance.Roll > 0.1f);
            yield return new WaitForSeconds(2);
            
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