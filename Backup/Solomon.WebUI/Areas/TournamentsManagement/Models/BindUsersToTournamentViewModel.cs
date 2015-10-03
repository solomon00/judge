using Solomon.Domain.Entities;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Security;

namespace Solomon.WebUI.Areas.TournamentsManagement.ViewModels
{
    public class BindUsersToTournamentViewModel
    {
        public int TournamentID { get; set; }
        public string TournamentName { get; set; }
        public SelectList AvailableUsers { get; set; }
        public SelectList BoundUsers { get; set; }
    }
}
