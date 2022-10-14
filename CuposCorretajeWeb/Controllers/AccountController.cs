using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CuposCorretajeWeb.Models.Account;

namespace CuposCorretajeWeb.Controllers
{
    public class AccountController : Controller
    {
        private SignInManager _signInManager;

        public AccountController()
        {
        }

        public AccountController(SignInManager signInManager)
        {
            SignInManager = signInManager;
        }

        public SignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<SignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Signout()
        {
            HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (TempData["error"] != null) ModelState.AddModelError("", TempData["error"].ToString());
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            try
            {
                var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password);
                switch (result)
                {
                    case SignInStatus.Success:
                        return RedirectToLocal(returnUrl);
                    case SignInStatus.LockedOut:
                        return View("Lockout");
                    case SignInStatus.Failure:
                    default:
                        ModelState.AddModelError("", "Usuario o contraseña incorrecto.");
                        return View(model);
                }
            }
            catch (Exception e)
            {
#if DEBUG
                ModelState.AddModelError("", e.Message);
#else
                ModelState.AddModelError("", "Error");
#endif
                return View(model);
            }
        }

        public async Task<ActionResult> Autent(string idToken)
        {
            try
            {
                SignInStatus autenticado = await SignInManager.ExternalSignInAsync(idToken);
                if (autenticado == SignInStatus.Success) return RedirectToAction("Index", "Cupos");
                else return RedirectToAction("Login");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Error");
                return RedirectToAction("Login");
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Cupos");
        }
    }
}