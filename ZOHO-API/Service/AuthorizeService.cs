namespace ZOHO_API.Service
{
    public interface IAuthorizeService
    {
        string GetClientId();

        string GetClientSecret();

        string GetOrganizationid();    
        void SetClientId(string ClientId);
        void SetClientSecret(string ClientSecret);

        void SetOrganizationid(string Organizationid);
    }
    public class AuthorizeService : IAuthorizeService
    {
        private string _clientId;
        private string _clientSecret;
        private string _organizationid;
        public string GetClientId() => _clientId;

        public string GetClientSecret() => _clientSecret;

        public string GetOrganizationid() => _organizationid;
        public void SetClientId(string ClientId) => _clientId = ClientId;

        public void SetClientSecret(string ClientSecret) => _clientSecret = ClientSecret;

        public void SetOrganizationid(string Organizationid) => _organizationid = Organizationid;
    }
}
