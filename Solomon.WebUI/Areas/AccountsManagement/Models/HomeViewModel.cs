using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Solomon.WebUI.Areas.AccountsManagement.ViewModels
{
    public class HomeViewModel
    {
        public string TotalUserCount { get; set; }
        public string TotalOnlineUsersCount { get; set; }
        public string TotalAuthenticatedUsersCount { get; set; }
        public string TotalRolesCount { get; set; }
    }
}
