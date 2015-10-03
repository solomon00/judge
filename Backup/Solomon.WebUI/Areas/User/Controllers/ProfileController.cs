using log4net;
using log4net.Config;
using Solomon.Domain.Abstract;
using Solomon.Domain.Entities;
using Solomon.TypesExtensions;
using Solomon.WebUI.Areas.User.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace Solomon.WebUI.Areas.User.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private IRepository repository;
        private readonly ILog logger = LogManager.GetLogger(typeof(ProfileController));
        /// <summary>
        /// Controller constructor.
        /// Initialize repository.
        /// </summary>
        /// <param name="Repository">Repository object</param>
        public ProfileController(IRepository Repository)
        {
            XmlConfigurator.Configure();
            repository = Repository;
        }

        //
        // GET: /Account/Profile/

        public ActionResult Index()
        {
            int userID = WebSecurity.GetUserId(User.Identity.Name);
            UserProfile user = repository
                .Users
                .FirstOrDefault(u => u.UserId == userID);

            Country country = repository.Country.FirstOrDefault(c => c.CountryID == user.CountryID);
            City city = repository.City.FirstOrDefault(c => c.CityID == user.CityID);
            Institution institution = repository.Institutions.FirstOrDefault(i => i.InstitutionID == user.InstitutionID);

            ProfileViewModel viewModel = new ProfileViewModel()
            {
                FirstName = user.FirstName,
                SecondName = user.SecondName,
                ThirdName = user.ThirdName,
                BirthDay = user.BirthDay,
                PhoneNumber = user.PhoneNumber,
                CategoryListID = user.Category != null ? (int)user.Category : 0,
                CountryID = city != null ? city.CountryID : user.CountryID,
                Country = city != null ? city.Country.Name : country != null ? country.Name : String.Empty,
                CityID = user.CityID,
                City = city != null ? city.Name : String.Empty,
                InstitutionID = user.InstitutionID,
                Institution = institution != null ? institution.Name : String.Empty,
                GradeLevel = user.GradeLevel
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Index(ProfileViewModel Model)
        {
            if (ModelState.IsValid)
            {
                int userID = WebSecurity.CurrentUserId;
                UserProfile user = repository
                    .Users
                    .FirstOrDefault(u => u.UserId == userID);
                Country country = repository
                    .Country
                    .FirstOrDefault(c => c.CountryID == Model.CountryID);

                int? countryID = null;
                if (country != null)
                {
                    countryID = country.CountryID;
                }
                else if (Model.Country != null)
                {
                    ModelState.AddModelError("Country", "Страна не существует в базе");
                }

                City city = repository
                    .City
                    .FirstOrDefault(c => c.CityID == Model.CityID);

                int? cityID = null;
                if (city != null)
                {
                    cityID = city.CityID;
                }
                else if (Model.City != null)
                {
                    ModelState.AddModelError("City", "Город не существует в базе");
                }

                Institution institution = repository
                    .Institutions
                    .FirstOrDefault(i => i.InstitutionID == Model.InstitutionID);

                int? institutionID = null;
                if (institution != null)
                {
                    institutionID = institution.InstitutionID;
                }
                else if (Model.Institution != null)
                {
                    ModelState.AddModelError("Institution", "Образовательное учреждение не существует в базе");
                }

                if (user == null)
                {
                    logger.Warn("User with id = " + userID + " not exist in database");
                    TempData["ErrorMessage"] = "Пользователь не существует в базе";
                    return View(Model);
                }

                logger.Info("User " + user.UserId + " change accaunt information \nSecondName \"" +
                    user.SecondName + "\" -> \"" + Model.SecondName + "\"\nFirstName \"" +
                    user.FirstName + "\" -> \"" + Model.FirstName + "\"\nThirdName \"" +
                    user.ThirdName + "\" -> \"" + Model.ThirdName + "\"\nCategory \"" +
                    user.Category + "\" -> \"" + (UserCategories)Model.CategoryListID + "\"\nCity \"" +
                    user.CityID + "\" -> \"" + Model.CityID + "\"\nInstitution \"" +
                    user.InstitutionID + "\" -> \"" + Model.InstitutionID + "\"\nGradeLevel \"" +
                    user.GradeLevel + "\" -> \"" + Model.GradeLevel + "\"");

                user.FirstName = Model.FirstName;
                user.SecondName = Model.SecondName;
                user.ThirdName = Model.ThirdName;
                user.BirthDay = Model.BirthDay;
                user.PhoneNumber = Model.PhoneNumber;
                user.Category = (UserCategories)Model.CategoryListID;
                user.CountryID = countryID;
                user.CityID = cityID;
                user.InstitutionID = institutionID;
                user.GradeLevel = Model.GradeLevel;

                repository.UpdateUserProfile(user);

                TempData["SuccessMessage"] = "Данные успешно обновлены";
                return View(Model);
            }

            // If we got this far, something failed, redisplay form
            TempData["ErrorMessage"] = "Произошла ошибка при обновлении данных";
            return View(Model);
        }

    }
}
