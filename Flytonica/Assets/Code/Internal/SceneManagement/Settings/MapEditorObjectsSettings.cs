using UnityEngine;

namespace Code.Internal.SceneManagement
{
    [CreateAssetMenu(fileName = "Map Editor Objects", menuName = "Flytoncia/Map Editor", order = 1)]
    public class MapEditorObjectsSettings : ScriptableObject
    {
        [Header("Objects for all scenarios")]
        public GameObject[] genericObjects;
        
        [Header("Objects for racing scenarios")]
        public GameObject gatesStartObject;
        public GameObject gatesFinishObject;
        public GameObject[] gates;
        
        [Header("Objects for searching scenarios")]
        public GameObject[] searchingObjects;
    }
}