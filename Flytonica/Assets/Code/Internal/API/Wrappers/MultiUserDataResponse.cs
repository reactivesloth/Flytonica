using System.Collections.Generic;

namespace Code.Internal.API.Wrappers
{
    [System.Serializable]
    public class MultiUserDataResponse
    {
        public List<UserData> data;
        public int total_count;
    }
}
