using Diploma.Helpers;
using Diploma.Models;
using Diploma.Services.Interfaces;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Diploma.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AuthController()
        {
            _authService = DependencyResolver.Current.GetService<IAuthService>();
            _userService = DependencyResolver.Current.GetService<IUserService>();
        }

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                var currentUser = UserIdentityHelper.GetCurrentUser();
                if (currentUser != null)
                {
                    return RedirectToArea(currentUser.Role);
                }
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginRequest model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                var currentUser = UserIdentityHelper.GetCurrentUser();
                if (currentUser != null)
                {
                    return RedirectToArea(currentUser.Role);
                }
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.AuthenticateAsync(model.Login, model.Password);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.ErrorMessage);
                return View(model);
            }

            var userData = $"{result.User.Id}|{(int)result.User.Role}|{result.User.CompanyId}";

            var ticket = new FormsAuthenticationTicket(
                1,
                result.User.UserName,
                System.DateTime.Now,
                model.RememberMe ? System.DateTime.Now.AddDays(30) : System.DateTime.Now.AddHours(8),
                model.RememberMe,
                userData,
                FormsAuthentication.FormsCookiePath
            );

            var encryptedTicket = FormsAuthentication.Encrypt(ticket);
            var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            Response.Cookies.Add(authCookie);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Перенаправляем в Area пользователя
            return RedirectToArea(result.User.Role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Login", "Auth");
        }

        // Метод для перенаправления в Area по роли
        private ActionResult RedirectToArea(UserRole role)
        {
            switch (role)
            {
                case UserRole.Admin:
                    return RedirectToAction("Index", "Admin", new { area = "Admin" });
                case UserRole.Owner:
                    return RedirectToAction("Defects", "Owner", new { area = "Owner" });
                case UserRole.DeveloperEngineer:
                    return RedirectToAction("Index", "Engineer", new { area = "Engineer" });
                case UserRole.Contractor:
                    return RedirectToAction("Index", "Contractor", new { area = "Contractor" });
                default:
                    return RedirectToAction("Index", "Home");
            }
        }
    }
}