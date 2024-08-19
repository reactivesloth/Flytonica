using UnityEngine;

namespace Code.Internal.SceneManagement
{
    [CreateAssetMenu(fileName = "Scenario", menuName = "Flytoncia/Scenario", order = 1)]
    public class ScenarioSettings : ScriptableObject
    {
        public new string name;
    }
}