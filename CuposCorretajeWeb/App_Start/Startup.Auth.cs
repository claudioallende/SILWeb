using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using CuposCorretajeWeb.Models;
using CuposCorretajeWeb.Models.Account;

namespace CuposCorretajeWeb
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            app.CreatePerOwinContext(() => new UserManager(new UserStore()));
            app.CreatePerOwinContext<SignInManager>((options, context) => new SignInManager(context.GetUserManager<UserManager>(), context.Authentication));

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider(),
                ExpireTimeSpan = TimeSpan.FromHours(20),
                SlidingExpiration = false
                //ExpireTimeSpan = TimeSpan.FromMinutes(15) //Tiempo de inactividad para vencer la sesion
            });
        }
    }
}