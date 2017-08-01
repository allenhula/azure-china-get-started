using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AllenlAadSsoDemo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> About()
        {
            try
            {
                var clientCredential = new ClientCredential(Startup.clientId, Startup.clientSecret);
                var authContext = new AuthenticationContext(Startup.authority);
                var authResult = await authContext.AcquireTokenAsync(Startup.targetResource, clientCredential);
                var token = authResult.AccessToken;

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                    ViewBag.Message = await httpClient.GetStringAsync($"{Startup.targetResource}api/values");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Exception: {ex.Message}";
            }            

            return View();
        }

        [Authorize]
        public async Task<ActionResult> Contact()
        {
            try
            {
                var userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                var authContext = new AuthenticationContext(Startup.authority, new NaiveSessionCache(userObjectID));
                var credential = new ClientCredential(Startup.clientId, Startup.clientSecret);
                var userIdentifier = new UserIdentifier(userObjectID, UserIdentifierType.UniqueId);
                var result = await authContext.AcquireTokenSilentAsync(Startup.targetResource, credential, userIdentifier);
                var token = result.AccessToken;

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                    ViewBag.Message = await httpClient.GetStringAsync($"{Startup.targetResource}api/values/5");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Expection: {ex.Message}";
            }

            return View();
        }
    }
}