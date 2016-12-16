using Microsoft.Azure.KeyVault;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace KeyVaultWebDemo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Result()
        {
            var sb = new StringBuilder();
            var containerName = "kvdemocontainer";

            try
            {
                var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(Utils.GetToken));
                sb.AppendLine("Connected to Key Vault! || ");

                var connectionString = keyVaultClient.GetSecretAsync(WebConfigurationManager.AppSettings["connStringKvUri"]).Result.Value;
                sb.AppendLine("Get connection string from Key Vault! || ");

                Utils.EncryptSecret = connectionString;
                sb.AppendLine(string.Format(@"Connection string is: {0}. || ", connectionString));

                var storageAccount = CloudStorageAccount.Parse(connectionString);
                sb.AppendLine(@"Connected to storage!  || ");

                var blobClient = storageAccount.CreateCloudBlobClient();
                
                var container = blobClient.GetContainerReference(containerName);
                container.CreateIfNotExists();
                sb.AppendLine(string.Format(@"Created a container named {0}.  || ", containerName));
            }
            catch (Exception ex)
            {
                sb.AppendLine(string.Format("Got Exception: {0}", ex.Message == string.Empty ? "something wrong" : ex.Message));
            }

            ViewBag.Message = sb.ToString();

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}