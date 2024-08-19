using UnityEditor;
using UnityEngine;

namespace Code.Internal.SceneManagement
{
    [CreateAssetMenu(fileName = "Maps", menuName = "Flytoncia/Map", order = 1)]
    public class MapSettings : ScriptableObject
    {
        public SceneAsset scene;
        public new string name;
        public ScenarioSettings[] mapScenarios;
    }
}