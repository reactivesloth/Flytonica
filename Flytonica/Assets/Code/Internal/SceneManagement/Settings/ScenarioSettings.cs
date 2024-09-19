using Code.Internal.Drone;
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

        [CanBeNull] public DroneSettings currentDrone;
        [CanBeNull] public MapSettings currentMap;
        [CanBeNull] public ScenarioSettings[] nestedScenarios;
        [CanBeNull] public ScenarioSettings nextScenario;

        public static ScenarioSettings Create(string name, string description, ScenarioType scenarioType, MapSettings mapSettings, DroneSettings drone)
        {
            var instance = CreateInstance<ScenarioSettings>();
            
            instance.name = name;
            instance.description = description;
            instance.scenarioType = scenarioType;
            instance.currentMap = mapSettings;
            instance.currentDrone = drone;
            
            return instance;
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
    
    public static class ScenarioTypeExtension
    {
        public static string GetName(this ScenarioType type)
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
}