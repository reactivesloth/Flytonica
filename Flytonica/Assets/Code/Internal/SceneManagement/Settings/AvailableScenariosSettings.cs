using System.Collections.Generic;
using UnityEngine;

namespace Code.Internal.SceneManagement
{
    [CreateAssetMenu(fileName = "Scenario List", menuName = "Flytoncia/Scenes/Available Scenario", order = 1)]
    public class AvailableScenariosSettings : ScriptableObject
    {
        public List<ScenarioSettings> scenarios;
    }
}