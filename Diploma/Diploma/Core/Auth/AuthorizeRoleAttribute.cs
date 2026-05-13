using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Diploma.Core.Enums;

namespace Diploma.Core.Auth
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AuthorizeRoleAttribute : AuthorizeAttribute
    {
        private readonly UserRole[] _roles;

        public AuthorizeRoleAttribute(params UserRole[] roles)
        {
            _roles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
                return false;

            var roleClaim = httpContext.User.Identity.Name
                .Split('|')
                .ElementAtOrDefault(1);

            if (!int.TryParse(roleClaim, out var roleInt))
                return false;

            var userRole = (UserRole)roleInt;
            return _roles.Contains(userRole);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
                filterContext.Result = new RedirectResult("~/Auth/Login");
            else
                filterContext.Result = new HttpStatusCodeResult(403);
        }
    }
}