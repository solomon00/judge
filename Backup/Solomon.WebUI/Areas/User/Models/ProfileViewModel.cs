﻿using Solomon.Domain.Entities;
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
    public class ProfileViewModel : UserViewModel
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

        [Display(Name = "Образов. учреждение (Организация)")]
        public string Institution { get; set; }
        public int? InstitutionID { get; set; }

        [Display(Name = "Год обучения (класс/курс)")]
        public int? GradeLevel { get; set; }

        public ProfileViewModel()
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

}
