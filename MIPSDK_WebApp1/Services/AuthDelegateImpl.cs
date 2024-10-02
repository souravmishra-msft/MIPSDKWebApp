using Microsoft.Identity.Client;
using Microsoft.InformationProtection;

namespace MIPSDK_WebApp1.Services
{
    public class AuthDelegateImpl: IAuthDelegate
    {
        private readonly string _redirectUri;
        private readonly string _tenantId;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public AuthDelegateImpl(IConfiguration configuration)
        {
            _redirectUri = "https://localhost:44395/" + configuration.GetSection("AzureAd").GetValue<string>("CallbackPath");
            _tenantId = configuration.GetSection("AzureAd").GetValue<string>("TenantId");
            _clientId = configuration.GetSection("AzureAd").GetValue<string>("ClientId");
            _clientSecret = configuration.GetSection("AzureAd").GetValue<string>("ClientSecret");
        }

        public string AcquireToken(Identity identity, string authority, string resource, string claims) {
            IConfidentialClientApplication app;
            AuthenticationResult authResult;

            if (authority.ToLower().Contains("common"))
            {
                var authorityUri = new Uri(authority);
                authority = String.Format("https://{0}/{1}", authorityUri.Host, _tenantId);
            }

            Console.WriteLine("Performing client secret based authentication (Client-Credentials Flow)");
            app = ConfidentialClientApplicationBuilder.Create(_clientId)
                .WithClientSecret(_clientSecret)
                .WithRedirectUri(_redirectUri)
                .Build();

            string[] scopes = new string[] { resource[resource.Length - 1].Equals('/') ? $"{resource}.default": $"{resource}/.default" };


            authResult = app.AcquireTokenForClient(scopes)
                .WithTenantId(_tenantId)
                .ExecuteAsync()
                .GetAwaiter()
                .GetResult();

            return authResult.AccessToken;
        }
    }
}
