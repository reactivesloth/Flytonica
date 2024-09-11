using System.Collections.Generic;

namespace Code.Internal.API.Wrappers.ReceiveModels
{
    [System.Serializable]
    public class MultiScenarioDataResponse
    {
        public List<ScenarioData> data;
        public int total_count;
    }
}
