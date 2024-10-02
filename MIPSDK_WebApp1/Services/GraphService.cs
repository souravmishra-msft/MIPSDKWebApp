using Microsoft.Graph.Drives.Item.Items.Item.GetActivitiesByInterval;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace MIPSDK_WebApp1.Services
{
    public class GraphService
    {
        private readonly ILogger<GraphService> _logger;
        private readonly HttpClient _httpClient;

        public GraphService(ILogger<GraphService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        //Get User Profile
        public  async Task<User?> GetUserProfileAsync(string accessToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me");

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var userProfile = JsonConvert.DeserializeObject<User>(jsonResponse);
                    return userProfile;
                }
                else
                {
                    _logger.LogError("Error calling the Graph API: {StatusCode} - {Reason}", response.StatusCode, response.ReasonPhrase);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Graph API");
                return null;
            }
        }

        //Get User Profile Image
        public async Task<byte[]> GetUserProfileImageAsync(string accessToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me/photo/$value");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await _httpClient.SendAsync(request);
                if(response.IsSuccessStatusCode)
                {
                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    return imageBytes;
                }
                else
                {
                    _logger.LogError("Error calling the Graph API: {StatusCode} - {Reason}", response.StatusCode, response.ReasonPhrase);
                    return null;
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Graph API for profile image");
                return null;
            }
        }
    }
}
