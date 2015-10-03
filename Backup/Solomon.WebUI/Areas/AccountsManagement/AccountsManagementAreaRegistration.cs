using System.Web.Mvc;

namespace Solomon.WebUI.Areas.AccountsManagement
{
    public class AccountsManagementAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "AccountsManagement";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "AccountsManagement_InstitutionID",
                "AccountsManagement/Institution/{action}/{InstitutionID}",
                new { controller = "Institution", InstitutionID = UrlParameter.Optional }
            );

            context.MapRoute(
                "AccountsManagement_UserID",
                "AccountsManagement/Membership/{action}/{UserID}",
                new { controller = "Membership", UserID = UrlParameter.Optional },
                new { UserID = @"\d+" }
            );

            context.MapRoute(
                "AccountsManagement_default",
                "AccountsManagement/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
