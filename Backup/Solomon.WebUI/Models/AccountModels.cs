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

namespace Solomon.WebUI.Models
{
    public partial class LoginViewModel
    {
        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Логин")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Display(Name = "Запомнить?")]
        public bool RememberMe { get; set; }

        public bool EnablePasswordReset { get; set; }
    }

    public class RegisterViewModel
    {
        //[Required(ErrorMessage = "Обязательное поле")]
        //[Display(Name = "Имя")]
        //public string FullName { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Логин")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [StringLength(100, ErrorMessage = "{0} должен быть длинее {2} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Пароль еще раз")]
        [System.Web.Mvc.Compare("Password", ErrorMessage = "Пароли не совпадают.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Секретный вопрос")]
        public string SecretQuestion { get; set; }

        [Display(Name = "Секретный ответ")]
        public string SecretAnswer { get; set; }

        [Display(Name = "Необходимо подтверждение через email")]
        public bool Approve { get; set; }

        public bool RequireSecretQuestionAndAnswer { get; set; }

        //[Display(Name = "Категория")]
        //public IEnumerable<SelectListItem> CategoryList { get; set; }
        //public string CategoryListID { get; set; }


        //public RegisterViewModel()
        //{
        //    CategoryList = new List<SelectListItem>()
        //        {
        //            new SelectListItem() { Value = ((int)UserCategory.School).ToString(), Text = "Школьник" },
        //            new SelectListItem() { Value = ((int)UserCategory.Student).ToString(), Text = "Студент" },
        //            new SelectListItem() { Value = ((int)UserCategory.Teacher).ToString(), Text = "Преподаватель" },
        //            new SelectListItem() { Value = ((int)UserCategory.Other).ToString(), Text = "Другое" }
        //        };
        //}
    }

    public class ManageProfileViewModel
    {
        [Display(Name = "Имя")]
        public string FirstName { get; set; }

        [Display(Name = "Фамилия")]
        public string SecondName { get; set; }

        [Display(Name = "Отчество")]
        public string ThirdName { get; set; }

        [Display(Name = "Дата рождения")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date, ErrorMessage = "Пожалуйста, введите дату в формате dd.mm.yyyy")]
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

        public ManageProfileViewModel()
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

    public class NewTeamViewModel
    {
        [Required(ErrorMessage = "Обязательное поле")]
        public string Name { get; set; }
    }

    public class ManageTeamViewModel
    {
        public int TeamID { get; set; }

        [Required]
        [Display(Name = "Название команды")]
        public string Name { get; set; }

        public IEnumerable<UserProfileTeam> Members { get; set; }
    }

    public class TeamViewModel
    {
        public IEnumerable<Team> Invites { get; set; }
        public IEnumerable<Team> Teams { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Текущий пароль")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} должен быть длинее {2} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль еще раз")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Пароли не совпадают.")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordModel
    {
        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Логин")]
        public string UserName { get; set; }

    }

    public class ResetPasswordConfirmModel
    {
        public string Token { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [StringLength(100, ErrorMessage = "{0} должен быть длинее {2} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль еще раз")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Пароли не совпадают.")]
        public string ConfirmPassword { get; set; }
    }

    public class RegisterExternalLoginViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }
    }

    public class ExternalLoginModel
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }
}
