using System.Collections.Generic;
using Code.Internal.API.Wrappers;
using Code.Internal.Drone;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Internal.SceneManagement
{
    [CreateAssetMenu(fileName = "Scenario", menuName = "Flytoncia/Scenario", order = 1)]
    public class ScenarioSettings : ScriptableObject
    {
        public SettingType settingType;
        [HideInInspector] public int id;
        public new string name;
        [TextArea(3,5)]public string description;
        public ScenarioType scenarioType = ScenarioType.FreeFlight;
        public MapSettings[] availableMaps;

        [CanBeNull] public DroneSettings currentDrone;
        [CanBeNull] public DroneFlightSettings currentDroneMode;
        [CanBeNull] public MapSettings currentMap;

        [CanBeNull] public List<ScenarioSettings> nestedScenarios;
        [CanBeNull] public ScenarioSettings nextScenario;

        public bool cameraThirdPerson = false;
        public bool cameraSwitchAllowed = true;
        public List<SpawnedObject> objects;
        public List<WindLayerSettings> windSettings;

        public static ScenarioSettings CreateDynamicTaskScenario(int id, string name, string description,
            ScenarioType scenarioType,
            MapSettings mapSettings, DroneSettings drone, DroneFlightSettings mode, bool cameraThirdPerson,
            bool cameraSwitchAllowed, List<SpawnedObject> objects = null, List<WindLayerSettings> windSettings = null)
        {
            var instance = CreateInstance<ScenarioSettings>();

            instance.settingType = SettingType.TaskScenario;
            instance.id = id;
            instance.name = name;
            instance.description = description;
            instance.scenarioType = scenarioType;
            instance.currentDrone = drone;
            instance.currentMap = mapSettings;
            instance.currentDroneMode = mode;
            instance.cameraThirdPerson = cameraThirdPerson;
            instance.cameraSwitchAllowed = cameraSwitchAllowed;
            instance.objects = objects;
            instance.windSettings = windSettings;
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

    public enum SettingType
    {
        Scenario,
        TaskScenario,
        Task,
        List
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