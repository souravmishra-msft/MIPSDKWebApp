using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using MIPSDK_WebApp1.Controllers;

namespace MIPSDK_WebApp1.Services
{
    public class AuthService
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly string[] _graphScopes;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ITokenAcquisition tokenAcquisition, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _tokenAcquisition = tokenAcquisition;
            _graphScopes = configuration.GetSection("MicrosoftGraph:Scopes").Get<string[]>() ?? Array.Empty<string>();
            _logger = logger;
        }

        public async Task<string> GetTokenAsync()
        {
            try
            {
                var token = await _tokenAcquisition.GetAccessTokenForUserAsync(_graphScopes);
                return token;
            }
            catch (MsalUiRequiredException ex)
            {
                _logger.LogError(ex, "MsalUiRequiredException: User interaction is required.");
                throw;
            }
            catch (MsalServiceException ex)
            {
                _logger.LogError(ex, "MsalServiceException");
                throw;
            }
            catch (MsalClientException ex)
            {
                _logger.LogError(ex, "MsalClientException");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception");
                throw;
            }
        }
    }
}
