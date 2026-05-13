
using Diploma.Models;
using System.Web;
using System.Web.Mvc;

namespace Diploma.Helpers
{
    public class AuthorizeRoleAttribute : AuthorizeAttribute
    {
        private readonly UserRole[] _allowedRoles;

        public AuthorizeRoleAttribute(params UserRole[] roles)
        {
            _allowedRoles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
                return false;

            var user = UserIdentityHelper.GetCurrentUser();
            if (user == null)
                return false;

            foreach (var role in _allowedRoles)
            {
                if (user.Role == role)
                    return true;
            }

            return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectResult("~/Auth/Login?returnUrl=" +
                    System.Web.HttpUtility.UrlEncode(filterContext.HttpContext.Request.RawUrl));
            }
            else
            {
                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/AccessDenied.cshtml"
                };
            }
        }
    }
}