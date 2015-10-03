using Solomon.Domain.Entities;
using Solomon.TypesExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace Solomon.WebUI.Areas.AccountsManagement.ViewModels
{
    public class GenerateUsersViewModel
    {
        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Шаблон логина")]
        public string UserNameTemplate { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Имена пользователей")]
        public HttpPostedFileBase Titles { get; set; }
    }

    public class Account
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        [Display(Name = "Имя")]
        public string FirstName { get; set; }

        [Display(Name = "Фамилия")]
        public string SecondName { get; set; }

        [Display(Name = "Отчество")]
        public string ThirdName { get; set; }

        [Display(Name = "Дата рождения")]
        public DateTime? BirthDay { get; set; }

        [Display(Name = "Номер телефона")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Категория")]
        public IEnumerable<SelectListItem> CategoryList { get; set; }
        public int CategoryListID { get; set; }

        [Display(Name = "Страна")]
        public string Country { get; set; }
        public int? CountryID { get; set; }

        [Display(Name = "Город")]
        public string City { get; set; }
        public int? CityID { get; set; }

        [Display(Name = "Образовательное учреждение (Организация)")]
        public string Institution { get; set; }
        public int? InstitutionID { get; set; }

        [Display(Name = "Год обучения (класс/курс)")]
        public int? GradeLevel { get; set; }

        public Account()
        {
            CategoryList = new List<SelectListItem>()
                {
                    new SelectListItem() { Value = ((int)UserCategories.None).ToString(), Text = "Не выбрано" },
                    new SelectListItem() { Value = ((int)UserCategories.School).ToString(), Text = "Школьник" },
                    new SelectListItem() { Value = ((int)UserCategories.Student).ToString(), Text = "Студент" },
                    new SelectListItem() { Value = ((int)UserCategories.Teacher).ToString(), Text = "Преподаватель" },
                    new SelectListItem() { Value = ((int)UserCategories.Other).ToString(), Text = "Другое" }
                };
        }
    }

    public class GeneratedUsersListViewModel
    {
        public bool Generated { get; set; }
        public bool RegisteredForTournament { get; set; }
        public string TournamentName { get; set; }

        public List<Account> Accounts { get; set; }
        public Account AccountTemplate { get; set; }

        [Display(Name = "Зарегистрировать участников на турнир")]
        public IEnumerable<SelectListItem> TournamentList { get; set; }
        public int TournamentID { get; set; }
    }
}
