using System.Web.Mvc;

namespace Solomon.WebUI.Areas.ProblemsManagement
{
    public class ProblemsManagementAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "ProblemsManagement";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "ProblemsManagement_ProblemID",
                "ProblemsManagement/{controller}/{action}/{ProblemID}",
                new { ProblemID = UrlParameter.Optional }
            );

            context.MapRoute(
                "ProblemsManagement_default",
                "ProblemsManagement/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
