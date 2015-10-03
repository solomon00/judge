using Solomon.Domain.Concrete;
using Solomon.TypesExtensions;
using Solomon.TypesExtensions.ValidationAttributeExtensions;
using Solomon.WebUI.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace Solomon.WebUI.Areas.ProblemsManagement.ViewModels
{
    public class NewProblemViewModel
    {
        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Display(Name = "Тип задачи")]
        public IEnumerable<SelectListItem> ProblemTypesList { get; set; }
        public int ProblemTypesListID { get; set; }

        [Display(Name = "Теги")]
        public List<SelectListItem> ProblemTagsList { get; set; }
        public int[] ProblemTagsListIDs { get; set; }

        [RequiredIf("ProblemTypesListID", Comparison.IsNotEqualTo, (int)ProblemTypes.Open, ErrorMessage = "Обязательное поле")]
        [Display(Name = "Ограничение по времени (сек)")]
        public double TimeLimit { get; set; }

        [RequiredIf("ProblemTypesListID", Comparison.IsNotEqualTo, (int)ProblemTypes.Open, ErrorMessage = "Обязательное поле")]
        [Display(Name = "Ограничение по памяти (кб)")]
        public int MemoryLimit { get; set; }

        [AllowHtml]
        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [AllowHtml]
        [RequiredIf("ProblemTypesListID", Comparison.IsNotEqualTo, (int)ProblemTypes.Open, ErrorMessage = "Обязательное поле")]
        [Display(Name = "Формат входных данных")]
        public string InputFormat { get; set; }

        [AllowHtml]
        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Формат выходных данных")]
        public string OutputFormat { get; set; }

        [Display(Name = "Чекер")]
        public IEnumerable<SelectListItem> CheckerList { get; set; }
        public string CheckerListID { get; set; }

        [RequiredIf("CheckerListID", Comparison.IsEqualTo, "Other")]
        public HttpPostedFileBase Checker { get; set; }

        [RequiredIf("ProblemTypesListID", Comparison.IsNotEqualTo, (int)ProblemTypes.Open, ErrorMessage = "Обязательное поле")]
        [Display(Name = "Тесты (zip)")]
        public HttpPostedFileBase Tests { get; set; }

        [RequiredIf("ProblemTypesListID", Comparison.IsNotEqualTo, (int)ProblemTypes.Open, ErrorMessage = "Обязательное поле")]
        [Display(Name = "Примеры (zip)")]
        public HttpPostedFileBase Samples { get; set; }

        [RequiredIf("ProblemTypesListID", Comparison.IsEqualTo, (int)ProblemTypes.Open, ErrorMessage = "Обязательное поле")]
        [Display(Name = "Ответ (txt)")]
        public HttpPostedFileBase OpenProblemResult { get; set; }

        [Display(Name = "Отложенная проверка")]
        public bool CheckPending { get; set; }


        public NewProblemViewModel()
        {
            var repository = new EFRepository();

            TimeLimit = 2;
            MemoryLimit = 65536;

            List<SelectListItem> tempList = new List<SelectListItem>();
            StdCheckers.Checkers.ForEach(ch =>
                {
                    if (ch.Available)
                    {
                        tempList.Add(new SelectListItem()
                            {
                                Value = ch.CheckerID.ToString(),
                                Text = ch.Description
                            });
                    }
                });
            
            tempList.Add(new SelectListItem() { Text = "Другой", Value = "other" /* Must be in lower case for clientside validation*/ });

            CheckerList = tempList;

            ProblemTypesList = new List<SelectListItem>()
                {
                    new SelectListItem() { Value = ((int)ProblemTypes.Standart).ToString(), Text = "Стандартная задача (стандартные потоки ввода/вывода)" },
                    //new SelectListItem() { Value = ((int)ProblemTypes.Interactive).ToString(), Text = "Интерактивная задача (взаимодействие с программой интерактором)" },
                    new SelectListItem() { Value = ((int)ProblemTypes.Open).ToString(), Text = "Открытая задача (только ответ на задачу)" }
                };


            ProblemTagsList = new List<SelectListItem>();
            repository.ProblemTags.Each(t => ProblemTagsList.Add(new SelectListItem() { Text = t.Name, Value = t.ProblemTagID.ToString() }));
        }
    }
}
