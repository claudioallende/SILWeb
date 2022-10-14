using IdentityModel.Client;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using RestSharp;
using CuposCorretajeWeb.Models.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CuposCorretajeWeb.Models.Identity;

namespace CuposCorretajeWeb.Models.Account
{
    public class SignInManager : SignInManager<Usuario, string>
    {
        public SignInManager(UserManager<Usuario, string> userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager) { }

        public async Task<SignInStatus> PasswordSignInAsync(string UserName, string Password)
        {
            Task<string> IdTokenAsync = GetIdToken(UserName, Password);
            TokenResponse token = GetAccessToken(UserName, Password);
            string IdToken = await IdTokenAsync;
            if (!string.IsNullOrEmpty(IdToken) && !string.IsNullOrEmpty(token.AccessToken))
            {
                DatosUsuario datos = await GetDatosUsuario(IdToken);
                SetTokenInSession(token);
                SetClaims("Centro", ClaimsUtil.GetListClaims(datos.Claims, "Centro"));
                SetClaim("CentroPorDefecto", ClaimsUtil.GetClaim(datos.Claims, "CentroPorDefecto"));
                SetClaim("AccesoAuditoriaCuposCorretaje", ClaimsUtil.GetClaim(datos.Claims, "AccesoAuditoriaCuposCorretaje"));
                SetClaim("AccesoConfiguracionCYOeEmailDeCuentasCuposCorretaje", ClaimsUtil.GetClaim(datos.Claims, "AccesoConfiguracionCYOeEmailDeCuentasCuposCorretaje"));
                Task signin = SignInAsync(new Usuario(UserName, UserName, Password), false, false);
                await signin;
                return SignInStatus.Success;
            }
            return SignInStatus.Failure;
        }

        public Task<DatosUsuario> GetDatosUsuario(string IdToken)
        {
            var client = new RestClient(ConfigurationManager.AppSettings["SSO_SERVER"] + "/api/User/GetUserPermissionCuposCorretaje");
            var request = new RestRequest();
            request.Method = Method.GET;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + IdToken);
            var response = client.Execute(request);
            string content = response.Content;
            return Task.Run(() => Newtonsoft.Json.JsonConvert.DeserializeObject<DatosUsuario>(content, new ClaimsConverter()));
        }

        public Task<string> GetIdToken(string UserName, string Password)
        {
            var client = new RestClient(ConfigurationManager.AppSettings["SSO_SERVER"] + "/api/user/authenticate");
            var request = new RestRequest();
            request.Method = Method.POST;
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(new { 
                Username = UserName,
                Password = Password,
                Sitio = "cuposcorretaje"
            });
            var response = client.Execute(request);
            string content = response.Content;
            return Task.Run(() => Newtonsoft.Json.JsonConvert.DeserializeObject<string>(content));
        }

        public async Task<SignInStatus> ExternalSignInAsync(string idToken)
        {
            DatosUsuario datos = await GetDatosUsuario(idToken);
            if (datos.IsAuthenticated())
            {
                TokenResponse token = GetAccessToken(datos.USUARIO.ToLower(), datos.PASSWORD.ToLower());
                SetTokenInSession(token);
                SetClaims("Centro", ClaimsUtil.GetListClaims(datos.Claims, "Centro"));
                SetClaim("CentroPorDefecto", ClaimsUtil.GetClaim(datos.Claims, "CentroPorDefecto"));
                SetClaim("AccesoAuditoriaCuposCorretaje", ClaimsUtil.GetClaim(datos.Claims, "AccesoAuditoriaCuposCorretaje"));
                SetClaim("AccesoConfiguracionCYOeEmailDeCuentasCuposCorretaje", ClaimsUtil.GetClaim(datos.Claims, "AccesoConfiguracionCYOeEmailDeCuentasCuposCorretaje"));
                await SignInAsync(new Usuario(datos.USUARIO, datos.USUARIO, datos.PASSWORD), false, false);
                return SignInStatus.Success;
            }
            return SignInStatus.Failure;
        }

        public async Task SignInAsync(
            Usuario user,
            bool isPersistent,
            bool rememberBrowser)
        {
            // Clear any partial cookies from external or two factor partial sign ins
            AuthenticationManager.SignOut(
                DefaultAuthenticationTypes.ApplicationCookie);
            var userIdentity = await user.GenerateUserIdentityAsync(UserManager);
            if (rememberBrowser)
            {
                var rememberBrowserIdentity =
                    AuthenticationManager.CreateTwoFactorRememberBrowserIdentity(user.Id);
                AuthenticationManager.SignIn(
                    new AuthenticationProperties { IsPersistent = isPersistent },
                    userIdentity,
                    rememberBrowserIdentity);
            }
            else
            {
                AuthenticationManager.SignIn(
                    new AuthenticationProperties { IsPersistent = isPersistent },
                    userIdentity);
            }
        }

        public void SetTokenInSession(TokenResponse token)
        {
            UserManager Manager = UserManager as CuposCorretajeWeb.Models.Account.UserManager;
            Manager.SetClaim("access_token", token.AccessToken);
        }

        public void SetClaim(string key, string value)
        {
            UserManager Manager = UserManager as CuposCorretajeWeb.Models.Account.UserManager;
            Manager.SetClaim(key, value);
        }

        public void SetClaims(string key, IList<string> values)
        {
            UserManager Manager = UserManager as CuposCorretajeWeb.Models.Account.UserManager;
            Manager.SetClaims(key, values);
        }

        public TokenResponse GetAccessToken(string UserName, string Password)
        {
            string client_id = ConfigurationManager.AppSettings["CLIENT_ID"];
            string client_secret = ConfigurationManager.AppSettings["CLIENT_SECRET"];
            TokenClient tokenClient = new TokenClient(
                ConfigurationManager.AppSettings["AUTH_SERVER"],
                client_id,
                client_secret);
            return RequestToken(tokenClient, UserName, Password);
        }

        public TokenResponse RequestToken(TokenClient tokenClient, string UserName, string Password)
        {
            return tokenClient.RequestResourceOwnerPasswordAsync
                (UserName, Password, ConfigurationManager.AppSettings["SCOPES"]).Result;
        }

        public TokenResponse RefreshToken(TokenClient tokenClient, string refreshToken)
        {
            Console.WriteLine("Using refresh token: {0}", refreshToken);

            return tokenClient.RequestRefreshTokenAsync(refreshToken).Result;
        }
    }
}