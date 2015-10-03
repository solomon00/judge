using System.Web.Mvc;

namespace Solomon.WebUI.Areas.User
{
    public class UserAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "User";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "User_Team_default",
                url: "User/{UserName}/Team/{TeamID}/{action}",
                defaults: new { controller = "Team", action = "Index" },
                constraints: new { TeamID = @"\d+" }
            );

            context.MapRoute(
                "User_default",
                "User/{UserName}/{controller}/{action}",
                new { action = "Index" }
            );
        }
    }
}
