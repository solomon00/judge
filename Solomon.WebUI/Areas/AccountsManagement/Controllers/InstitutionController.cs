using Solomon.WebUI.Areas.AccountsManagement.ViewModels;
using Solomon.Domain.Abstract;
using System.Web.Mvc;
using System.Linq;
using Solomon.Domain.Entities;
using System.Web.Security;
using System.Web;
using Solomon.TypesExtensions;
using log4net;
using log4net.Config;
using viewModels = Solomon.WebUI.Areas.AccountsManagement.ViewModels;
using System;
using WebMatrix.WebData;
using System.Collections.Generic;

namespace Solomon.WebUI.Areas.AccountsManagement.Controllers
{
    [Authorize(Roles = "Judge, Administrator")]
    public class InstitutionController : Controller
    {
        private IRepository repository;
        private readonly ILog logger = LogManager.GetLogger(typeof(InstitutionController));
        
        /// <summary>
        /// Controller constructor.
        /// Initialize repository.
        /// </summary>
        /// <param name="Repository">Repository object</param>
        public InstitutionController(IRepository Repository)
        {
            XmlConfigurator.Configure();
            repository = Repository;
        }

        public ActionResult Index(int Page = 1, int PageSize = 25, string FilterBy = "all", string SearchTerm = null)
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + " \"" + User.Identity.Name + 
                "\" visited AccountsManagement/Institution/Index");

            ManageInstitutionsViewModel viewModel = new ManageInstitutionsViewModel();
            viewModel.FilterBy = FilterBy;
            viewModel.SearchTerm = SearchTerm;

            if (System.Web.HttpContext.Current.Request.HttpMethod == "POST") 
            {
                Page = 1;
            }

            if (PageSize == 0)
                PageSize = 25;

            viewModel.PageSize = PageSize;

            if (!string.IsNullOrEmpty(FilterBy))
            {
                UserProfile user = repository.Users.FirstOrDefault(u => u.UserId == WebSecurity.CurrentUserId);

                if (Roles.IsUserInRole("Administrator"))
                {
                    if (FilterBy == "all" || string.IsNullOrEmpty(SearchTerm))
                    {
                        viewModel.PaginatedInstitutionList = repository.Institutions
                            .OrderByDescending(i => i.InstitutionID)
                            .ToPaginatedList<Institution>(Page, PageSize);
                    }
                    else if (!string.IsNullOrEmpty(SearchTerm))
                    {
                        if (FilterBy == "name")
                        {
                            viewModel.PaginatedInstitutionList = repository.Institutions
                                .Where(i => i.Name.ToLower().IndexOf(SearchTerm.ToLower()) != -1)
                                .OrderByDescending(i => i.InstitutionID)
                                .ToPaginatedList<Institution>(Page, PageSize);
                        }
                        else if (FilterBy == "city")
                        {
                            viewModel.PaginatedInstitutionList = repository.Institutions
                                .Where(i => i.City.Name.ToLower().IndexOf(SearchTerm.ToLower()) != -1)
                                .OrderByDescending(i => i.InstitutionID)
                                .ToPaginatedList<Institution>(Page, PageSize);
                        }
                    }
                }
                else
                {
                    if (FilterBy == "all" || string.IsNullOrEmpty(SearchTerm))
                    {
                        viewModel.PaginatedInstitutionList = repository.Institutions
                            .Where(i => i.UsersCanModify.FirstOrDefault(u => u.UserId == WebSecurity.CurrentUserId) != null)
                            .OrderByDescending(i => i.InstitutionID)
                            .ToPaginatedList<Institution>(Page, PageSize);
                    }
                    else if (!string.IsNullOrEmpty(SearchTerm))
                    {
                        if (FilterBy == "name")
                        {
                            viewModel.PaginatedInstitutionList = repository.Institutions
                                .Where(i => i.UsersCanModify.FirstOrDefault(u => u.UserId == WebSecurity.CurrentUserId) != null)
                                .Where(i => i.Name.ToLower().IndexOf(SearchTerm.ToLower()) != -1)
                                .OrderByDescending(i => i.InstitutionID)
                                .ToPaginatedList<Institution>(Page, PageSize);
                        }
                        else if (FilterBy == "city")
                        {
                            viewModel.PaginatedInstitutionList = repository.Institutions
                                .Where(i => i.UsersCanModify.FirstOrDefault(u => u.UserId == WebSecurity.CurrentUserId) != null)
                                .Where(i => i.City.Name.ToLower().IndexOf(SearchTerm.ToLower()) != -1)
                                .OrderByDescending(i => i.InstitutionID)
                                .ToPaginatedList<Institution>(Page, PageSize);
                        }
                    }
                }
            }

