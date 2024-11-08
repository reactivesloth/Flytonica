using System.Linq;
using Code.Internal.Network.Teacher;
using Code.Internal.Scenario;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Code.Internal.UserInterface.Elements
{
    public class Leaderboard : MonoBehaviour
    {
        private static Leaderboard _instance;
        public static Leaderboard Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<Leaderboard>(true);
                }
                return _instance;
            }
        }

        [SerializeField] private Button closeButton;
        [SerializeField] private Table table;
        private float updateInterval = 1f; // Интервал обновления в секундах
        private float timer; // Таймер для отслеживания времени

        private void Awake()
        {
            _instance = this;
            gameObject.SetActive(false);
        }

        public void Open(UnityAction closeAction = null)
        {
            print("Open leaderboard");
            gameObject.SetActive(true);
            if (closeAction != null)
                closeButton.onClick.AddListener(closeAction);
            closeButton.onClick.AddListener(Close);
            
            GenerateResultTable();
            timer = 0; // Сбросить таймер при открытии
        }

        private void Update()
        {
            if (gameObject.activeSelf) // Проверяем, что таблица открыта
            {
                timer += Time.deltaTime;
                if (timer >= updateInterval)
                {
                    GenerateResultTable(); // Обновляем таблицу
                    timer = 0; // Сбросить таймер
                }
            }
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void GenerateResultTable()
        {
            // Сортируем результаты
            var sortedResults = UsersManager.Instance.LeaderboardResults
                .OrderByDescending(result => result.IsFinished)  // Сначала финишировавшие пользователи
                .ThenByDescending(result => result.Score)        // Затем по убыванию очков
                .ThenBy(result => result.Time)                   // И по возрастанию времени
                .ToList();

            table.Clear();
            
            foreach (var result in sortedResults)
            {
                table.AddRow(result.PlayerName, result.Score.ToString(), ScenarioBase.GetTime(result.Time));
            }
        }
        
        private void OnDisable()
        {
            table.Clear();
            closeButton.onClick.RemoveAllListeners();
        }
    }
}
