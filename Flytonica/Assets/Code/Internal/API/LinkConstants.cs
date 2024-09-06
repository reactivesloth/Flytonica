using System.IO;

namespace Code.Internal.API
{
    public static class LinkConstants
    {
        private const string ServerUrl = "https://dronesimapi.4app.pro/v1";
        
        private const string UserAuthPath = "users/auth";
        private const string CurrentUserInfoPath = "users/get/me";
        
        public static string AuthUrl => Path.Combine(ServerUrl, UserAuthPath);
        public static string UserInfoUrl => Path.Combine(ServerUrl, CurrentUserInfoPath);
    }
}