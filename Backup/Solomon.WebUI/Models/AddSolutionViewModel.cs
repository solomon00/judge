using Solomon.Domain.Concrete;
using Solomon.Domain.Entities;
using Solomon.TypesExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Solomon.WebUI.ViewModels
{
    public class AddSolutionViewModel
    {
        public int TournamentID { get; set; }
        public int ProblemID { get; set; }

        public ProblemTypes PT { get; set; }

        [Required(ErrorMessage = "Выберите файл")]
        public HttpPostedFileBase SolutionFile { get; set; }

        public IEnumerable<SelectListItem> ProgrammingLanguagesList { get; set; }
        public int ProgrammingLanguageID { get; set; }

        public AddSolutionViewModel()
        {
            List<SelectListItem> tempList = new List<SelectListItem>();

            var repository = new EFRepository();
            repository.ProgrammingLanguages.Each(pl =>
                {
                    if (repository.IsProgrammingLanguageEnable(pl.ProgrammingLanguageID))
                        tempList.Add(new SelectListItem() { Text = pl.Title, Value = ((int)pl.ProgrammingLanguageID).ToString() });
                });

            ProgrammingLanguagesList = tempList;
        }
    }
}