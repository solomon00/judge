using Solomon.Domain.Entities;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Security;

namespace Solomon.WebUI.Areas.TournamentsManagement.ViewModels
{
    public class BindProblemsToTournamentViewModel
    {
        public int TournamentID { get; set; }
        public string TournamentName { get; set; }
        public SelectList AvailableProblems { get; set; }
        public SelectList BoundProblems { get; set; }
    }
}
