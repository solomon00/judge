using Solomon.Domain.Entities;
using Solomon.TypesExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Solomon.WebUI.ViewModels
{
    public class TournamentListViewModel
    {
        public IEnumerable<Tournament> ActiveTournaments { get; set; }
        public IEnumerable<Tournament> NotBegunTournaments { get; set; }
        public IEnumerable<Tournament> FinishTournaments { get; set; }
    }
}