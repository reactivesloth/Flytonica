namespace Code.Internal.API.Wrappers.ReceiveModels
{
    [System.Serializable]
    public class UserData
    {
        public int id;
        public string name;
        public string login;
        public UserType type;
        public int? talant_id;
        public string license_until;
        public string created_at;
        public int? owner_id;
        public string owner_name;
        public string owner_login;
        public UserType? owner_type;
        public int? owner_talant_id;
        public string owner_license_until;
        public string owner_created_at;
    }
}
