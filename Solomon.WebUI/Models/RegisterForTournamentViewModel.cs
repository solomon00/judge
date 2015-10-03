using Solomon.Domain.Entities;
using Solomon.TypesExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Solomon.WebUI.ViewModels
{
    public class RegisterForTournamentViewModel
    {
        public Tournament tournament { get; set; }

        public IEnumerable<SelectListItem> TeamList { get; set; }
        public int UserProfileTeamID { get; set; }
    }
}