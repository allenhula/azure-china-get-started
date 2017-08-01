using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.ActiveDirectory;
using Owin;

namespace AllenlConsentDemo_Api
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseWindowsAzureActiveDirectoryBearerAuthentication(
                new WindowsAzureActiveDirectoryBearerAuthenticationOptions
                {
                    MetadataAddress = $"{ConfigurationManager.AppSettings["ida:AADInstance"]}/{ConfigurationManager.AppSettings["ida:Tenant"]}/federationmetadata/2007-06/federationmetadata.xml",
                    Tenant = ConfigurationManager.AppSettings["ida:Tenant"],
                    TokenValidationParameters = new TokenValidationParameters {
                         ValidAudience = ConfigurationManager.AppSettings["ida:Audience"],
                         RoleClaimType = "roles"
                    }
                });
        }
    }
}
