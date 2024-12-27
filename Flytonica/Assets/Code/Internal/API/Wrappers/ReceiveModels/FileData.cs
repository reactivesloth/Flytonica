
namespace Code.Internal.API.Wrappers.ReceiveModels
{
    [System.Serializable]
    public class FileData
    {
        public int id;
        public int file_size;
        public string file_path;
        public string file_id;
        public string file_unique_id;
        public int owner_id;
        public string created_at;
    }
}
