namespace Code.Internal.API.Wrappers
{
    [System.Serializable]
    public class UserAuthData
    {
        public string login;
        public string password;

        public UserAuthData(string login, string password)
        {
            this.login = login;
            this.password = password;
        }
    }
}