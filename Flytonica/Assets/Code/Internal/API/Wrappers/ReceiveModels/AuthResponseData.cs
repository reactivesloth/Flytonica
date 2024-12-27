namespace Code.Internal.API.Wrappers.ReceiveModels
{
    [System.Serializable]
    public class AuthResponseData
    {
        public string access_token;
        public string refresh_token;
        public int id;
        public UserType type;
    }
}
