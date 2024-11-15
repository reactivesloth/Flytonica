using System;
using System.Collections;
using Code.Internal.Drone;
using Code.Internal.Scenario.Race;
using Code.Internal.UserInterface;
using UnityEngine;

namespace Code.Internal.Scenario.Tutorial
{
    public class ScenarioTutorial_15_FPV : ScenarioTutorialRoutine
    {
        [SerializeField] private ScenarioTutorialReplique welcomeText; // Вступление
        [SerializeField] private ScenarioTutorialReplique fpvHintText; // Туториал по FPV
        [SerializeField] private ScenarioTutorialReplique engineInit; // Запуск двигателя квадрокоптера
        [SerializeField] private ScenarioTutorialReplique changeToFPV; // Смена режима на FPV
        [SerializeField] private ScenarioTutorialReplique takeOff; // Поднятие дрона в воздух на 3 метра
        [SerializeField] private ScenarioTutorialReplique holdHeight; // Удерживайте позицию в точке
        //[SerializeField] private ScenarioTutorialReplique moveBetweenPoints; // Движение между точками
        [SerializeField] private ScenarioTutorialReplique landing; // Приземление
        [SerializeField] private ScenarioTutorialReplique engineOff; // Отключение двигателей квадрокоптера

        [SerializeField] private ScenarioTutorialReplique mistakeReturn; // Вы не можете удержать высоту, попробуйте еще раз сначала

        [SerializeField] private string checkpointHoldHeightName;
        private Transform checkpointHoldHeight;
        private bool checkpointHoldHeightReached;
        private float checkpointHoldHeightTimer;

        protected override IEnumerator RunRoutine()
        {
            // Вступление
            yield return StartCoroutine(PlayReqlique(welcomeText));

            // Вступление
            yield return StartCoroutine(PlayReqlique(fpvHintText));

            // Запуск двигателя
            yield return StartCoroutine(PlayReqlique(engineInit));
            yield return new WaitUntil(() => DroneController.Instance.EnginesEnabled);
            
            // Смена на FPV
            yield return StartCoroutine(PlayReqlique(changeToFPV));
            yield return new WaitUntil(() => DroneInput.Instance.DroneCam);

            // Набор высоты
            yield return StartCoroutine(PlayReqlique(takeOff));
            yield return new WaitUntil(() => DroneController.Instance.DroneSensors.GetHeightFromFloor() > 3);
            
            // Удержание в точке 5 секунд
            ShowCheckpointHoldHeight(true);
            yield return StartCoroutine(PlayReqlique(holdHeight));
            yield return new WaitUntil(() => checkpointHoldHeightReached);
            ShowCheckpointHoldHeight(false);
            
            // Движение по точкам с удержанием высоты
            //yield return StartCoroutine(PlayReqlique(moveBetweenPoints));
            //yield return new WaitUntil(() => allCheckpointsReached == true);
            
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

        protected override void Update()
        {
            base.Update();

            if (checkpointHoldHeight == null)
            {
                checkpointHoldHeight = GameObject.Find(checkpointHoldHeightName).transform;
                if (checkpointHoldHeight != null)
                {
                    ShowCheckpointHoldHeight(false);
                    Checkpoint cp = checkpointHoldHeight.GetComponentInChildren<Checkpoint>();
                    cp.SetEndColor(true);
                }
            }
            else
            {
                if (checkpointHoldHeightReached || !checkpointHoldHeight.gameObject.activeSelf) return;
                if (Vector3.Distance(DroneController.Instance.transform.position, checkpointHoldHeight.position) < 1f)
                {
                    checkpointHoldHeightTimer += Time.deltaTime;
                    if (checkpointHoldHeightTimer >= 5)
                    {
                        checkpointHoldHeightReached = true;
                    }
                }
                else
                {
                    if (checkpointHoldHeightTimer != 0)
                    {
                        StartCoroutine(PlayReqlique(mistakeReturn));
                    }

                    checkpointHoldHeightTimer = 0;
                }
                DroneHUD.Instance.SetTime(ScenarioBase.GetTimeWithMs(5 - checkpointHoldHeightTimer));
            }
        }

        private void ShowCheckpointHoldHeight(bool value)
        {
            checkpointHoldHeight.gameObject.SetActive(value);
        }
    }
}