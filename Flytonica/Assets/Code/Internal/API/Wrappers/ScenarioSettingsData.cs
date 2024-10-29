using System.Collections.Generic;
using Code.Internal.SceneManagement;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Internal.API.Wrappers
{
    [System.Serializable]
    public class ScenarioSettingsData
    {
        public string name;
        public string description;

        public int droneId;
        public int mapId;
        public int droneModeId;
        public ScenarioType typeId;
        public bool cameraThirdPerson;
        public bool cameraAllowedSwitchModeId;
        public List<WindLayerSettings> windLayers;
        public List<SpawnedObject> objects;

        public ScenarioSettingsData(string name, string description, int droneId, int mapId, ScenarioType typeId,
            int droneModeId, bool cameraThirdPerson, bool cameraAllowedSwitchModeId,
            List<WindLayerSettings> windLayers = null, List<SpawnedObject> objects = null)
        {
            this.droneId = droneId;
            this.mapId = mapId;
            this.typeId = typeId;
            this.droneModeId = droneModeId;
            this.description = description;
            this.name = name;
            this.cameraThirdPerson = cameraThirdPerson;
            this.cameraAllowedSwitchModeId = cameraAllowedSwitchModeId;
            this.windLayers = windLayers;
            this.objects = objects ?? new List<SpawnedObject>();
        }
    }

    [System.Serializable]
    public class SpawnedObject
    {
        public string prefabName;

        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public int sortOrder;

        public SpawnedObject(string prefabName, Vector3 position, Quaternion rotation, Vector3 scale, int sortOrder = 0)
        {
            this.prefabName = prefabName;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.sortOrder = sortOrder;
        }
    }

    [System.Serializable]
    public class WindLayerSettings
    {
        public const float MaxForce = 37;

        public int windForceId; // ID силы ветра (0-12 из таблицы)
        public int windDirectionId; // ID направления ветра (0-7, 0 - север, 1 - северо-восток и т.д.)

        public WindLayerSettings(int windForceId, int windDirectionId)
        {
            this.windForceId = windForceId;
            this.windDirectionId = windDirectionId;
        }

        public (float minForce, float maxForce) GetWindSpeeds()
        {
            float[] windForces =
                { 0.0f, 0.3f, 1.6f, 3.4f, 5.5f, 8.0f, 10.8f, 13.9f, 17.2f, 20.8f, 24.5f, 28.5f, 32.6f };

            if (windForceId >= 0 && windForceId < windForces.Length)
            {
                var minForce = windForces[windForceId];
                var maxForce = windForceId + 1 < windForces.Length ? windForces[windForceId + 1] : float.MaxValue;
                return (minForce, maxForce);
            }
            else
            {
                Debug.LogError("Invalid windForceId");
                return (0f, 0f); // Возвращаем 0 как минимальное и максимальное значение в случае ошибки
            }
        }

        public (Vector3 direction, string directionName) GetWindDirection()
        {
            Vector3[] windDirections =
            {
                new(0, 0, 1), // Север (0°)
                new(1, 0, 1), // Северо-восток (45°)
                new(1, 0, 0), // Восток (90°)
                new(1, 0, -1), // Юго-восток (135°)
                new(0, 0, -1), // Юг (180°)
                new(-1, 0, -1), // Юго-запад (225°)
                new(-1, 0, 0), // Запад (270°)
                new(-1, 0, 1) // Северо-запад (315°)
            };

            string[] directionNames =
            {
                "С", // Север
                "СВ", // Северо-восток
                "В", // Восток
                "ЮВ", // Юго-восток
                "Ю", // Юг
                "ЮЗ", // Юго-запад
                "З", // Запад
                "СЗ" // Северо-запад
            };

            if (windDirectionId >= 0 && windDirectionId < windDirections.Length)
            {
                return (windDirections[windDirectionId].normalized, directionNames[windDirectionId]);
            }

            Debug.LogError("Invalid windDirectionId");
            return (Vector3.zero, "Invalid");
        }
    }
}