using Solomon.Domain.Entities;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Security;

namespace Solomon.WebUI.Areas.ProblemsManagement.ViewModels
{
    public class BindTournamentsToProblemViewModel
    {
        public int ProblemID { get; set; }
        public string ProblemName { get; set; }
        public SelectList AvailableTournaments { get; set; }
        public SelectList BoundTournaments { get; set; }
    }
}
