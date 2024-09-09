
namespace Code.Internal.API.Wrappers
{
    [System.Serializable]
    public class CreateLogRequestData
    {
        public int user_scenario_id;
        public string device_uuid;
        public int status;
        public string file;  // Assumed to be a file path or a base64 string
        public string replay;  // Assumed to be a file path or a base64 string
    }
}
