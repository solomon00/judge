using System;
using System.Transactions;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using Solomon.WebUI.Models;
using Solomon.WebUI.Mailers;
using System.Collections.Generic;
using Solomon.Domain.Concrete;
using Solomon.Domain.Entities;
using System.Linq;
using DotNetOpenAuth.AspNet;
using Solomon.Domain.Abstract;
using log4net;
using log4net.Config;
using Solomon.TypesExtensions;
using System.Web;
using System.Data.Entity;
using System.IO;
using System.Web.Routing;

namespace Solomon.WebUI.Controllers
{
    //TODO: Remove all external logins or configure them
    [Authorize]
    //[InitializeSimpleMembership]
    public class AccountController : Controller
    {
        private IRepository repository;
        private IUserMailer userMailer;
        private readonly ILog logger = LogManager.GetLogger(typeof(AccountController));
        
        /// <summary>
        /// Controller constructor.
        /// Initialize repository.
        /// </summary>
        /// <param name="Repository">Repository object</param>
        public AccountController(IRepository Repository, IUserMailer UserMailer)
        {
            XmlConfigurator.Configure();
            repository = Repository;
            userMailer = UserMailer;
        }

        #region Login / Register
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl = null)
        {
            //if (AreaName(returnUrl) == "AccountsManagement")
            //{
            //    return RedirectToAction("UnauthorizedAccess", "Errors");
            //}
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                int userID = WebSecurity.GetUserId(model.UserName);
                UserProfile user = repository.Users.FirstOrDefault(u => u.UserId == userID);

                logger.Info("User \"" + model.UserName + "\" login with password \"" + model.Password + "\",\n return url: \"" + returnUrl + "\"");

                if (user.LastAccessTime == null)
                {
                    return RedirectToAction("Index", "Profile", new { Area = "User", UserName = user.UserName });
                }

                return RedirectToLocal(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            logger.Info("Login failed with username: \"" + model.UserName + "\" password: \"" + model.Password + "\", return url: \"" + returnUrl + "\"");
            ModelState.AddModelError("", "Имя пользователя или пароль неверен.");
            return View(model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" logout");

            WebSecurity.Logout();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel Model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    string confirmationToken = 
                        WebSecurity.CreateUserAndAccount(
                            Model.UserName, Model.Password, new { Email = Model.Email }, true);

                    Roles.AddUserToRole(Model.UserName, "User");

                    userMailer.RegisterConfirmation(Model.Email, Model.UserName, confirmationToken).Send();

                    logger.Info("Send register confirm email to \"" + Model.UserName + "\"");

                    return RedirectToAction("RegisterStepTwo", "Account");
                }
                catch (MembershipCreateUserException ex)
                {
                    logger.Warn("Registration failed with error: \"" + ErrorCodeToString(ex.StatusCode) + "\"");
                    ModelState.AddModelError("", ErrorCodeToString(ex.StatusCode));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(Model);
        }

        [AllowAnonymous]
        public ActionResult RegisterStepTwo()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult RegisterConfirmation(string Id)
        {
            if (WebSecurity.ConfirmAccount(Id))
            {
                logger.Info("Account confirmed with token: \"" + Id + "\"");
                return RedirectToAction("ConfirmationSuccess");
            }

            logger.Info("Account confirmation failed with token: \"" + Id + "\"");
            return RedirectToAction("ConfirmationFailure");
        }

        [AllowAnonymous]
        public ActionResult ConfirmationSuccess()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ConfirmationFailure()
        {
            return View();
        }

        /// <summary>
        /// An Ajax method to check if a username is unique.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult CheckForUniqueUser(string UserName)
        {
            UserProfile user = repository.Users.FirstOrDefault(u => u.UserName == UserName);
            JsonResponse response = new JsonResponse();
            response.Exists = (user == null) ? false : true;

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// An Ajax method to check if a email is unique.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult CheckForUniqueEmail(string Email)
        {
            UserProfile user = repository.Users.Where(u => u.Email == Email).FirstOrDefault();
            JsonResponse response = new JsonResponse();
            response.Exists = (user == null) ? false : true;

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Password reset

        [AllowAnonymous]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            UserProfile user = repository.Users.FirstOrDefault(u => u.UserName == model.UserName);
            string emailAddress = user == null ? null : user.Email;

            try
            {
                if (!string.IsNullOrEmpty(emailAddress))
                {
                    string confirmationToken =
                        WebSecurity.GeneratePasswordResetToken(model.UserName);

                    userMailer.PasswordReset(emailAddress, model.UserName, confirmationToken).Send();

                    logger.Info("Send reset password email to \"" + model.UserName + "\"");

                    return RedirectToAction("ResetPasswordStepTwo");
                }
            }
            catch (Exception) { }

            return RedirectToAction("InvalidUserName");
        }

        [AllowAnonymous]
        public ActionResult InvalidUserName()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordStepTwo()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult ResetPasswordConfirmation(ResetPasswordConfirmModel model)
        {
            if (ModelState.IsValid)
            {
                if (WebSecurity.ResetPassword(model.Token, model.NewPassword))
                {
                    logger.Info("Password reset with token: " + model.Token + ". New password: " + model.NewPassword);

                    return RedirectToAction("PasswordResetSuccess");
                }
                return RedirectToAction("PasswordResetFailure");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation(string Id)
        {
            ResetPasswordConfirmModel model = new ResetPasswordConfirmModel() { Token = Id };
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult PasswordResetFailure()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult PasswordResetSuccess()
        {
            return View();
        }

        #endregion

        #region Manage

        //
        // POST: /Account/Disassociate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        ////
        //// GET: /Account/Manage

        //public ActionResult Manage(ManageMessageId? message)
        //{
        //    ViewBag.StatusMessage =
        //        message == ManageMessageId.ChangePasswordSuccess ? "Пароль успешно изменен."
        //        : message == ManageMessageId.SetPasswordSuccess ? "Пароль успешно установлен."
        //        : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
        //        : "";
        //    ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
        //    ViewBag.ReturnUrl = Url.Action("Manage");
        //    return View();
        //}

        ////
        //// POST: /Account/Manage

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Manage(ChangePasswordViewModel model)
        //{
        //    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
        //    ViewBag.HasLocalPassword = hasLocalAccount;
        //    ViewBag.ReturnUrl = Url.Action("Manage");
        //    if (hasLocalAccount)
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            // ChangePassword will throw an exception rather than return false in certain failure scenarios.
        //            bool changePasswordSucceeded;
        //            try
        //            {
        //                changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
        //            }
        //            catch (Exception)
        //            {
        //                changePasswordSucceeded = false;
        //            }

        //            if (changePasswordSucceeded)
        //            {
        //                logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) + 
        //" \"" + User.Identity.Name + "\" successfully change password");
        //                return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
        //            }
        //            else
        //            {
        //                logger.Info("Password changing for user " + WebSecurity.GetUserId(User.Identity.Name) + 
        //" \"" + User.Identity.Name + "\" failed with error: \"" + "Текущий пароль неверен или новый пароль некорректен." + "\"");
        //                ModelState.AddModelError("", "Текущий пароль неверен или новый пароль некорректен.");
        //            }
        //        }
        //    }
        //    else
        //    {
        //        // User does not have a local password so remove any validation errors caused by a missing
        //        // OldPassword field
        //        ModelState state = ModelState["OldPassword"];
        //        if (state != null)
        //        {
        //            state.Errors.Clear();
        //        }

        //        if (ModelState.IsValid)
        //        {
        //            try
        //            {
        //                WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
        //                return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
        //            }
        //            catch (Exception e)
        //            {
        //                ModelState.AddModelError("", e);
        //            }
        //        }
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return View(model);
        //}

        //
        // GET: /Account/Manage

        //public ActionResult Manage()
        //{
        //    int userID = WebSecurity.GetUserId(User.Identity.Name);
        //    UserProfile user = repository
        //        .Users
        //        .FirstOrDefault(u => u.UserId == userID);

        //    Country country = repository.Country.FirstOrDefault(c => c.CountryID == user.CountryID);
        //    City city = repository.City.FirstOrDefault(c => c.CityID == user.CityID);
        //    Institution institution = repository.Institutions.FirstOrDefault(i => i.InstitutionID == user.InstitutionID);

        //    ManageProfileViewModel viewModel = new ManageProfileViewModel()
        //        {
        //            FirstName = user.FirstName,
        //            SecondName = user.SecondName,
        //            ThirdName = user.ThirdName,
        //            BirthDay = user.BirthDay,
        //            PhoneNumber = user.PhoneNumber,
        //            CategoryListID = user.Category != null ? (int)user.Category : 0,
        //            CountryID = city != null ? city.CountryID : user.CountryID,
        //            Country = city != null ? city.Country.Name : country != null ? country.Name : String.Empty,
        //            CityID = user.CityID,
        //            City = city != null ? city.Name : String.Empty,
        //            InstitutionID = user.InstitutionID,
        //            Institution = institution != null ? institution.Name : String.Empty,
        //            GradeLevel = user.GradeLevel
        //        };

        //    return View(viewModel);
        //}

        ////
        //// POST: /Account/Manage

        //public JsonResult Country(string term, int limit)
        //{
        //    var country = repository
        //        .Country
        //        .Where(c => c.Name.Contains(term))
        //        .Take(limit)
        //        .Select(c => new { value = c.CountryID, label = c.Name });

        //    return this.Json(country, JsonRequestBehavior.AllowGet);
        //}
        //public JsonResult City(string term, int limit, int countryID = 0)
        //{
        //    var city = repository
        //        .City
        //        .Where(c => c.Name.Contains(term) && c.CountryID == countryID)
        //        .OrderByDescending(c => c.Important)
        //        .Take(limit)
        //        .Select(c => new
        //        {
        //            value = c.CityID,
        //            label = ((c.Important == 1 ? "<b>" + c.Name + "</b>" : c.Name) + 
        //                "<br/><span style=\"color: grey;\">" + 
        //                (!String.IsNullOrEmpty(c.Region) ? c.Region : String.Empty) + 
        //                (!String.IsNullOrEmpty(c.Area) ? ", " + c.Area : String.Empty) + "</span>")
        //        });

        //    return this.Json(city, JsonRequestBehavior.AllowGet);
        //}
        //public JsonResult Institution(string term, int limit, int cityID = 0)
        //{
        //    var institutions = repository
        //        .Institutions
        //        .Where(i => i.Name.Contains(term) && i.CityID == cityID)
        //        .Take(limit)
        //        .Select(i => new { value = i.InstitutionID, label = i.Name });

        //    return this.Json(institutions, JsonRequestBehavior.AllowGet);
        //}

        //[HttpPost]
        //public ActionResult Manage(ManageProfileViewModel Model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        int userID = WebSecurity.CurrentUserId;
        //        UserProfile user = repository
        //            .Users
        //            .FirstOrDefault(u => u.UserId == userID);
        //        Country country = repository
        //            .Country
        //            .FirstOrDefault(c => c.CountryID == Model.CountryID);

        //        int? countryID = null;
        //        if (country != null)
        //        {
        //            countryID = country.CountryID;
        //        }
        //        else if (Model.Country != null)
        //        {
        //            ModelState.AddModelError("Country", "Страна не существует в базе");
        //        }

        //        City city = repository
        //            .City
        //            .FirstOrDefault(c => c.CityID == Model.CityID);

        //        int? cityID = null;
        //        if (city != null)
        //        {
        //            cityID = city.CityID;
        //        }
        //        else if (Model.City != null)
        //        {
        //            ModelState.AddModelError("City", "Город не существует в базе");
        //        }

        //        Institution institution = repository
        //            .Institutions
        //            .FirstOrDefault(i => i.InstitutionID == Model.InstitutionID);

        //        int? institutionID = null;
        //        if (institution != null)
        //        {
        //            institutionID = institution.InstitutionID;
        //        }
        //        else if (Model.Institution != null)
        //        {
        //            ModelState.AddModelError("Institution", "Образовательное учреждение не существует в базе");
        //        }

        //        if (user == null)
        //        {
        //            logger.Warn("User with id = " + userID + " not exist in database");
        //            TempData["ErrorMessage"] = "Пользователь не существует в базе";
        //            return View(Model);
        //        }

        //        logger.Info("User " + user.UserId + " change accaunt information \nSecondName \"" +
        //            user.SecondName + "\" -> \"" + Model.SecondName + "\"\nFirstName \"" +
        //            user.FirstName + "\" -> \"" + Model.FirstName + "\"\nThirdName \"" +
        //            user.ThirdName + "\" -> \"" + Model.ThirdName + "\"\nCategory \"" +
        //            user.Category + "\" -> \"" + (UserCategories)Model.CategoryListID + "\"\nCity \"" +
        //            user.CityID + "\" -> \"" + Model.CityID + "\"\nInstitution \"" +
        //            user.InstitutionID + "\" -> \"" + Model.InstitutionID + "\"\nGradeLevel \"" +
        //            user.GradeLevel + "\" -> \"" + Model.GradeLevel + "\"");

        //        user.FirstName = Model.FirstName;
        //        user.SecondName = Model.SecondName;
        //        user.ThirdName = Model.ThirdName;
        //        user.BirthDay = Model.BirthDay;
        //        user.PhoneNumber = Model.PhoneNumber;
        //        user.Category = (UserCategories)Model.CategoryListID;
        //        user.CountryID = countryID;
        //        user.CityID = cityID;
        //        user.InstitutionID = institutionID;
        //        user.GradeLevel = Model.GradeLevel;

        //        repository.UpdateUserProfile(user);

        //        TempData["SuccessMessage"] = "Данные успешно обновлены";
        //        return View(Model);
        //    }

        //    // If we got this far, something failed, redisplay form
        //    TempData["ErrorMessage"] = "Произошла ошибка при обновлении данных";
        //    return View(Model);
        //}

        #endregion

        #region Team

        //
        // GET: /Account/Team

        public ActionResult Team(TeamMessageId? message)
        {
            ViewBag.StatusMessage =
                message == TeamMessageId.TeamAlreadyExist ? "Команда с таким именем уже существует"
                : message == TeamMessageId.TeamNameIsNotDefined ? "Неккоректное имя команды"
                : message == TeamMessageId.TeamNotExist ? "Команда не существует"
                : message == TeamMessageId.Fail ? "Произошла ошибка при создании команды"
                : "";

            int userID = WebSecurity.GetUserId(User.Identity.Name);
            UserProfile user = repository
                .Users.Include(u => u.Teams)
                .FirstOrDefault(u => u.UserId == userID);

            if (user == null)
            {
                logger.Warn("User with id = " + userID + " not found");
                throw new HttpException(404, "User with id = " + userID + " not found");
            }

            TeamViewModel viewModel = new TeamViewModel();

            List<Team> teams = new List<Team>();
            user.Teams.Each(t =>
                {
                    if (t.Confirm == 1)
                        teams.Add(new Team() { TeamID = t.TeamID, Name = t.Team.Name, Members = t.Team.Members });
                });

            List<Team> invites = new List<Team>();
            user.Teams.Each(t =>
            {
                if (t.Confirm == 0)
                    invites.Add(new Team() { TeamID = t.TeamID, Name = t.Team.Name, Members = t.Team.Members });
            });

            viewModel.Invites = invites;
            viewModel.Teams = teams;

            return View(viewModel);
        }

        //
        // POST: /Account/CreateTeam

        [HttpPost]
        public ActionResult CreateTeam(string TeamName)
        {
            int userID = WebSecurity.GetUserId(User.Identity.Name);
            UserProfile user = repository
                .Users
                .FirstOrDefault(u => u.UserId == userID);

            if (user == null)
            {
                logger.Warn("User with id = " + userID + " not found");
                throw new HttpException(404, "User with id = " + userID + " not found");
            }

            if (TeamName == null || TeamName == "")
            {
                return RedirectToAction("Team", new { Message = TeamMessageId.TeamNameIsNotDefined });
            }

            Team team = repository.Teams.FirstOrDefault(t => t.Name == TeamName);

            if (team != null)
            {
                return RedirectToAction("Team", new { Message = TeamMessageId.TeamAlreadyExist });
            }

            team = new Team() { Name = TeamName };

            repository.AddTeam(team);

            UserProfileTeam userProfileTeam = new UserProfileTeam() { UserID = userID, TeamID = team.TeamID, Confirm = 1 };

            repository.AddUserProfileTeam(userProfileTeam);

            return RedirectToAction("ManageTeam", new { ID = team.TeamID });
        }

        public ActionResult ManageTeam(int ID = -1)
        {
            int userID = WebSecurity.GetUserId(User.Identity.Name);
            UserProfile user = repository
                .Users
                .FirstOrDefault(u => u.UserId == userID);

            if (user == null)
            {
                logger.Warn("User with id = " + userID + " not found");
                throw new HttpException(404, "User with id = " + userID + " not found");
            }

            Team team = repository.Teams.FirstOrDefault(t => 
                t.TeamID == ID && 
                t.Members.FirstOrDefault(up => up.UserID == userID && up.Confirm == 1) != null);

            if (team == null)
            {
                return RedirectToAction("Team", new { Message = TeamMessageId.TeamNotExist });
            }

            ManageTeamViewModel viewModel = new ManageTeamViewModel()
                {
                    TeamID = team.TeamID,
                    Name = team.Name,
                    Members = team.Members
                };

            return View(viewModel);
        }


        /// <summary>
        /// An Ajax method to check if a name of team is unique.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult CheckForUniqueTeam(string Name, int TeamID)
        {
            Team team = repository.Teams.FirstOrDefault(t => t.Name == Name && t.TeamID != TeamID);
            JsonResponse response = new JsonResponse();
            response.Exists = (team == null) ? false : true;

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ChangeTeamName(string Name, int TeamID)
        {
            Team team = repository.Teams.FirstOrDefault(t => t.TeamID == TeamID);
            JsonResponse response = new JsonResponse();
            if (team == null)
            {
                response.Message = Name == null || Name == "" ? "Неккоректное имя команды" : "Команда не существует";
                response.Success = false;
            }
            else
            {
                team.Name = Name;
                repository.AddTeam(team);

                response.Message = "Имя команды успешно изменено";
                response.Success = true;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetUsers(int TeamID, string term, int limit)
        {
            int userId = WebSecurity.GetUserId(User.Identity.Name);

            var user = repository
                .Users
                .Where(u => 
                    u.UserName.Contains(term) && 
                    u.UserId != userId && 
                        (u.Teams.FirstOrDefault(ut => ut.TeamID == TeamID) == null ||
                        u.Teams.FirstOrDefault(ut => ut.TeamID == TeamID).Confirm == -1))
                .Take(limit)
                .Select(u => new { value = u.UserId, label = u.UserName });

            return this.Json(user, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public JsonResult InviteUser(string UserName, int TeamID)
        {
            Team team = repository.Teams.FirstOrDefault(t => t.TeamID == TeamID);
            int userID = WebSecurity.GetUserId(UserName);
            UserProfile user = repository
                .Users
                .FirstOrDefault(u => u.UserId == userID);

            JsonResponse response = new JsonResponse();
            if (team == null || user == null)
            {
                response.Message = user == null ? "Неккоректное имя пользователя" : "Произошла ошибка";
                response.Success = false;
            }
            else
            {
                UserProfileTeam userProfileTeam = repository
                    .UserProfileTeam
                    .FirstOrDefault(ut => ut.UserID == userID && ut.TeamID == team.TeamID);

                if (userProfileTeam == null)
                {
                    userProfileTeam = new UserProfileTeam()
                        {
                            UserID = user.UserId,
                            TeamID = team.TeamID
                        };
                }

                userProfileTeam.Confirm = 0;
                repository.AddUserProfileTeam(userProfileTeam);

                response.Message = "Приглашение для " + UserName + " успешно отправлено";
                response.Success = true;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult InviteResponse(int TeamID = -1, string Accept = null, string Reject = null)
        {
            int userID = WebSecurity.GetUserId(User.Identity.Name);
            UserProfile user = repository
                .Users
                .FirstOrDefault(u => u.UserId == userID);

            if (user == null)
            {
                logger.Warn("User with id = " + userID + " not found");
                throw new HttpException(404, "User with id = " + userID + " not found");
            }

            Team team = repository.Teams.FirstOrDefault(t => t.TeamID == TeamID);

            if (team == null)
            {
                return RedirectToAction("Team", new { Message = TeamMessageId.TeamNotExist });
            }

            if (Accept != null)
            {
                UserProfileTeam userProfileTeam = repository
                    .UserProfileTeam
                    .FirstOrDefault(ut => ut.UserID == userID && ut.TeamID == TeamID);

                if (userProfileTeam == null)
                {
                    logger.Warn("UserProfileTeam with userID = " + userID + " and teamID = " + TeamID + " not found");
                    return RedirectToAction("Team", new { Message = TeamMessageId.TeamNotExist });
                }

                userProfileTeam.Confirm = 1;

                repository.AddUserProfileTeam(userProfileTeam);

                return RedirectToAction("Team");
            }
            if (Reject != null)
            {
                UserProfileTeam userProfileTeam = repository
                    .UserProfileTeam
                    .FirstOrDefault(ut => ut.UserID == userID && ut.TeamID == TeamID);

                if (userProfileTeam == null)
                {
                    logger.Warn("UserProfileTeam with userID = " + userID + " and teamID = " + TeamID + " not found");
                    return RedirectToAction("Team", new { Message = TeamMessageId.TeamNotExist });
                }

                userProfileTeam.Confirm = -1;

                repository.AddUserProfileTeam(userProfileTeam);

                return RedirectToAction("Team");
            }

            return RedirectToAction("Team", new { Message = TeamMessageId.Fail });
        }

        ///// <summary>
        ///// Partial view for members table
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //public JsonResult GetMembersData(int TeamID)
        //{
        //    var response = new JsonResponseMembersData() { Success = true };

        //    List<UserProfileTeam> members = repository.UserProfileTeam
        //        .Where(ut => ut.TeamID == TeamID).ToList();

        //    if (members.Count == 0)
        //    {
        //        response.Success = false;
        //    }

        //    // Get text view.
        //    using (var sw = new StringWriter())
        //    {
        //        ViewData["Members"] = members;

        //        var viewResult = ViewEngines.Engines.FindPartialView(this.ControllerContext, "GetMembersData");
        //        var viewContext = new ViewContext(this.ControllerContext, viewResult.View, ViewData, TempData, sw);
        //        viewResult.View.Render(viewContext, sw);
        //        viewResult.ViewEngine.ReleaseView(this.ControllerContext, viewResult.View);
        //        response.HtmlTable = sw.GetStringBuilder().ToString();
        //    }
            
        //    return Json(response, JsonRequestBehavior.AllowGet);
        //}

        #endregion

        #region External login

        //
        // POST: /Account/ExternalLogin

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // User is new, ask for their desired membership name
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                return View("ExternalLoginConfirmation", new RegisterExternalLoginViewModel { UserName = result.UserName, ExternalLoginData = loginData });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginViewModel model, string returnUrl)
        {
            string provider = null;
            string providerUserId = null;

            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Insert a new user into the database
                using (EFDbContext db = new EFDbContext())
                {
                    UserProfile user = db.UserProfiles.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
                    // Check if user already exists
                    if (user == null)
                    {
                        // Insert name into the profile table
                        db.UserProfiles.Add(new UserProfile { UserName = model.UserName });
                        db.SaveChanges();

                        OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserName);
                        OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);

                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("UserName", "User name already exists. Please enter a different user name.");
                    }
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            List<ExternalLoginModel> externalLogins = new List<ExternalLoginModel>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(new ExternalLoginModel
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId,
                });
            }

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        #endregion

        #region Helpers
        private string AreaName(string url)
        {
            // TODO: Write area name get.

            string[] str = url.Split('/');

            return str[1];
        }

        public static string getIPAddress(HttpRequestBase request)
        {
            string szRemoteAddr = request.UserHostAddress;
            string szXForwardedFor = request.ServerVariables["X_FORWARDED_FOR"];
            string szIP = "";

            if (szXForwardedFor == null)
            {
                szIP = szRemoteAddr;
            }
            else
            {
                szIP = szXForwardedFor;
                //if (szIP.IndexOf(",") > 0)
                //{
                //    string[] arIPs = szIP.Split(',');

                //    foreach (string item in arIPs)
                //    {
                //        if (!isPrivateIP(item))
                //        {
                //            return item;
                //        }
                //    }
                //}
            }
            return szIP;
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            UpdateSuccess,
            UpdateFail
        }

        public enum TeamMessageId
        {
            TeamAlreadyExist,
            TeamNotExist,
            TeamNameIsNotDefined,
            Fail
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Пользователь с таким именем уже существует. Пожалуйста, введите другое имя.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "Пользователь с таким e-mail адресом уже зарегистрирован. Пожалуйста введите другой адрес.";

                case MembershipCreateStatus.InvalidPassword:
                    return "Пароль не соответствует требованиям. Пожалуйста, введите другой пароль.";

                case MembershipCreateStatus.InvalidEmail:
                    return "e-mail адрес некорректен. Пожалуйста, проверьте правильность адреса.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "Ответ на восстановление пароля некорректен. Пожалуйста, проверьте правильность ответа.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "Вопрос на восстановление пароля некорректен. Пожалуйста, проверьте правильность вопроса.";

                case MembershipCreateStatus.InvalidUserName:
                    return "Имя пользователя некорректно. Пожалуйста, проверьте правильность имени пользователя";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
