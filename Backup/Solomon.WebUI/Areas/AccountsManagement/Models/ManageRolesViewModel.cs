using System.Collections.Generic;
using System.Web.Mvc;

namespace Solomon.WebUI.Areas.AccountsManagement.ViewModels
{
    public class ManageRolesViewModel
    {
        public SelectList Roles { get; set; }
        public string[] RoleList { get; set; }
    }
}
