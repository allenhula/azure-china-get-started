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

        [Authorize]
        public async Task<ActionResult> About()
        {
            try
            {
                var userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                var authContext = new AuthenticationContext(Startup.authority, new NaiveSessionCache(userObjectID));
                var credential = new ClientCredential(Startup.clientId, Startup.clientSecret);
                var userIdentifier = new UserIdentifier(userObjectID, UserIdentifierType.UniqueId);
                var result = await authContext.AcquireTokenSilentAsync(Startup.targetResource, credential, userIdentifier);
                var token = result.AccessToken;

                var subId = "6fe23adb-9c5f-478b-a01d-2d23187f9dd4";
                var getStorageAccountsURL = $"https://management.chinacloudapi.cn/subscriptions/{subId}/providers/Microsoft.Storage/storageAccounts?api-version=2016-12-01";
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                    ViewBag.Message = await httpClient.GetStringAsync(getStorageAccountsURL);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Exception: {ex.Message}";
            }            

            return View();
        }

        [Authorize(Roles = "DemoAdmin")]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}