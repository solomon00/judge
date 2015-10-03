using Solomon.Domain.Entities;
using Solomon.TypesExtensions;
using System.Collections.Generic;
using System.Web.Security;

namespace Solomon.WebUI.Areas.TournamentsManagement.ViewModels
{
    public class ManageTournamentsViewModel
    {
        public PaginatedList<Tournament> PaginatedTournamentList { get; set; }
        public string FilterBy { get; set; }
        public string SearchTerm { get; set; }
        public int PageSize { get; set; }
    }
}
