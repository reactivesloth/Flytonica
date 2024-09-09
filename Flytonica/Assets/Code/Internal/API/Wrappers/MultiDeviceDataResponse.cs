using System.Collections.Generic;

namespace Code.Internal.API.Wrappers
{
    [System.Serializable]
    public class MultiDeviceDataResponse
    {
        public List<DeviceData> data;
        public int total_count;
    }
}
