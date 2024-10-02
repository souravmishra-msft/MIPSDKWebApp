using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using MIPSDK_WebApp1.Models;
using MIPSDK_WebApp1.Services;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace MIPSDK_WebApp1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AuthService _authService;
        private readonly GraphService _graphService;

        public HomeController(
            ILogger<HomeController> logger, 
            AuthService authService,
            GraphService graphService)
        {
            _logger = logger;
            _authService = authService;
            _graphService = graphService;
        }

        [AuthorizeForScopes(ScopeKeySection = "MicrosoftGraph:Scopes")]
        public IActionResult Index()
        {
            return View();
        }

        [AuthorizeForScopes(ScopeKeySection = "MicrosoftGraph:Scopes")]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var token = await _authService.GetTokenAsync();
                ViewData["Access-Token"] = token;

                var userProfile = await _graphService.GetUserProfileAsync(token);
                if (userProfile == null)
                {
                    return View("Error");
                }

                ViewData["UserProfile"] = userProfile;

                var userProfileImage = await _graphService.GetUserProfileImageAsync(token);
                if(userProfileImage != null)
                {
                    var base64Image = Convert.ToBase64String(userProfileImage);
                    ViewData["ProfileImage"] = base64Image;
                }
            }
            catch (MsalUiRequiredException ex)
            {
                _logger.LogWarning(ex, "MsalUiRequiredException: User interaction is required.");
                // Trigger interactive login
                return Challenge(new AuthenticationProperties { RedirectUri = "/Home/Profile"});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acquiring access token");
                return View("Error");
            }

            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}