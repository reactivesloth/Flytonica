using UnityEngine;

namespace Code.Internal.SceneManagement
{
    [CreateAssetMenu(fileName = "Map Editor Objects", menuName = "Flytoncia/Map Editor", order = 1)]
    public class MapEditorObjectsSettings : ScriptableObject
    {
        public GameObject[] Objects;
    }
}