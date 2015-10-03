using Solomon.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Solomon.WebUI.ViewModels
{
    public class ProblemsListViewModel
    {
        public IEnumerable<Problem> Problems { get; set; }
        public int TournamentID { get; set; }
    }
}