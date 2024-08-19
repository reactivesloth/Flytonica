using UnityEngine;

namespace Code.Internal.SceneManagement
{
    [CreateAssetMenu(fileName = "Maps List", menuName = "Flytoncia/Scenes/Available Maps", order = 1)]
    public class AvailableMapsSettings : ScriptableObject
    {
        public MapSettings[] maps;
    }
}