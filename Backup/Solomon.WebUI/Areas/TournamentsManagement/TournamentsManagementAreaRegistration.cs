using System.Web.Mvc;

namespace Solomon.WebUI.Areas.TournamentsManagement
{
    public class TournamentsManagementAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "TournamentsManagement";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "TournamentsManagement_TournamentID",
                "TournamentsManagement/{controller}/{action}/{TournamentID}",
                new { TournamentID = UrlParameter.Optional }
            );

            context.MapRoute(
                "TournamentsManagement_default",
                "TournamentsManagement/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
