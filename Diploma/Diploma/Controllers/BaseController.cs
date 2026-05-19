using BCrypt;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web.Mvc;

namespace Diploma.Controllers
{
    public class BaseController : Controller
    {
        protected string SidebarState => Request.Cookies["sidebarState"]?.Value ?? "expanded";
        protected string SidebarRightState => Request.Cookies["sidebarRightState"]?.Value ?? "expanded";
        protected string UserTheme => Request.Cookies["theme"]?.Value ?? "light";
        protected string Language => Request.Cookies["preferredLanguage"]?.Value ?? "en";
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.Theme = UserTheme;
            ViewBag.SidebarState = SidebarState;
            ViewBag.SidebarRightState = SidebarRightState;
            Thread.CurrentThread.CurrentCulture = new CultureInfo(Language ?? "en");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Language ?? "en");
            base.OnActionExecuting(filterContext);
        }

    }
}