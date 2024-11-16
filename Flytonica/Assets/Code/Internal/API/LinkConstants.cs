using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace Code.Internal.API
{
    public static class LinkConstants
    {
        public const string ServerUrl = "https://dronesimapi.4app.pro", Version = "v1";

        // Path for user authentication
        // Sends: UserAuthRequestData (login, password)
        // Receives: AuthResponseData (access_token, refresh_token, id, type)
        private const string UserAuthPath = "users/auth";

        // Path for fetching current user information
        // Sends: None
        // Receives: UserData (id, name, login, type, etc.)
        private const string CurrentUserInfoPath = "users/get/me";

        // Path for fetching user information by ID
        // Sends: None
        // Receives: UserData (id, name, login, type, etc.)
        private const string UserByIdPath = "users/get/{0}";

        // Path for fetching multiple users with pagination
        // Sends: Query parameters (page, itemsPerPage)
        // Receives: MultiUserDataResponse (List<UserData>, total_count)
        private const string UsersMultiPath = "users/get_multi";

        // Path for fetching user scenario by user ID
        // Sends: Query parameters (optional) for filtering scenarios
        // Receives: MultiAssignedScenarioDataResponse (List<AssignedScenarioData>, total_count)
        private const string UserScenarioPath = "users/scenario/{0}";

        private const string UserScenarioEmptyPath = "users/scenario";

        // Path for uploading a map configuration
        // Sends: UploadScenarioFileData (file, name)
        // Receives: ScenarioData (id, name, owner_id, created_at, FileData)
        private const string MapConfigCreatePath = "mapconfig/create";
        private const string MapConfigDeletePath = "mapconfig/delete/{0}";

        // Path for fetching multiple map configurations with pagination
        // Sends: Query parameters (page, itemsPerPage)
        // Receives: MultiScenarioWithFileDataResponse (List<ScenarioData>, total_count)
        private const string MapConfigMultiPath = "mapconfig/get_multi";
        private const string MapConfigGetPath = "mapconfig/get/{0}";

        // Path for creating a scenario
        // Sends: ScenarioData (name, owner_id, file_id, created_at)
        // Receives: ScenarioData (id, name, owner_id, file_id, created_at)
        private const string ScenarioCreatePath = "scenario/create";

        // Path for fetching a scenario by ID
        // Sends: None
        // Receives: ScenarioData (id, name, owner_id, file_id, created_at)
        private const string ScenarioGetPath = "scenario/get/{0}";
        private const string ScenarioDeletePath = "scenario/delete/{0}";

        // Path for fetching multiple scenarios with pagination
        // Sends: Query parameters (page, itemsPerPage)
        // Receives: MultiScenarioDataResponse (List<ScenarioData>, total_count)
        private const string ScenarioMultiPath = "scenario/get_multi";

        // Path for creating a new device
        // Sends: CreateDeviceRequestData (uuid)
        // Receives: DeviceData (id, uuid, created_at, user_id)
        private const string DeviceCreatePath = "device/create";

        private const string CheckDevicePath = "device/get/uuid/{0}";

        // Path for fetching multiple devices with pagination
        // Sends: Query parameters (page, itemsPerPage)
        // Receives: MultiDeviceDataResponse (List<DeviceData>, total_count)
        private const string DeviceMultiPath = "device/get_multi";

        // Path for creating a log entry
        // Sends: CreateLogRequestData (user_scenario_id, device_uuid, status, file, replay)
        // Receives: LogData (id, uuid, owner_id, scenario_id, file_id, created_at)
        private const string LogCreatePath = "logs/create";
        private const string LogDeletePath = "logs/delete/{0}";
        private const string LogIndividualCreatePath = "logs/personal/create";
        private const string LogIndividualDeletePath = "logs/personal/delete/{0}";
        

        // Path for fetching multiple log entries with pagination
        // Sends: Query parameters (page, itemsPerPage)
        // Receives: MultiLogDataResponse (List<LogData>, total_count)
        private const string LogsMultiPath = "logs/get_multi";
        private const string LogsIndividualMultiPath = "logs/personal/get_multi";

        private const string GroupsList = "groups/get_multi";
        private const string GroupPath = "groups/get/{0}";

        public static string AuthUrl => CombineUrl(UserAuthPath);
        public static string UserInfoUrl => CombineUrl(CurrentUserInfoPath);
        public static string UserByIdUrl(int id) => CombineUrl(string.Format(UserByIdPath, id));

        public static string UsersMultiUrl(Dictionary<string, string> queryParams = null) =>
            CombineUrl(UsersMultiPath, queryParams);

        public static string UserScenarioUrl(int userId, Dictionary<string, string> queryParams = null) =>
            CombineUrl(string.Format(UserScenarioPath, userId), queryParams);

        public static string UserScenarioUrl() => CombineUrl(UserScenarioEmptyPath);

        public static string MapConfigCreateUrl => CombineUrl(MapConfigCreatePath);

        public static string MapConfigMultiUrl(Dictionary<string, string> queryParams = null) =>
            CombineUrl(MapConfigMultiPath, queryParams);
        
        public static string MapConfigGetUrl(int id) =>
            CombineUrl(string.Format(MapConfigGetPath, id));
        
        public static string MapConfigDeleteUrl(int scenarioId) => CombineUrl(string.Format(MapConfigDeletePath, scenarioId));

        public static string ScenarioCreateUrl => CombineUrl(ScenarioCreatePath);
        public static string ScenarioGetUrl(int scenarioId) => CombineUrl(string.Format(ScenarioGetPath, scenarioId));

        public static string ScenarioMultiUrl(Dictionary<string, string> queryParams = null) =>
            CombineUrl(ScenarioMultiPath, queryParams);

        public static string DeviceCreateUrl(Dictionary<string, string> queryParams = null) =>
            CombineUrl(DeviceCreatePath, queryParams);

        public static string DeviceMultiUrl(Dictionary<string, string> queryParams = null) =>
            CombineUrl(DeviceMultiPath, queryParams);

        public static string DeviceCheckUrl(string id) => CombineUrl(string.Format(CheckDevicePath, id));

        public static string LogCreateUrl => CombineUrl(LogCreatePath);
        
        public static string IndividualLogCreateUrl => CombineUrl(LogIndividualCreatePath);
        
        public static string LogDeleteUrl(int id) => CombineUrl(string.Format(LogDeletePath, id));
        
        public static string LogIndividualDeleteUrl(int id) => CombineUrl(string.Format(LogIndividualDeletePath, id));

        public static string LogsMultiUrl(Dictionary<string, string> queryParams = null) =>
            CombineUrl(LogsMultiPath, queryParams);
        
        public static string LogsIndividualMultiUrl(Dictionary<string, string> queryParams = null) =>
            CombineUrl(LogsIndividualMultiPath, queryParams);

        public static string GroupsMulti(Dictionary<string, string> queryParams = null) =>
            CombineUrl(GroupsList, queryParams);

        public static string GetGroup(int id) =>
            CombineUrl(string.Format(GroupPath, id));

        public static string GetFile(string path) => CombineUrl(path, isVersion: false);

        private static string CombineUrl(string path, Dictionary<string, string> queryParams = null, bool isVersion = true)
        {
            var url = new StringBuilder($"{ServerUrl}/");
            if (isVersion)
                url.Append($"{Version}/");
            url.Append($"{path}");

            if (queryParams != null && queryParams.Count > 0)
            {
                var queryString = new List<string>();
                foreach (var param in queryParams)
                {
                    // Формируем строку запроса вручную без замены символов
                    queryString.Add($"{param.Key}={param.Value}");
                }

                // Присоединяем сформированные параметры к URL
                url.Append($"?{string.Join("&", queryString)}");
            }

            return url.ToString();
        }

    }
}