using System.Collections.Generic;

namespace Code.Internal.API.Wrappers.ReceiveModels
{
    [System.Serializable]
    public class MultiDeviceDataResponse
    {
        public List<DeviceData> data;
        public int total_count;
    }
}
