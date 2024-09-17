using System.Collections.Generic;

namespace Code.Internal.API.Wrappers.ReceiveModels
{
    [System.Serializable]
    public class GroupData
    {
        public int id;
        public string name;
        public int owner_id;
        public int teacher_id;
        public string created_at;
        public int owner_id_1;
        public string owner_name;
        public string owner_login;
        public int owner_type;
        public int? owner_talant_id;
        public string owner_license_until;
        public string owner_created_at;
        public int teacher_id_1;
        public string teacher_name;
        public string teacher_login;
        public int teacher_type;
        public int? teacher_talant_id;
        public string teacher_license_until;
        public string teacher_created_at;

        public StudentsList members;
    }

    [System.Serializable]
    public class StudentsList
    {
        public List<StudentData> data;
        public int total_count;
    }

    [System.Serializable]
    public class StudentData
    {
        public int id;
        public int student_id;
        public int group_id;
        public string created_at;
        public int user_id;
        public string user_name;
        public string user_login;
        public int user_type;
        public int? user_talant_id;
        public string user_license_until;
        public string user_created_at;
    }
}