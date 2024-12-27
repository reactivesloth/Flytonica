namespace Code.Internal.API.Wrappers.ReceiveModels
{
    [System.Serializable]
    public class AssignedScenarioData
    {
        public int id;
        public int scenario_id;
        public int user_id;
        public int status;
        public string created_at;
        public string scenario_name;
        public int scenario_owner_id;
        public string scenario_created_at;
        public string user_name;
        public string user_login;
        public string user_hash_password;
        public UserType user_type;
        public int user_talant_id;
        public string user_license_until;
        public string user_created_at;
        public int user_owner_id;
    }
}
