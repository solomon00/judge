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
    public class EditProblemViewModel
    {
        public int ProblemID { get; set; }

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

        public string CheckerCode;
        [Display(Name = "Чекер")]
        public IEnumerable<SelectListItem> CheckerList { get; set; }
        public string CheckerListID { get; set; }

        [Display(Name = "Чекер")]
        [RequiredIf("CheckerListID", Comparison.IsEqualTo, "other")]
        public HttpPostedFileBase Checker { get; set; }


        [RequiredIf("ProblemTypesListID", Comparison.IsNotEqualTo, (int)ProblemTypes.Open, ErrorMessage = "Обязательное поле")]
        [Display(Name = "Тесты (zip)")]
        public IEnumerable<SelectListItem> TestsDropDown { get; set; }
        public string TestsDropDownID { get; set; }

        [Display(Name = "Тесты (zip)")]
        [RequiredIf("TestsDropDownID", Comparison.IsEqualTo, "other")]
        public HttpPostedFileBase Tests { get; set; }


        [RequiredIf("ProblemTypesListID", Comparison.IsNotEqualTo, (int)ProblemTypes.Open, ErrorMessage = "Обязательное поле")]
        [Display(Name = "Примеры (zip)")]
        public IEnumerable<SelectListItem> SamplesDropDown { get; set; }
        public string SamplesDropDownID { get; set; }

        [Display(Name = "Примеры (zip)")]
        [RequiredIf("SamplesDropDownID", Comparison.IsEqualTo, "other")]
        public HttpPostedFileBase Samples { get; set; }


        [RequiredIf("ProblemTypesListID", Comparison.IsEqualTo, (int)ProblemTypes.Open, ErrorMessage = "Обязательное поле")]
        [Display(Name = "Ответ")]
        public IEnumerable<SelectListItem> OpenProblemResultDropDown { get; set; }
        public string OpenProblemResultDropDownID { get; set; }

        [Display(Name = "Ответ (txt)")]
        [RequiredIf("ProblemTypesListID", Comparison.IsEqualTo, "other")]
        public HttpPostedFileBase OpenProblemResult { get; set; }

        [Display(Name = "Отложенная проверка")]
        public bool CheckPending { get; set; }

        public IEnumerable<string> Tournaments { get; set; }


        public EditProblemViewModel()
        {
            TimeLimit = 2;
            MemoryLimit = 65536;

            List<SelectListItem> tempList = new List<SelectListItem>()
                {
                    new SelectListItem()
                        {
                            Value = "-1",
                            Text = "Текущий",
                            Selected = true
                        }
                };
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

            TestsDropDown = new List<SelectListItem>()
                {
                    new SelectListItem() { Text = "Текущие", Value = "-1" },
                    new SelectListItem() { Text = "Другие", Value = "other" }
                };

            SamplesDropDown = new List<SelectListItem>()
                {
                    new SelectListItem() { Text = "Текущие", Value = "-1" },
                    new SelectListItem() { Text = "Другие", Value = "other" }
                };

            OpenProblemResultDropDown = new List<SelectListItem>()
                {
                    new SelectListItem() { Text = "Текущий", Value = "-1" },
                    new SelectListItem() { Text = "Другой", Value = "other" }
                };

            ProblemTagsList = new List<SelectListItem>();
        }
    }
}
