
using System;

namespace Code.Internal.API.Wrappers.ReceiveModels
{
    [System.Serializable]
    public class LogData
    {
        public int id;
        public string uuid;
        public int owner_id;
        public int user_scenario_id;
        public int file_id;
        public int replay_id;
        public string created_at;  // Дата создания записи

        // Информация о файле
        public int file_file_size;
        public string file_file_path;
        public string file_file_id;
        public string file_file_unique_id;
        public int file_owner_id;

        // Информация о пользователе
        public int user_id;
        public string user_name;
        public string user_login;
        public int user_type;
        public int? user_talant_id;
        public string? user_license_until;
        public string user_created_at;  // Дата создания пользователя

        // Информация о сценарии пользователя
        public int user_scenario_id_1;
        public int user_scenario_scenario_id;
        public int user_scenario_user_id;
        public int user_scenario_status;
        public string user_scenario_created_at;  // Дата создания сценария пользователя

        // Информация о реплее
        public int replay_file_size;
        public string replay_file_path;
        public string replay_file_id;
        public string replay_file_unique_id;
        public int replay_owner_id;

        // Информация о сценарии
        public int scenario_id;
        public string scenario_name;
        public int scenario_owner_id;
        public int scenario_teacher_id;
        public string scenario_created_at;  // Дата создания сценария
    }

}
