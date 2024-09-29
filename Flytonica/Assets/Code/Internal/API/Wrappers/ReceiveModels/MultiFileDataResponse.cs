using System.Collections.Generic;

namespace Code.Internal.API.Wrappers.ReceiveModels
{
    [System.Serializable]
    public class MultiFileDataResponse
    {
        public List<FileData> data;
        public int total_count;
    }
}
