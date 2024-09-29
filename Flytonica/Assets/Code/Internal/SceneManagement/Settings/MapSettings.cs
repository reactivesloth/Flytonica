using UnityEngine;

namespace Code.Internal.SceneManagement
{
    [CreateAssetMenu(fileName = "Maps", menuName = "Flytoncia/Map", order = 1)]
    public class MapSettings : ScriptableObject
    {
        public new string name;
        public string loadingSceneName;
        public int windLayersCount;
        public int maxAllowedHeight = 0;
    }
}