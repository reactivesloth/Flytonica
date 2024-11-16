using System.Collections.Generic;
using System.Text;
using Code.Internal.API;
using Code.Internal.Network;
using Code.Internal.Replays;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface;
using FishNet;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Internal.Scenario
{
    public class ScenarioSwitcherController : MonoBehaviour
    {
        public static ScenarioSwitcherController Instance { get; private set; }

        [SerializeField] private SceneLoadingSettings sceneSettings;

        private int _currentStatus = 0;

        public float ScoreSum;
        public bool IsNet => sceneSettings.isNet;  

        private void Awake()
        {
            Instance = this;
        }

        
        public void FailTask()
        {
            _currentStatus = 2;
            End();
        }
        
        public void StartTask()
        {
            if (!sceneSettings.isTask) return;
            ReplayController.Instance.StartRecording(sceneSettings.taskId);
            ReportBuilder.Instance.Clear();
            ScoreSum = 0;
            
            ReportBuilder.Instance.AddParameter("Количество участников", "1", false);
        }

        public void NextOrEnd(bool isFailed = false)
        {
            if(_currentStatus != 2)
                _currentStatus = isFailed ? 2 : 1;

            print(sceneSettings.currentScenario.name);
            print(sceneSettings.currentScenario.nextScenario?.name);

            if (sceneSettings.currentScenario.nextScenario)
                Next();
            else
                End();
        }

        private void Next()
        {
            // Показ окна
            /*Time.timeScale = 0f;
            PopupPanel.ConfigurePopup("Ваш результат: ", $"{BuildResultString(result)}", null, "Переиграть", Color.white, Color.black,
                Replay, null, "Продолжить", Color.green, Color.black, () => LoadNext(result));*/

            print("Next");
            ReportBuilder.Instance.AddPrefix();
            LoadNext();
        }

        private async void LoadNext()
        {
            InstanceFinder.NetworkManager.GetComponent<PlayersSpawner>()
                .Despawn(InstanceFinder.ClientManager.Connection);
            sceneSettings.currentScenario = sceneSettings.currentScenario.nextScenario;
            await SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            GameSceneManager.Instance.LoadGlobalScene(sceneSettings.currentScenario.currentMap, OnSceneLoaded);

            Time.timeScale = 1f;
        }

        private void OnSceneLoaded()
        {
            FindAnyObjectByType<ScenarioInitializer>().Initialize(sceneSettings.currentScenario);
            InstanceFinder.NetworkManager.GetComponent<PlayersSpawner>().Spawn(
                InstanceFinder.ClientManager.Connection, sceneSettings.currentScenario.currentDrone);
        }

        private void End()
        {
            /*PopupPanel.ConfigurePopup("Ваш результат: ", $"{BuildResultString(result)}", null, "Переиграть", Color.white, Color.black,
                Replay, null, "Отправить результат", Color.green, Color.black, () => EndTask(result));*/

            float totalScore = 0;
            if(sceneSettings.isTask)
                totalScore = ScoreSum / sceneSettings.currentScenarioCollection.nestedScenarios.Count;
            
            ReportBuilder.Instance.AddParameter("Общая оценка задания", $"{totalScore:F0}%", false);;
            
            print("End");
            EndTask();
        }


        private void EndTask()
        {
            Time.timeScale = 0.01f;
            var replay = ReplayController.Instance.StopRecording();
            if (sceneSettings.isTask)
                SendData(replay);
            EndSession();
        }

        public void EndSession()
        {
            Time.timeScale = 1f;
            if (InstanceFinder.ServerManager.Started)
                InstanceFinder.ServerManager.StopConnection(true);
            InstanceFinder.ClientManager.StopConnection();

            GameSceneManager.Instance.ToMenuSingle();
        }

        private void SendData(string replayPath)
        {
            Send(ReportBuilder.Instance.GenerateJsonReport(), replayPath);
        }

        private void Send(string result, string replayPath)
        {
            var settingsFile = Encoding.UTF8.GetBytes(result);

            if (!System.IO.File.Exists(replayPath))
            {
                Debug.LogError($"Файл реплея не найден по пути: {replayPath}");
                return;
            }


            var replayData = System.IO.File.ReadAllBytes(replayPath);

            var form = new WWWForm();
            form.AddField("user_scenario_id", sceneSettings.taskId);
            form.AddField("device_uuid", "");
            form.AddField("status", _currentStatus);
            form.AddBinaryData("file", settingsFile, "Result.json", "application/json");
            form.AddBinaryData("replay", replayData, "Replay.replay", "application/octet-stream");

            HttpClient.PostFormData(LinkConstants.LogCreateUrl, form, Debug.Log, (s, l) => Debug.LogError(s));
        }

        private float GetTotalScore()
        {
            float result = 0;

            return result;
        }
    }
}