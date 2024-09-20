using System.Collections.Generic;
using Code.Internal.Drone;
using JetBrains.Annotations;
using UnityEngine;

namespace Code.Internal.SceneManagement
{
    [CreateAssetMenu(fileName = "Scenario", menuName = "Flytoncia/Scenario", order = 1)]
    public class ScenarioSettings : ScriptableObject
    {
        public int id;
        public new string name;
        public string description;
        public ScenarioType scenarioType = ScenarioType.FreeFlight;
        public MapSettings[] availableMaps;

        [CanBeNull] public DroneSettings currentDrone;
        [CanBeNull] public DroneFlightSettings currentDroneMode;
        [CanBeNull] public MapSettings currentMap;
        [CanBeNull] public List<ScenarioSettings> nestedScenarios;
        [CanBeNull] public ScenarioSettings nextScenario;

        public static ScenarioSettings Create(int id, string name, string description, ScenarioType scenarioType, MapSettings mapSettings, DroneSettings drone, DroneFlightSettings mode)
        {
            var instance = CreateInstance<ScenarioSettings>();

            instance.id = id;
            instance.name = name;
            instance.description = description;
            instance.scenarioType = scenarioType;
            instance.currentDrone = drone;
            instance.currentMap = mapSettings;
            instance.currentDroneMode = mode;
            
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