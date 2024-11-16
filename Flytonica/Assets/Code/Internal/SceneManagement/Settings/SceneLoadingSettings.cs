using System.Collections.Generic;
using Code.Internal.Drone;
using UnityEngine;

namespace Code.Internal.SceneManagement
{
    [CreateAssetMenu(fileName = "Scene Settings", menuName = "Flytoncia/Scene Settings", order = 1)]
    public class SceneLoadingSettings : ScriptableObject
    {
        public bool isTask;
        public bool isNet;
        public int taskId = -1;
        public MapSettings currentMap;
        public ScenarioSettings currentScenarioCollection;
        public ScenarioSettings currentScenario;
    }
}