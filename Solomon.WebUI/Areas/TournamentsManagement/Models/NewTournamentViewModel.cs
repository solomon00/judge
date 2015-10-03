using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections.Generic;
using Solomon.Domain.Entities;
using Solomon.TypesExtensions;

namespace Solomon.WebUI.Areas.TournamentsManagement.ViewModels
{
    public class NewTournamentViewModel
    {
        [Required]
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Display(Name = "Тип турнира")]
        public IEnumerable<SelectListItem> TournamentTypesList { get; set; }
        public int TournamentTypesListID { get; set; }

        [Display(Name = "Формат турнира")]
        public IEnumerable<SelectListItem> TournamentFormatsList { get; set; }
        public int TournamentFormatsListID { get; set; }

        [Display(Name = "Отображать в таблице результатов время отправки решения")]
        public bool ShowSolutionSendingTime { get; set; }
        [Display(Name = "Отображать время оставшееся до конца турнира")]
        public bool ShowTimer { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Дата начала")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Время начала")]
        [DisplayFormat(DataFormatString = "{0:HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime StartTime { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Дата конца")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Время конца")]
        [DisplayFormat(DataFormatString = "{0:HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime EndTime { get; set; }

        public NewTournamentViewModel()
        {
            StartDate = DateTime.Now;
            StartTime = DateTime.Parse("00:00");
            EndDate = DateTime.Now;
            EndTime = DateTime.Parse("00:00");

            TournamentFormatsList = new List<SelectListItem>()
                {
                    new SelectListItem() { Value = ((int)TournamentFormats.ACM).ToString(), Text = "ACM (проверка до первого неверного ответа, штрафное время за посылки)" },
                    new SelectListItem() { Value = ((int)TournamentFormats.IOI).ToString(), Text = "IOI (проверка на всех тестах, без штрафных очков)" }
                };

            TournamentTypesList = new List<SelectListItem>()
                {
                    new SelectListItem() { Value = ((int)TournamentTypes.Open).ToString(), Text = "Открытый" },
                    new SelectListItem() { Value = ((int)TournamentTypes.Close).ToString(), Text = "Закрытый" }
                };
        }
    }
}
