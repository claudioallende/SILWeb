using CuposCorretajeWeb.Models.Account;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Account
{
    public class UserStore : IUserStore<Usuario, string>,
        IUserPasswordStore<Usuario, string>,
        IUserLockoutStore<Usuario, string>,
        IUserTwoFactorStore<Usuario, string>
    {
        public System.Threading.Tasks.Task CreateAsync(Usuario user)
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task DeleteAsync(Usuario user)
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<Usuario> FindByIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<Usuario> FindByNameAsync(string userName)
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task UpdateAsync(Usuario user)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public System.Threading.Tasks.Task<string> GetPasswordHashAsync(Usuario user)
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<bool> HasPasswordAsync(Usuario user)
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task SetPasswordHashAsync(Usuario user, string passwordHash)
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<int> GetAccessFailedCountAsync(Usuario user)
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<bool> GetLockoutEnabledAsync(Usuario user)
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<DateTimeOffset> GetLockoutEndDateAsync(Usuario user)
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<int> IncrementAccessFailedCountAsync(Usuario user)
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task ResetAccessFailedCountAsync(Usuario user)
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task SetLockoutEnabledAsync(Usuario user, bool enabled)
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task SetLockoutEndDateAsync(Usuario user, DateTimeOffset lockoutEnd)
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<bool> GetTwoFactorEnabledAsync(Usuario user)
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task SetTwoFactorEnabledAsync(Usuario user, bool enabled)
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task AddClaimAsync(Usuario user, System.Security.Claims.Claim claim)
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<IList<System.Security.Claims.Claim>> GetClaimsAsync(Usuario user)
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task RemoveClaimAsync(Usuario user, System.Security.Claims.Claim claim)
        {
            throw new NotImplementedException();
        }
    }
}