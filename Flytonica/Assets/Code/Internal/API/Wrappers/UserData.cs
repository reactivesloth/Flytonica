using System;

namespace Code.Internal.API.Wrappers
{
    [Serializable]
    public class UserData
    {
        public int id;
        public string name;
        public string login;
        public int type;
        public int? talant_id;
        public DateTime? license_until;
        public DateTime created_at;
        public int? owner_id;
        public int? owner_id_1;
        public string owner_name;
        public string owner_login;
        public int? owner_type;
        public int? owner_talant_id;
        public DateTime? owner_license_until;
        public DateTime? owner_created_at;
    }
}