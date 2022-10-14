using System;
using System.Collections.Generic;
using System.Linq;
//using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace CuposCorretajeWeb.Models.AtributosValidacion
{
    public class ClaimsAuthorizeAttribute : AuthorizeAttribute
    {
        private string claimType;
        private string claimValue;

        public ClaimsAuthorizeAttribute(string type, string value)
        {
            this.claimType = type;
            this.claimValue = value;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var identity = (System.Security.Claims.ClaimsIdentity)Thread.CurrentPrincipal.Identity;
            var claim = identity.Claims.FirstOrDefault(c => c.Type == claimType && c.Value == claimValue);

            if (claim != null)
            {
                base.OnAuthorization(filterContext);
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    } 
}