using Solomon.Domain.Entities;
using Solomon.TypesExtensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Security;

namespace Solomon.WebUI.Areas.AccountsManagement.ViewModels
{
    public class ManageCityViewModel
    {
        public PaginatedList<City> PaginatedCityList { get; set; }
        public string FilterBy { get; set; }
        public string SearchTerm { get; set; }
        public int PageSize { get; set; }
    }

    public class CreateCityViewModel
    {
        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Страна")]
        public string Country { get; set; }
        public int? CountryID { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Название города")]
        public string City { get; set; }
        public int? CityID { get; set; }

        [Display(Name = "Район")]
        public string Area { get; set; }
        [Display(Name = "Область")]
        public string Region { get; set; }
    }

    public class EditCityViewModel
    {
        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Страна")]
        public string Country { get; set; }
        public int? CountryID { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Название города")]
        public string City { get; set; }
        public int? CityID { get; set; }

        [Display(Name = "Район")]
        public string Area { get; set; }
        [Display(Name = "Область")]
        public string Region { get; set; }
    }
}
