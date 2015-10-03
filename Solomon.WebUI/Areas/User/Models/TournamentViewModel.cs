using Solomon.Domain.Entities;
using Solomon.TypesExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace Solomon.WebUI.Areas.User.ViewModels
{
    public class TournamentListViewModel : UserViewModel
    {
        public IEnumerable<Tournament> ActiveTournaments { get; set; }
        public IEnumerable<Tournament> NotBegunTournaments { get; set; }
        public IEnumerable<Tournament> FinishTournaments { get; set; }
    }

}
