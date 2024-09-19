using System.Collections.Generic;
using Code.Internal.SceneManagement;
using UnityEngine;

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

        public List<WindLayerSettings> windLayers;
        
        public List<SpawnedObject> objects;

        public ScenarioSettingsData(string name, string description, int droneId, int mapId, ScenarioType typeId, int droneModeId, List<WindLayerSettings> windLayers = null, List<SpawnedObject> objects = null)
        {
            this.droneId = droneId;
            this.mapId = mapId;
            this.typeId = typeId;
            this.droneModeId = droneModeId;
            this.description = description;
            this.name = name;
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

        public SpawnedObject(string prefabName, Vector3 position, Quaternion rotation)
        {
            this.prefabName = prefabName;
            this.position = position;
            this.rotation = rotation;
        }
    }

    [System.Serializable]
    public class WindLayerSettings
    {
        public int windForceId;
        public int windDirectionId;

        public WindLayerSettings(int windForceId, int windDirectionId)
        {
            this.windForceId = windForceId;
            this.windDirectionId = windDirectionId;
        }
    }
}