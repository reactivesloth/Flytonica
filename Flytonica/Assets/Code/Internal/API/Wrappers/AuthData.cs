namespace Code.Internal.API.Wrappers
{
    [System.Serializable]
    public class AuthData
    {
        public string login;
        public string password;

        public AuthData(string login, string password)
        {
            this.login = login;
            this.password = password;
        }
    }
}