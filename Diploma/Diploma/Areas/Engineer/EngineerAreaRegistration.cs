using System.Web.Mvc;

namespace Diploma.Areas.Engineer
{
    public class EngineerAreaRegistration : AreaRegistration
    {
        public override string AreaName => "Engineer";

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Engineer_default",
                "Engineer/{controller}/{action}/{id}",
                new { controller = "Engineer", action = "Index", id = UrlParameter.Optional },
                new[] { "Diploma.Areas.Engineer.Controllers" }
            );
        }
    }
}