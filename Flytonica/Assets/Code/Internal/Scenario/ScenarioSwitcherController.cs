using System.Collections.Generic;
using System.Text;
using Code.Internal.API;
using Code.Internal.Network;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface;
using FishNet;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Internal.Scenario
{
    public class ScenarioSwitcherController : MonoBehaviour
    {
        public static ScenarioSwitcherController Instance { get; private set; }

        [SerializeField] private SceneLoadingSettings sceneSettings;

        private List<Dictionary<string, string>> _results = new();
        private int _currentStatus = 0;

        private void Awake()
        {
            Instance = this;
        }

        public void ResetResults()
        {
            _results = new List<Dictionary<string, string>>();
        }

        public void NextOrEnd(Dictionary<string, string> result, bool isFailed = false)
        {
            if(_currentStatus != 2)
                _currentStatus = isFailed ? 2 : 1;
            
            print(sceneSettings.currentScenario.name);
            print(sceneSettings.currentScenario.nextScenario?.name);
            
            if (sceneSettings.currentScenario.nextScenario)
                Next(result);
            else
                End(result);
        }

        private void Next(Dictionary<string, string> result)
        {
            // Показ окна
            /*Time.timeScale = 0f;
            PopupPanel.ConfigurePopup("Ваш результат: ", $"{BuildResultString(result)}", null, "Переиграть", Color.white, Color.black,
                Replay, null, "Продолжить", Color.green, Color.black, () => LoadNext(result));*/
            
            LoadNext(result);
        }

        private void LoadNext(Dictionary<string, string> result)
        {
            _results.Add(result);
            
            sceneSettings.currentScenario = sceneSettings.currentScenario.nextScenario;
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            GameSceneManager.Instance.LoadGlobalScene(sceneSettings.currentScenario.currentMap, OnSceneLoaded);
            
            Time.timeScale = 1f;
        }

        private void OnSceneLoaded()
        {
            FindAnyObjectByType<ScenarioInitializer>().Initialize(sceneSettings.currentScenario);
            var drone = InstanceFinder.NetworkManager.GetComponent<PlayersSpawner>().Spawn(
                InstanceFinder.ClientManager.Connection, sceneSettings.currentScenario.currentDrone,
                sceneSettings.currentScenario.currentDroneMode);
        }

        private void End(Dictionary<string, string> result)
        {
            /*PopupPanel.ConfigurePopup("Ваш результат: ", $"{BuildResultString(result)}", null, "Переиграть", Color.white, Color.black,
                Replay, null, "Отправить результат", Color.green, Color.black, () => EndTask(result));*/
            
            EndTask(result);
        }
        

        private void EndTask(Dictionary<string, string> lastResult)
        {
            Time.timeScale = 0f;
            _results.Add(lastResult);
            if(sceneSettings.isTask)
                SendData();
            ResetResults();
            EndSession();
        }
        
        public void EndSession () {
            Time.timeScale = 1f;
            if (InstanceFinder.ServerManager.Started)
                InstanceFinder.ServerManager.StopConnection(true);
            InstanceFinder.ClientManager.StopConnection();
            
            GameSceneManager.Instance.ToMenuSingle();
        }

        private void SendData()
        {
            var flatDictionary = new Dictionary<string, string>();

            foreach (var dict in _results)
            {
                foreach (var kvp in dict)
                {
                    if (!flatDictionary.ContainsKey(kvp.Key))
                    {
                        flatDictionary.Add(kvp.Key, kvp.Value);
                    }
                    else
                    {
                        // Обработка дубликатов ключей по необходимости
                        flatDictionary[kvp.Key] += ", " + kvp.Value;
                    }
                }
            }

            var json = BuildJsonString(flatDictionary);
            print(json);
            Send(json);
        }

        private void Send(string result)
        {
            var settingsFile = Encoding.UTF8.GetBytes(result);

            var form = new WWWForm();
            form.AddField("user_scenario_id", sceneSettings.taskId);
            form.AddField("device_uuid", "");
            form.AddField("status", _currentStatus);
            form.AddBinaryData("file", settingsFile, "Result.json");
            form.AddBinaryData("replay", settingsFile, "Result.json");
            
            HttpClient.PostFormData(LinkConstants.LogCreateUrl, form, Debug.Log, Debug.LogError);
        }

        #region JSON Generation
        
        private string BuildJsonString(Dictionary<string, string> flatDictionary)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            bool first = true;
            foreach (var kvp in flatDictionary)
            {
                if (!first)
                {
                    sb.Append(",");
                }
                sb.Append("\"");
                sb.Append(EscapeString(kvp.Key));
                sb.Append("\":\"");
                sb.Append(EscapeString(kvp.Value));
                sb.Append("\"");
                first = false;
            }
            sb.Append("}");
            return sb.ToString();
        }

        // Метод для экранирования специальных символов в JSON-строке
        private string EscapeString(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        if (c < 32 || c > 126)
                        {
                            sb.AppendFormat("\\u{0:X4}", (int)c);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            return sb.ToString();
        }
        
        #endregion

        private string BuildResultString(Dictionary<string, string> result)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in result)
            {
                sb.Append(kvp.Key);
                sb.Append(": ");
                sb.Append(kvp.Value);
                sb.Append("\n");
            }
            return sb.ToString();
        }

        private void Replay()
        {
            Time.timeScale = 1f;
            GameSceneManager.Instance.Replay();
        }
    }
}