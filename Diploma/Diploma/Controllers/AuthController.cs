using Dapper;
using Diploma.Core.Database;
using Diploma.Core.Enums;
using Diploma.Models.Domain;
using Diploma.Models.ViewModels.Auth;
using System;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;

namespace Diploma.Controllers
{
    public class AuthController : Controller
    {
        // GET: /Auth/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Dashboard");

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        // POST: /Auth/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            User user;
            using (var db = DbConnectionFactory.Create())
            {
                user = db.QueryFirstOrDefault<User>(
                    @"SELECT ""Id"", ""UserName"", ""PasswordHash"", ""FullName"",
                             ""Phone"", ""Email"", ""Role"", ""CompanyId"", ""IsActive""
                      FROM ""Users""
                      WHERE ""UserName"" = @UserName AND ""IsActive"" = TRUE",
                    new { model.UserName });
            }

            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Неверный логин или пароль");
                return View(model);
            }

            // Пишем в куку: "userId|role"
            var cookieValue = $"{user.Id}|{(int)user.Role}|{user.FullName}";
            FormsAuthentication.SetAuthCookie(cookieValue, model.RememberMe);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToDashboard(user.Role);
        }

        // POST: /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        // ── helpers ──────────────────────────────────────────────

        private ActionResult RedirectToDashboard(UserRole role)
        {
            switch (role)
            {
                case UserRole.Owner:
                    return RedirectToAction("Index", "Owner");
                case UserRole.DeveloperEngineer:
                    return RedirectToAction("Index", "Engineer");
                case UserRole.Contractor:
                    return RedirectToAction("Index", "Contractor");
                case UserRole.Admin:
                    return RedirectToAction("Index", "Admin");
                default:
                    return RedirectToAction("Index", "Home");
            }
        }

        private static bool VerifyPassword(string plain, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(plain, hash);
        }
    }
}