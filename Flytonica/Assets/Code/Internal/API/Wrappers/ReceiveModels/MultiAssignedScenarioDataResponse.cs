using System.Collections.Generic;

namespace Code.Internal.API.Wrappers.ReceiveModels
{
    [System.Serializable]
    public class MultiAssignedScenarioDataResponse
    {
        public List<AssignedScenarioData> data;
        public int total_count;
    }
}
