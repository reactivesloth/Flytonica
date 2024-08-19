using JetBrains.Annotations;
using UnityEngine;

namespace Code.Internal.SceneManagement
{
    [CreateAssetMenu(fileName = "Scenario", menuName = "Flytoncia/Scenario", order = 1)]
    public class ScenarioSettings : ScriptableObject
    {
        public new string name;
        public string description;
        public ScenarioType scenarioType = ScenarioType.FreeFlight;
        public MapSettings[] availableMaps;

        [CanBeNull] public ScenarioSettings[] nestedScenarios;
        [CanBeNull] public ScenarioSettings nextScenario;
        
        public string GetScenarioTypeName(ScenarioType type)
        {
            return type switch
            {
                ScenarioType.FreeFlight => $"Свободный полет",
                ScenarioType.Tutorial => $"Обучение",
                ScenarioType.Race => $"Гонка",
                ScenarioType.Transport => $"Перевозка грузов",
                ScenarioType.Searching => $"Поиск с помощью камеры",
                ScenarioType.SearchingWithIR => $"Поиск с тепловизором",
                _ => $"Пользовательский сценарий"
            };
        }
    }

    public enum ScenarioType
    {
        FreeFlight,
        Tutorial,
        Race,
        Transport,
        Searching,
        SearchingWithIR
    }
}