using System.Collections.Generic;

namespace Code.Internal.API.Wrappers.ReceiveModels
{
    [System.Serializable]
    public class MultiLogDataResponse
    {
        public List<LogData> data;
        public int total_count;
    }
}
