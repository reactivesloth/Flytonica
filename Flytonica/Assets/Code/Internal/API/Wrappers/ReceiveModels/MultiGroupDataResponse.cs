using System.Collections.Generic;

namespace Code.Internal.API.Wrappers.ReceiveModels
{
    [System.Serializable]
    public class MultiGroupDataResponse
    {
        public List<GroupData> data;
        public int total_count;
    }

}