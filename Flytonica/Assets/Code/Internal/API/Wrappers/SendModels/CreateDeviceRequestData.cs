
namespace Code.Internal.API.Wrappers.SendModels
{
    [System.Serializable]
    public class CreateDeviceRequestData
    {
        public string uuid;

        public CreateDeviceRequestData(string uuid)
        {
            this.uuid = uuid;
        }
    }
}
