using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace Code.Internal.API
{
    public static class LinkConstants
    {
        private const string ServerUrl = "https://dronesimapi.4app.pro/v1";

        private const string UserAuthPath = "users/auth";
        private const string CurrentUserInfoPath = "users/get/me";
        private const string UserByIdPath = "users/get/{0}";
        private const string UsersMultiPath = "users/get_multi";
        private const string UserScenarioPath = "users/scenario/{0}";
        private const string MapConfigCreatePath = "mapconfig/create";
        private const string MapConfigMultiPath = "mapconfig/get_multi";
        private const string ScenarioCreatePath = "scenario/create";
        private const string ScenarioGetPath = "scenario/get/{0}";
        private const string ScenarioMultiPath = "scenario/get_multi";
        private const string DeviceCreatePath = "device/create";
        private const string DeviceMultiPath = "device/get_multi";
        private const string LogCreatePath = "logs/create";
        private const string LogsMultiPath = "logs/get_multi";

        public static string AuthUrl => CombineUrl(UserAuthPath);
        public static string UserInfoUrl => CombineUrl(CurrentUserInfoPath);
        public static string UserByIdUrl(int id) => CombineUrl(string.Format(UserByIdPath, id));

        public static string UsersMultiUrl(Dictionary<string, string> queryParams = null) =>
            CombineUrl(UsersMultiPath, queryParams);

        public static string UserScenarioUrl(int userId, Dictionary<string, string> queryParams = null) =>
            CombineUrl(string.Format(UserScenarioPath, userId), queryParams);

        public static string MapConfigCreateUrl => CombineUrl(MapConfigCreatePath);

        public static string MapConfigMultiUrl(Dictionary<string, string> queryParams = null) =>
            CombineUrl(MapConfigMultiPath, queryParams);

        public static string ScenarioCreateUrl => CombineUrl(ScenarioCreatePath);
        public static string ScenarioGetUrl(int scenarioId) => CombineUrl(string.Format(ScenarioGetPath, scenarioId));

        public static string ScenarioMultiUrl(Dictionary<string, string> queryParams = null) =>
            CombineUrl(ScenarioMultiPath, queryParams);

        public static string DeviceCreateUrl(Dictionary<string, string> queryParams = null) =>
            CombineUrl(DeviceCreatePath, queryParams);

        public static string DeviceMultiUrl(Dictionary<string, string> queryParams = null) =>
            CombineUrl(DeviceMultiPath, queryParams);

        public static string LogCreateUrl => CombineUrl(LogCreatePath);

        public static string LogsMultiUrl(Dictionary<string, string> queryParams = null) =>
            CombineUrl(LogsMultiPath, queryParams);

        private static string CombineUrl(string path, Dictionary<string, string> queryParams = null)
        {
            var url = new StringBuilder($"{ServerUrl}/{path}");

            if (queryParams != null && queryParams.Count > 0)
            {
                var query = HttpUtility.ParseQueryString(string.Empty);
                foreach (var param in queryParams)
                {
                    query[param.Key] = param.Value;
                }

                url.Append($"?{query}");
            }

            return url.ToString();
        }
    }
}