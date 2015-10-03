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
    public class CityController : Controller
    {
        private IRepository repository;
        private readonly ILog logger = LogManager.GetLogger(typeof(InstitutionController));
        
        /// <summary>
        /// Controller constructor.
        /// Initialize repository.
        /// </summary>
        /// <param name="Repository">Repository object</param>
        public CityController(IRepository Repository)
        {
            XmlConfigurator.Configure();
            repository = Repository;
        }

        public ActionResult Index(int Page = 1, int PageSize = 25, string FilterBy = "all", string SearchTerm = null)
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + " \"" + User.Identity.Name +
                "\" visited AccountsManagement/City/Index");

            ManageCityViewModel viewModel = new ManageCityViewModel();
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
                        viewModel.PaginatedCityList = repository.City
                            .OrderByDescending(c => c.CityID)
                            .ToPaginatedList<City>(Page, PageSize);
                    }
                    else if (!string.IsNullOrEmpty(SearchTerm))
                    {
                        if (FilterBy == "name")
                        {
                            viewModel.PaginatedCityList = repository.City
                                .Where(c => c.Name.ToLower().IndexOf(SearchTerm.ToLower()) != -1)
                                .OrderByDescending(c => c.CityID)
                                .ToPaginatedList<City>(Page, PageSize);
                        }
                        else if (FilterBy == "area")
                        {
                            viewModel.PaginatedCityList = repository.City
                                .Where(c => c.Area.ToLower().IndexOf(SearchTerm.ToLower()) != -1)
                                .OrderByDescending(c => c.CityID)
                                .ToPaginatedList<City>(Page, PageSize);
                        }
                        else if (FilterBy == "region")
                        {
                            viewModel.PaginatedCityList = repository.City
                                .Where(c => c.Region.ToLower().IndexOf(SearchTerm.ToLower()) != -1)
                                .OrderByDescending(c => c.CityID)
                                .ToPaginatedList<City>(Page, PageSize);
                        }
                        //else if (FilterBy == "city")
                        //{
                        //    viewModel.PaginatedCityList = repository.City
                        //        .Where(i => i.City.Name.ToLower().IndexOf(SearchTerm.ToLower()) != -1)
                        //        .OrderByDescending(i => i.InstitutionID)
                        //        .ToPaginatedList<Institution>(Page, PageSize);
                        //}
                    }
                }
                else
                {
                    //if (FilterBy == "all" || string.IsNullOrEmpty(SearchTerm))
                    //{
                    //    viewModel.PaginatedCityList = repository.City
                    //        .Where(i => i.UsersCanModify.FirstOrDefault(u => u.UserId == WebSecurity.CurrentUserId) != null)
                    //        .OrderByDescending(i => i.CityID)
                    //        .ToPaginatedList<Institution>(Page, PageSize);
                    //}
                    //else if (!string.IsNullOrEmpty(SearchTerm))
                    //{
                    //    if (FilterBy == "name")
                    //    {
                    //        viewModel.PaginatedInstitutionList = repository.Institutions
                    //            .Where(i => i.UsersCanModify.FirstOrDefault(u => u.UserId == WebSecurity.CurrentUserId) != null)
                    //            .Where(i => i.Name.ToLower().IndexOf(SearchTerm.ToLower()) != -1)
                    //            .OrderByDescending(i => i.InstitutionID)
                    //            .ToPaginatedList<Institution>(Page, PageSize);
                    //    }
                    //    //else if (FilterBy == "city")
                    //    //{
                    //    //    viewModel.PaginatedInstitutionList = repository.Institutions
                    //    //        .Where(i => i.UsersCanModify.FirstOrDefault(u => u.UserId == WebSecurity.CurrentUserId) != null)
                    //    //        .Where(i => i.City.Name.ToLower().IndexOf(SearchTerm.ToLower()) != -1)
                    //    //        .OrderByDescending(i => i.InstitutionID)
                    //    //        .ToPaginatedList<Institution>(Page, PageSize);
                    //    //}
                    //}
                }
            }

            return View(viewModel);
        }

        public ActionResult Create()
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + " \"" + User.Identity.Name +
                "\" visited AccountsManagement/City/Create");

            var model = new viewModels.CreateCityViewModel();
            return View(model);
        }

        /// <summary>
        /// This method redirects to the GrantRolesToUser method.
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(viewModels.CreateCityViewModel Model)
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
                        .FirstOrDefault(c => c.Name == Model.City && c.Area == Model.Area && c.Region == Model.Region && c.CountryID == country.CountryID);
                    if (city != null)
                    {
                        ModelState.AddModelError("City", "Город (населенный пункт) с таким именем уже существует");
                        return View(Model);
                    }

                    city = new City()
                        {
                            Name = Model.City,
                            Area = Model.Area,
                            Region = Model.Region,
                            CountryID = country.CountryID
                        };

                    int userID = WebSecurity.CurrentUserId;
                    if (userID != 1)
                    {
                        UserProfile user = repository.Users.FirstOrDefault(u => u.UserId == userID);
                        //institution.UsersCanModify = new List<UserProfile>();
                        //institution.UsersCanModify.Add(user);
                    }
                    int cityID = repository.AddCity(city);

                    logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) + " \"" + User.Identity.Name +
                        "\" create city \"" + cityID + "\"");
                    TempData["SuccessMessage"] = "Город (населенный пункт) успешно создан!";
                    return RedirectToAction("Create");
                }
                catch (MembershipCreateUserException ex)
                {
                    logger.Warn("Error occured on user " + WebSecurity.GetUserId(User.Identity.Name) + 
                        " \"" + User.Identity.Name + "\" city creating: ", ex);
                    TempData["ErrorMessage"] = "Произошла ошибка при создании города (населенного пункта)";
                }
            }

            return View(Model);
        }

        #region View City Details Methods

        public ActionResult Update(int CityID = -1)
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) +
                " \"" + User.Identity.Name + "\" visited AccountsManagement/City/Update");

            City city = repository.City.FirstOrDefault(c => c.CityID == CityID);

            if (city == null)
            {
                logger.Warn("City with id = " + CityID + " not found");
                throw new HttpException(404, "City not found");
            }

            var model = new viewModels.EditCityViewModel()
            {
                Country = city.Country.Name,
                CountryID = city.CountryID,
                City = city.Name,
                CityID = city.CityID,
                Area = city.Area,
                Region = city.Region
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Update(viewModels.EditCityViewModel Model, int CityID = -1, string Update = null, string Delete = null, string Cancel = null)
        {
            if (CityID == -1)
            {
                logger.Warn("City with id = " + CityID + " not found");
                throw new HttpException(404, "City not found");
            }

            if (Delete != null)
                return DeleteCity(CityID);
            if (Cancel != null)
                return CancelCity();
            if (Update != null)
                return UpdateCity(Model, CityID);

            return CancelCity();

        }

        private ActionResult UpdateCity(viewModels.EditCityViewModel Model, int CityID = -1)
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
                        .FirstOrDefault(c => c.Name == Model.City && c.Area == Model.Area && c.Region == Model.Region && c.CityID != CityID);
                    if (city != null)
                    {
                        ModelState.AddModelError("City", "Город (населенный пункт) с таким именем уже существует");
                        return View(Model);
                    }

                    city = repository
                        .City
                        .FirstOrDefault(c => c.CityID == Model.CityID);
                    if (city == null)
                    {
                        TempData["ErrorMessage"] = "Произошла ошибка при обновлении города (населенного пункта)";
                        logger.Warn("Error occured on user " + WebSecurity.GetUserId(User.Identity.Name) +
                            " \"" + User.Identity.Name + "\" city updating: city with id " + Model.CityID + " not exist");
                        return View(Model);
                    }

                    city.Name = Model.City;
                    city.Area = Model.Area;
                    city.Region = Model.Region;
                    city.CountryID = country.CountryID;

                    repository.AddCity(city);

                    logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) +
                        " \"" + User.Identity.Name + "\" update city \"" + Model.CityID + "\"");
                    TempData["SuccessMessage"] = "Город (населенный пункт) успешно обновлен!";
                    return RedirectToAction("Update", new { CityID = Model.CityID });
                }
                catch (MembershipCreateUserException ex)
                {
                    logger.Warn("Error occured on user " + WebSecurity.GetUserId(User.Identity.Name) +
                        " \"" + User.Identity.Name + "\" city updating: ", ex);
                    TempData["ErrorMessage"] = "Произошла ошибка при обновлении города (населенного пункта)";
                }
            }

            return RedirectToAction("Update", new { CityID = CityID });
        }

        #endregion

        #region Delete City Methods

        private ActionResult DeleteCity(int CityID = -1)
        {
            if (CityID == -1)
            {
                logger.Warn("City with id = " + CityID + " not found");
                throw new HttpException(404, "City not found");
            }

            try
            {
                repository.DeleteCity(CityID);

                logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) + " \"" + User.Identity.Name +
                    "\" delete city with id = " + CityID);
                TempData["SuccessMessage"] = "Город (населенный пункт) удален";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Произошла ошибка при удалении города (населенного пункта)";
                logger.Error("Error occurred on user " + WebSecurity.GetUserId(User.Identity.Name) + " \"" + User.Identity.Name +
                    "\" deleting city with id = " + CityID + ": ", ex);
            }

            return RedirectToAction("Update", new { CityID = CityID });
        }

        #endregion

        #region Cancel City Methods

        private ActionResult CancelCity()
        {
            return RedirectToAction("Index");
        }

        #endregion
    }
}
