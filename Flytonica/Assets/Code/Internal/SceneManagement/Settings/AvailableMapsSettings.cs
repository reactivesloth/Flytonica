using System.Collections.Generic;
using UnityEngine;

namespace Code.Internal.SceneManagement
{
    [CreateAssetMenu(fileName = "Maps", menuName = "Flytoncia/MapList", order = 1)]
    public class AvailableMapsSettings : ScriptableObject
    {
        public MapSettings[] maps;
    }
}