using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Solomon.WebUI.Areas.TournamentsManagement.ViewModels
{
    public class HomeViewModel
    {
        public string TotalTournamentCount { get; set; }
        public string TotalActiveTournamentCount { get; set; }
        public string TotalFinishTournamentCount { get; set; }
        public string TotalNotBegunTournamentCount { get; set; }
        public string ACMTournamentCount { get; set; }
        public string IOITournamentCount { get; set; }
        public string OpenTournamentCount { get; set; }
        public string CloseTournamentCount { get; set; }
    }
}
