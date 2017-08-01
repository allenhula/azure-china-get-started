using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace AllenlConsentDemo_Api
{
    public class ClaimsAuthorizeAttribute : AuthorizeAttribute //AuthorizationFilterAttribute
    {
        private string claimType;
        private string claimValue;

        public ClaimsAuthorizeAttribute(string type, string value)
        {
            this.claimType = type;
            this.claimValue = value;
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var claims = ClaimsPrincipal.Current.Claims.ToList();

            var typeMatchingClaim = claims.Find(c => c.Type.Equals(this.claimType, StringComparison.OrdinalIgnoreCase));

            if (typeMatchingClaim != null)
            {
                var values = typeMatchingClaim.Value.Split(' ');
                foreach (var value in values)
                {
                    if (value.Equals(this.claimValue, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}