            return View(viewModel);
        }

        public ActionResult Create()
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + " \"" + User.Identity.Name + 
                "\" visited AccountsManagement/Institution/Create");

            var model = new viewModels.CreateInstitutionViewModel();
            return View(model);
        }

        /// <summary>
        /// This method redirects to the GrantRolesToUser method.
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(viewModels.CreateInstitutionViewModel Model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    Country country = repository
                        .Country
                        .FirstOrDefault(c => c.CountryID == Model.CountryID);
                    if (country == null)
                    {
                        ModelState.AddModelError("Country", "Страна не существует в базе");
                        return View(Model);
                    }

                    City city = repository
                        .City
                        .FirstOrDefault(c => c.CityID == Model.CityID && c.CountryID == Model.CountryID);
                    if (city == null)
                    {
                        ModelState.AddModelError("City", "Город не существует в базе");
                        return View(Model);
                    }

                    Institution institution = repository
                        .Institutions
                        .FirstOrDefault(i => i.Name == Model.Institution && i.CityID == city.CityID);
                    if (institution != null)
                    {
                        ModelState.AddModelError("Institution", "Образовательное учреждение с таким именем уже существует");
                        return View(Model);
                    }

                    institution = new Institution()
                        {
                            Name = Model.Institution,
                            CityID = city.CityID
                        };

                    int userID = WebSecurity.CurrentUserId;
                    if (userID != 1)
                    {
                        UserProfile user = repository.Users.FirstOrDefault(u => u.UserId == userID);
                        institution.UsersCanModify = new List<UserProfile>();
                        institution.UsersCanModify.Add(user);
                    }
                    int institutionID = repository.AddInstitution(institution);

                    logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) + " \"" + User.Identity.Name + 
                        "\" create institution \"" + institutionID + "\"");
                    TempData["SuccessMessage"] = "Образовательное учреждение (организация) успешно создано(а)!";
                    return RedirectToAction("Create");
                }
                catch (MembershipCreateUserException ex)
                {
                    logger.Warn("Error occured on user " + WebSecurity.GetUserId(User.Identity.Name) + 
                        " \"" + User.Identity.Name + "\" institution creating: ", ex);
                    TempData["ErrorMessage"] = "Произошла ошибка при создании образовательного учреждения (организации)";
                }
            }

            return View(Model);
        }

        ///// <summary>
        ///// This method redirects to the GrantRolesToUser method.
        ///// </summary>
        ///// <param name="Model"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public ActionResult Update(viewModels.EditInstitutionViewModel Model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // Attempt to register the user
        //        try
        //        {
        //            City city = repository
        //                .City
        //                .FirstOrDefault(c => c.Name == Model.City);
        //            if (city == null)
        //            {
        //                ModelState.AddModelError("City", "Город не существует в базе");
        //                return View(Model);
        //            }

        //            Institution institution = repository
        //                .Institutions
        //                .FirstOrDefault(i => i.Name == Model.Name && i.CityID == city.CityID && i.InstitutionID != Model.InstitutionID);
        //            if (institution != null)
        //            {
        //                ModelState.AddModelError("Name", "Образовательное учреждение с таким именем уже существует");
        //                return View(Model);
        //            }

        //            institution = repository
        //                .Institutions
        //                .FirstOrDefault(i => i.InstitutionID == Model.InstitutionID);
        //            if (institution == null)
        //            {
        //                TempData["ErrorMessage"] = "Произошла ошибка при обновлении образовательного учреждения (организации)";
        //                logger.Warn("Error occured on user " + WebSecurity.GetUserId(User.Identity.Name) + 
        //" \"" + User.Identity.Name + "\" institution updating: institution with id " + Model.InstitutionID + " not exist");
        //                return View(Model);
        //            }

        //            institution.Name = Model.Name;
        //            institution.CityID = city.CityID;

        //            repository.AddInstitution(institution);

        //            logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) + "
        //\"" + User.Identity.Name + "\" update institution \"" + Model.InstitutionID + "\"");
        //            TempData["SuccessMessage"] = "Образовательное учреждение (организация) успешно обновлено(а)!";
        //            return RedirectToAction("Update", new { InstitutionID = Model.InstitutionID });
        //        }
        //        catch (MembershipCreateUserException ex)
        //        {
        //            logger.Warn("Error occured on user " + WebSecurity.GetUserId(User.Identity.Name) + 
        //" \"" + User.Identity.Name + "\" institution updating: ", ex);
        //            TempData["ErrorMessage"] = "Произошла ошибка при обновлении образовательного учреждения (организации)";
        //        }
        //    }

        //    return View(Model);
        //}

        #region View Institution Details Methods

        public ActionResult Update(int InstitutionID = -1)
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited AccountsManagement/Institution/Update");

            Institution institution = repository.Institutions.FirstOrDefault(i => i.InstitutionID == InstitutionID);

            if (institution == null)
            {
                logger.Warn("Institution with id = " + InstitutionID + " not found");
                throw new HttpException(404, "Institution not found");
            }

            var model = new viewModels.EditInstitutionViewModel()
            {
                Country = institution.City.Country.Name,
                CountryID = institution.City.CountryID,
                City = institution.City.Name,
                CityID = institution.CityID,
                InstitutionID = institution.InstitutionID,
                Institution = institution.Name
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Update(viewModels.EditInstitutionViewModel Model, int InstitutionID = -1, string Update = null, string Delete = null, string Cancel = null)
        {
            if (InstitutionID == -1)
            {
                logger.Warn("Institution with id = " + InstitutionID + " not found");
                throw new HttpException(404, "Institution not found");
            }

            if (Delete != null)
                return DeleteInstitution(InstitutionID);
            if (Cancel != null)
                return CancelInstitution();
            if (Update != null)
                return UpdateInstitution(Model, InstitutionID);

            return CancelInstitution();

        }

        private ActionResult UpdateInstitution(viewModels.EditInstitutionViewModel Model, int InstitutionID = -1)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    Country country = repository
                        .Country
                        .FirstOrDefault(c => c.CountryID == Model.CountryID);
                    if (country == null)
                    {
                        ModelState.AddModelError("Country", "Страна не существует в базе");
                        return View(Model);
                    }

                    City city = repository
                        .City
                        .FirstOrDefault(c => c.CityID == Model.CityID);
                    if (city == null)
                    {
                        ModelState.AddModelError("City", "Город не существует в базе");
                        return View(Model);
                    }

                    Institution institution = repository
                        .Institutions
                        .FirstOrDefault(i => i.Name == Model.Institution && i.CityID == city.CityID && i.InstitutionID != Model.InstitutionID);
                    if (institution != null)
                    {
                        ModelState.AddModelError("Institution", "Образовательное учреждение с таким именем уже существует");
                        return View(Model);
                    }

                    institution = repository
                        .Institutions
                        .FirstOrDefault(i => i.InstitutionID == Model.InstitutionID);
                    if (institution == null)
                    {
                        TempData["ErrorMessage"] = "Произошла ошибка при обновлении образовательного учреждения (организации)";
                        logger.Warn("Error occured on user " + WebSecurity.GetUserId(User.Identity.Name) + 
                            " \"" + User.Identity.Name + "\" institution updating: institution with id " + Model.InstitutionID + " not exist");
                        return View(Model);
                    }

                    institution.Name = Model.Institution;
                    institution.CityID = city.CityID;

                    repository.AddInstitution(institution);

                    logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                        " \"" + User.Identity.Name + "\" update institution \"" + Model.InstitutionID + "\"");
                    TempData["SuccessMessage"] = "Образовательное учреждение (организация) успешно обновлено(а)!";
                    return RedirectToAction("Update", new { InstitutionID = Model.InstitutionID });
                }
                catch (MembershipCreateUserException ex)
                {
                    logger.Warn("Error occured on user " + WebSecurity.GetUserId(User.Identity.Name) + 
                        " \"" + User.Identity.Name + "\" institution updating: ", ex);
                    TempData["ErrorMessage"] = "Произошла ошибка при обновлении образовательного учреждения (организации)";
                }
            }

            return RedirectToAction("Update", new { InstitutionID = InstitutionID });
        }

        #endregion

        #region Delete Institution Methods

        private ActionResult DeleteInstitution(int InstitutionID = -1)
        {
            if (InstitutionID == -1)
            {
                logger.Warn("Institution with id = " + InstitutionID + " not found");
                throw new HttpException(404, "Institution not found");
            }

            try
            {
                repository.DeleteInstitution(InstitutionID);

                logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) + " \"" + User.Identity.Name + 
                    "\" delete institution with id = " + InstitutionID);
                TempData["SuccessMessage"] = "Образовательное учреждение (организация) удалено(а)";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Произошла ошибка при удалении образовательного учреждения (организации)";
                logger.Error("Error occurred on user " + WebSecurity.GetUserId(User.Identity.Name) + " \"" + User.Identity.Name + 
                    "\" deleting institution with id = " + InstitutionID + ": ", ex);
            }

            return RedirectToAction("Update", new { InstitutionID = InstitutionID });
        }

        #endregion

        #region Cancel Institution Methods

        private ActionResult CancelInstitution()
        {
            return RedirectToAction("Index");
        }

        #endregion
    }
}
