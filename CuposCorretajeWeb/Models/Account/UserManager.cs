using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace CuposCorretajeWeb.Models.Account
{
    public class UserManager : UserManager<Usuario, string>
    {
        public UserManager(IUserStore<Usuario, string> store)
            : base(store)
        {
            AllClaims = new List<Claim>();
        }

        private IList<System.Security.Claims.Claim> AllClaims { get; set; }

        public override Task<IList<System.Security.Claims.Claim>> GetClaimsAsync(string userId)
        {
            return Task.FromResult<IList<System.Security.Claims.Claim>>(AllClaims);
        }

        public void SetAccessTokenClaims(string AccessToken)
        {
            AllClaims.Add(new Claim("access_token", AccessToken));
        }

        public void SetClaim(string key, string value)
        {
            AllClaims.Add(new Claim(key, value));
        }

        public void SetClaims(string key, IList<string> values)
        {
            foreach (string value in values)
            {
                AllClaims.Add(new Claim(key, value));
            }
        }
    }
}