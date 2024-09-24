using System.Collections.Generic;
using System.Text;
using Code.Internal.Network;
using Code.Internal.SceneManagement;
using Code.Internal.UserInterface;
using FishNet;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Internal.Scenario
{
    public class ScenarioSwitcherController : MonoBehaviour
    {
        public static ScenarioSwitcherController Instance { get; private set; }

        [SerializeField] private SceneLoadingSettings sceneSettings;

        private List<Dictionary<string, string>> _results = new();

        private void Awake()
        {
            Instance = this;
        }

        private void ResetResults()
        {
            _results = new List<Dictionary<string, string>>();
        }

        public void NextOrEnd(Dictionary<string, string> result)
        {
            if (sceneSettings.currentScenario.nextScenario)
                Next(result);
            else
                End(result);
        }

        private void Next(Dictionary<string, string> result)
        {
            // Показ окна
            
            Time.timeScale = 0f;
            PopupPanel.ConfigurePopup("Ваш результат: ", $"{BuildResultString(result)}", null, "Переиграть", Color.white, Color.black,
                Replay, null, "Продолжить", Color.green, Color.black, () => LoadNext(result));
        }

        private void LoadNext(Dictionary<string, string> result)
        {
            Time.timeScale = 1f;
            _results.Add(result);
            
            sceneSettings.currentScenario = sceneSettings.currentScenario.nextScenario;
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            InstanceFinder.NetworkManager.GetComponent<PlayersSpawner>().DespawnAll();
            GameSceneManager.Instance.LoadGlobalScene(sceneSettings.currentScenario.currentMap, OnSceneLoaded);
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
            PopupPanel.ConfigurePopup("Ваш результат: ", $"{BuildResultString(result)}", null, "Переиграть", Color.white, Color.black,
                Replay, null, "Отправить результат", Color.green, Color.black, () => EndTask(result));
        }

        private void EndTask(Dictionary<string, string> lastResult)
        {
            Time.timeScale = 0f;
            _results.Add(lastResult);
            SendData();
            EndSession();
        }
        
        public void EndSession () {
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

            string json = BuildJsonString(flatDictionary);
            
            print(json);
            
            //TODO: запрос на отправку
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