using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace CuposCorretajeWeb.Models.Account
{
    public class Usuario : IUser<string>
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public Usuario() { }
        public Usuario(string Id, string UserName, string Password)
        {
            this.Id = Id;
            this.UserName = UserName;
            this.Password = Password;
        }

        public virtual async Task<ClaimsIdentity> GenerateUserIdentityAsync(
            UserManager<Usuario, string> manager)
        {
            // Note the authenticationType must match the one defined in
            // CookieAuthenticationOptions.AuthenticationType 
            var userIdentity = await manager.CreateIdentityAsync(
                this, DefaultAuthenticationTypes.ApplicationCookie);
            userIdentity.AddClaims(await manager.GetClaimsAsync(this.UserName));
            return userIdentity;
        }
    }
}