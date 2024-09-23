using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Internal.SceneManagement
{
    [CreateAssetMenu(fileName = "Scenario List", menuName = "Flytoncia/Scenes/Available Scenario", order = 1)]
    public class AvailableScenariosSettings : ScriptableObject
    {
        public List<ScenarioSettings> scenarios;

        public ScenarioSettings Find(int scenarioId, SettingType settingType = SettingType.TaskScenario)
        {
            foreach (var scenario in scenarios)
            {
                var foundScenario = FindInScenario(scenario, scenarioId, settingType);
                if (foundScenario != null)
                {
                    return foundScenario;
                }
            }

            return null;
        }

        private ScenarioSettings FindInScenario(ScenarioSettings scenario, int scenarioId, SettingType settingType)
        {
            if (scenario.id == scenarioId && scenario.settingType == settingType)
            {
                return scenario;
            }

            if (scenario.nestedScenarios != null)
            {
                foreach (var nestedScenario in scenario.nestedScenarios)
                {
                    var found = FindInScenario(nestedScenario, scenarioId, settingType);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }
    }
}