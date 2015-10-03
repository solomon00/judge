using System.Web.Mvc;

namespace Solomon.WebUI.Areas.TestersManagement
{
    public class TestersManagementAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "TestersManagement";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "TestersManagement_default",
                "TestersManagement/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
