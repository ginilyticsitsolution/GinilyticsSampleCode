namespace ZOHO_API.Service
{
    public interface IAccessTokenService
    {
        string GetAccessToken();
        void SetAccessToken(string accessToken);
    }

    public class AccessTokenService : IAccessTokenService
    {
        private string _accessToken;

        public string GetAccessToken() => _accessToken;
        public void SetAccessToken(string accessToken) => _accessToken = accessToken;
    }
}
