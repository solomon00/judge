using Solomon.Domain.Entities;
using Solomon.TypesExtensions;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solomon.WebUI.Areas.AccountsManagement.ViewModels
{
    public class ManageInstitutionsViewModel
    {
        public PaginatedList<Institution> PaginatedInstitutionList { get; set; }
        public string FilterBy { get; set; }
        public string SearchTerm { get; set; }
        public int PageSize { get; set; }
    }

    public class CreateInstitutionViewModel
    {
        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Страна")]
        public string Country { get; set; }
        public int? CountryID { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Город")]
        public string City { get; set; }
        public int? CityID { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Наименование образовательного учреждения (Организации)")]
        public string Institution { get; set; }
        public int? InstitutionID { get; set; }
    }

    public class EditInstitutionViewModel
    {
        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Страна")]
        public string Country { get; set; }
        public int? CountryID { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Город")]
        public string City { get; set; }
        public int? CityID { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Наименование образовательного учреждения (Организации)")]
        public string Institution { get; set; }
        public int? InstitutionID { get; set; }
    }
}
