using log4net;
using log4net.Config;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Solomon.Domain.Abstract;
using Solomon.Domain.Entities;
using Solomon.TypesExtensions;
using Solomon.WebUI.Areas.AccountsManagement.ViewModels;
using Solomon.WebUI.Controllers;
using Solomon.WebUI.Helpers;
using Solomon.WebUI.Mailers;
using Solomon.WebUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;
using viewModels = Solomon.WebUI.Areas.AccountsManagement.ViewModels;

namespace Solomon.WebUI.Areas.AccountsManagement.Controllers
{
    [Authorize(Roles = "Judge, Administrator")]
    public partial class MembershipController : Controller
    {
        private IRepository repository;
        private IUserMailer userMailer;
        private readonly ILog logger = LogManager.GetLogger(typeof(MembershipController));

        /// <summary>
        /// Controller constructor.
        /// Initialize repository.
        /// </summary>
        /// <param name="Repository">Repository object</param>
        public MembershipController(IRepository Repository, IUserMailer UserMailer)
        {
            XmlConfigurator.Configure();
            repository = Repository;
            userMailer = UserMailer;
        }

        #region Index Method
        public ActionResult Index(int Page = 1, int PageSize = 25, string FilterBy = "all", string SearchTerm = null)
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited AccountsManagement/Membership/Index");

            ManageUsersViewModel viewModel = new ManageUsersViewModel();
            viewModel.FilterBy = FilterBy;
            viewModel.SearchTerm = SearchTerm;

            // New search term
            if (System.Web.HttpContext.Current.Request.HttpMethod == "POST")
            {
                Page = 1;
            }

            if (PageSize == 0)
                PageSize = 25;

            viewModel.PageSize = PageSize;

            if (!string.IsNullOrEmpty(FilterBy))
            {
                IQueryable<UserProfile> users = repository.Users;

                if (!Roles.IsUserInRole("Administrator"))
                {
                    users = users.Where(u => u.CreatedByUserID == WebSecurity.CurrentUserId);
                }

                if (FilterBy == "lastaccess")
                {
                    viewModel.PaginatedUserList = users
                            .Where(u => u.UserId != 1)
                            .OrderByDescending(u => u.LastAccessTime)
                            .ToPaginatedList<UserProfile>(Page, PageSize);
                }
                else if (FilterBy == "all" || string.IsNullOrEmpty(SearchTerm))
                {
                    viewModel.PaginatedUserList = users
                            .Where(u => u.UserId != 1)
                            .OrderBy(u => u.UserId)
                            .ToPaginatedList<UserProfile>(Page, PageSize);
                }
                else if (!string.IsNullOrEmpty(SearchTerm))
                {
                    if (FilterBy == "username")
                    {
                        viewModel.PaginatedUserList = users
                                .Where(u => u.UserId != 1 && u.UserName.ToLower().IndexOf(SearchTerm.ToLower()) != -1)
                                .OrderBy(u => u.UserId)
                                .ToPaginatedList<UserProfile>(Page, PageSize);
                    }
                    else if (FilterBy == "email")
                    {
                        viewModel.PaginatedUserList = users
                                .Where(u => u.UserId != 1 && u.Email.ToLower().IndexOf(SearchTerm.ToLower()) != -1)
                                .OrderBy(u => u.UserId)
                                .ToPaginatedList<UserProfile>(Page, PageSize);
                    }
                }
            }

            return View(viewModel);
        }
        #endregion

        #region Create User Methods
            
        public ActionResult CreateUser()
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited AccountsManagement/Membership/CreateUser");

            var model = new viewModels.RegisterViewModel()
            {
                //RequireSecretQuestionAndAnswer = membershipService.RequiresQuestionAndAnswer
            };
            return View(model);
        }

        /// <summary>
        /// This method redirects to the GrantRolesToUser method.
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateUser(viewModels.RegisterViewModel Model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    string confirmationToken = null;
                    if (Model.Approve)
                    {
                        confirmationToken = WebSecurity.CreateUserAndAccount(
                            Model.UserName, Model.Password, new { Email = Model.Email, CreatedByUserID = WebSecurity.CurrentUserId }, true);

                        userMailer.RegisterConfirmation(Model.Email, Model.UserName, confirmationToken).Send();
                    }
                    else
                    {
                        WebSecurity.CreateUserAndAccount(Model.UserName, Model.Password, new { Email = Model.Email, CreatedByUserID = WebSecurity.CurrentUserId });
                    }

                    Roles.AddUserToRole(Model.UserName, "User");

                    logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                        " \"" + User.Identity.Name + "\" created new account \"" + Model.UserName + "\"");

                    return RedirectToAction("GrantRolesToUsers", new { UserID = WebSecurity.GetUserId(Model.UserName) });
                }
                catch (MembershipCreateUserException ex)
                {
                    logger.Warn("Error occured on user " + WebSecurity.GetUserId(User.Identity.Name) + 
                        " \"" + User.Identity.Name + "\" account creating: \"" + ErrorCodeToString(ex.StatusCode) + "\"");
                    ModelState.AddModelError("", ErrorCodeToString(ex.StatusCode));
                }
            }

            return View(Model);
        }

        public ActionResult GenerateUsers()
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited AccountsManagement/Membership/GenerateUsers");

            var model = new viewModels.GenerateUsersViewModel();

            return View(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GenerateUsers(viewModels.GenerateUsersViewModel Model)
        {
            if (ModelState.IsValid)
            {
                if (Model.Titles != null && Model.Titles.ContentLength > 0)
                {
                    try
                    {
                        // Extract only the fielname
                        var fileName = Path.GetFileName(Model.Titles.FileName);

                        // Store the file inside ~/App_Data/uploads folder
                        var path = Path.Combine(LocalPath.RootDirectory, fileName);

                        Model.Titles.SaveAs(path);

                        List<viewModels.Account> accounts = new List<viewModels.Account>();

                        using (StreamReader sr = new StreamReaderExtended(path, true))
                        {
                            int i = 1;
                            string[] fullname;
                            string firstName = "";
                            string secondName = "";
                            string thirdName = "";
                            string userName = "";
                            string password = "";
                            UserProfile user;
                            while (!sr.EndOfStream)
                            {
                                firstName = "";
                                secondName = "";
                                thirdName = "";
                                
                                fullname = sr.ReadLine().Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);

                                if (fullname.Count() >= 1) secondName = fullname[0];
                                if (fullname.Count() >= 2) firstName = fullname[1];
                                if (fullname.Count() >= 3) thirdName = fullname[2];

                                do
                                {
                                    userName = Model.UserNameTemplate + i.ToString();
                                    i++;
                                    user = repository.Users.FirstOrDefault(u => u.UserName == userName);
                                }
                                while (user != null);

                                password = password.RandomString(7);

                                accounts.Add(new viewModels.Account()
                                {
                                    UserName = userName,
                                    SecondName = secondName,
                                    FirstName = firstName,
                                    ThirdName = thirdName,
                                    Password = password
                                });
                            }
                        }

                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }

                        List<SelectListItem> tournaments = new List<SelectListItem>();
                        tournaments.Add(new SelectListItem() { Value = "0", Text = "Не регистрировать" });
                        repository.Tournaments
                            .Where(t => t.EndDate > DateTime.Now)
                            .Each(t => tournaments.Add(new SelectListItem() { Value = t.TournamentID.ToString(), Text = t.Name }));

                        return View("ConfirmGenerateUsers", 
                            new GeneratedUsersListViewModel() { 
                                Accounts = accounts,
                                AccountTemplate = new viewModels.Account(),
                                TournamentList = tournaments, 
                                Generated = false });
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error occurred on user " + WebSecurity.GetUserId(User.Identity.Name) + 
                            " \"" + User.Identity.Name + "\" attempt generating accounts: ", ex);

                        ModelState.AddModelError("", "Произошла ошибка при генерации аккаунтов");
                    }
                }
            }

            return View(Model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ConfirmGenerateUsers(List<viewModels.Account> Accounts, int TournamentID, string Generate = null, string Cancel = null)
        {
            if (Cancel != null)
            {
                return RedirectToAction("GenerateUsers");
            }

            Tournament tournament = repository.Tournaments.FirstOrDefault(t => t.TournamentID == TournamentID);
            if (Generate != null)
            {
                try
                {
                    int userID = WebSecurity.CurrentUserId;
                    foreach (var account in Accounts)
                    {
                        WebSecurity.CreateUserAndAccount(account.UserName, account.Password,
                            new
                            {
                                BirthDay = account.BirthDay,
                                Category = account.CategoryListID,
                                CityID = account.CityID,
                                CountryID = account.CountryID,
                                FirstName = account.FirstName,
                                Generated = 1,
                                CreatedByUserID = userID,
                                GradeLevel = account.GradeLevel,
                                InstitutionID = account.InstitutionID,
                                PhoneNumber = account.PhoneNumber,
                                SecondName = account.SecondName,
                                ThirdName = account.ThirdName
                            });

                        Roles.AddUserToRole(account.UserName, "User");

                        if (tournament != null)
                            repository.BindUserToTournament(TournamentID, WebSecurity.GetUserId(account.UserName));
                    }

                    using (StreamWriter sw = new StreamWriter(LocalPath.AbsoluteGeneratedAccountsDirectory + DateTime.Now.ToString("dd.MM.yyyy_HH.mm.txt")))
                    {
                        sw.WriteLine("UserName FullName Password");

                        foreach (var account in Accounts)
                        {
                            sw.WriteLine(account.UserName + " " + account.SecondName + "_" + account.FirstName + "_" + account.ThirdName + " " + account.Password);
                        }
                    }

                    logger.Info("Genereted " + Accounts.Count() + " accounts");
                }
                catch (Exception ex)
                {
                    logger.Error("Error occurred on user " + WebSecurity.GetUserId(User.Identity.Name) + 
                        " \"" + User.Identity.Name + "\" generating accounts: ", ex);

                    ModelState.AddModelError("", "Произошла ошибка при генерации аккаунтов");
                }
            }

            var viewModel = new GeneratedUsersListViewModel() { 
                Accounts = Accounts,
                Generated = true,
                AccountTemplate = new viewModels.Account(),
                RegisteredForTournament = tournament != null,
                TournamentName = tournament != null ? tournament.Name : "",
                TournamentID = TournamentID
            };

            return View(viewModel);
        }

        [HttpPost]
        public void ExportToExcel(List<viewModels.Account> Accounts, int TournamentID = -1,
            string IncludeUserName = null,
            string IncludePassword = null,
            string IncludeFIO = null,
            string IncludeBirthDay = null,
            string IncludePhoneNumber = null,
            string IncludeCountry = null,
            string IncludeCity = null,
            string IncludeInstitution = null,
            string IncludeCategory = null,
            string IncludeGradeLevel = null
            )
        {
            var tournament = repository
                .Tournaments
                .FirstOrDefault(t => t.TournamentID == TournamentID);

            using (ExcelPackage pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Аккаунты");

                int row = 1, col = 1;

                if (IncludeUserName != null)
                {
                    ws.Cells[row, col].Value = "Логин";
                    ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    ws.Column(col).Width = 20;
                    col++;
                }

                if (IncludePassword != null)
                {
                    ws.Cells[row, col].Value = "Пароль";
                    ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    ws.Column(col).Width = 10;
                    col++;
                }

                if (IncludeFIO != null)
                {
                    ws.Cells[row, col].Value = "ФИО";
                    ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    ws.Column(col).Width = 30;
                    col++;
                }

                if (IncludeBirthDay != null)
                {
                    ws.Cells[row, col].Value = "Дата рождения";
                    ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    ws.Column(col).Width = 15;
                    col++;
                }

                if (IncludePhoneNumber != null)
                {
                    ws.Cells[row, col].Value = "Номер телефона";
                    ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    ws.Column(col).Width = 20;
                    col++;
                }

                if (IncludeCountry != null)
                {
                    ws.Cells[row, col].Value = "Страна";
                    ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    ws.Column(col).Width = 15;
                    col++;
                }

                if (IncludeCity != null)
                {
                    ws.Cells[row, col].Value = "Город";
                    ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    ws.Column(col).Width = 15;
                    col++;
                }

                if (IncludeInstitution != null)
                {
                    ws.Cells[row, col].Value = "Образовательное учреждение (организация)";
                    ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    ws.Column(col).Width = 40;
                    col++;
                }

                if (IncludeCategory != null)
                {
                    ws.Cells[row, col].Value = "Категория";
                    ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    ws.Column(col).Width = 20;
                    col++;
                }

                if (IncludeGradeLevel != null)
                {
                    ws.Cells[row, col].Value = "Год обучения";
                    ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    ws.Column(col).Width = 15;
                    col++;
                }


                //Format the header for columns
                using (ExcelRange rng = ws.Cells[row, 1, row, col])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //rng.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                    //rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));  //Set color to dark blue
                    //rng.Style.Font.Color.SetColor(Color.White);
                }

                foreach (var a in Accounts)
                {
                    row++;
                    col = 1;

                    // Login
                    if (IncludeUserName != null)
                    {
                        ws.Cells[row, col].Value = a.UserName;
                        ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        col++;
                    }

                    // Password
                    if (IncludePassword != null)
                    {
                        ws.Cells[row, col].Value = a.Password;
                        ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        col++;
                    }

                    // FIO
                    if (IncludeFIO != null)
                    {
                        ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ws.Cells[row, col].Value = (a.SecondName + " " + a.FirstName + " " + a.ThirdName).Trim();
                        col++;
                    }

                    // BirthDay
                    if (IncludeBirthDay != null)
                    {
                        ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ws.Cells[row, col].Value = a.BirthDay != null ? ((DateTime)a.BirthDay).ToString("dd.MM.yyyy") : String.Empty;
                        col++;
                    }

                    // Phone number
                    if (IncludeBirthDay != null)
                    {
                        ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ws.Cells[row, col].Value = a.PhoneNumber;
                        col++;
                    }

                    // Country
                    if (IncludeCountry != null)
                    {
                        // Country entity don't loaded from a.Country, becouse it's IEnumarable collection
                        var country = repository.Country.FirstOrDefault(c => c.CountryID == a.CountryID);
                        ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ws.Cells[row, col].Value = country != null ? country.Name : String.Empty;
                        col++;
                    }

                    // City
                    if (IncludeCity != null)
                    {
                        var city = repository.City.FirstOrDefault(c => c.CityID == a.CityID);
                        ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ws.Cells[row, col].Value = city != null ? (city.Name + " " + city.Area + " " + city.Region) : String.Empty;
                        col++;
                    }

                    // Institution
                    if (IncludeInstitution != null)
                    {
                        var institution = repository.Institutions.FirstOrDefault(ins => ins.InstitutionID == a.InstitutionID);
                        ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ws.Cells[row, col].Value = institution != null ? institution.Name : String.Empty;
                        col++;
                    }

                    // Category
                    if (IncludeCategory != null)
                    {
                        ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        string category = "";
                        switch ((UserCategories)a.CategoryListID)
                        {
                            case UserCategories.None:
                                category = "Не выбрано";
                                break;
                            case UserCategories.School:
                                category = "Школьник";
                                break;
                            case UserCategories.Student:
                                category = "Студент";
                                break;
                            case UserCategories.Teacher:
                                category = "Преподаватель";
                                break;
                            case UserCategories.Other:
                                category = "Другое";
                                break;
                        }
                        ws.Cells[row, col].Value = category;
                        col++;
                    }

                    // Grade level
                    if (IncludeGradeLevel != null)
                    {
                        ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ws.Cells[row, col].Value = a.GradeLevel;
                        col++;
                    }
                }


                ////Example how to Format Column 1 as numeric 
                //using (ExcelRange col = ws.Cells[2, 1, 2 + tbl.Rows.Count, 1])
                //{
                //    col.Style.Numberformat.Format = "#,##0.00";
                //    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                //}

                //Write it back to the client
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;  filename=Аккаунты.xlsx");
                Response.BinaryWrite(pck.GetAsByteArray());
            }
            return;
        }

        /// <summary>
        /// An Ajax method to check if a username is unique.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult CheckForUniqueUser(string userName)
        {
            UserProfile user = repository.Users.FirstOrDefault(u => u.UserName == userName);
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
        public ActionResult CheckForUniqueEmail(string Email)
        {
            UserProfile user = repository.Users.FirstOrDefault(u => u.Email == Email);
            JsonResponse response = new JsonResponse();
            response.Exists = (user == null) ? false : true;

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Пользователь с таким логином уже существует. Пожалуйста, введите другой логин.";

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

        #region View User Details Methods

        [HttpGet]
        public ActionResult Update(int UserID = -1)
        {
            int userID = UserID;

            if (userID == 1 && WebSecurity.CurrentUserId != 1)
                throw new HttpException(404, "User not found");

            UserProfile user = repository
                .Users
                .FirstOrDefault(u => u.UserId == userID);

            if (user == null)
                throw new HttpException(404, "User not found");

            Country country = repository.Country.FirstOrDefault(c => c.CountryID == user.CountryID);
            City city = repository.City.FirstOrDefault(c => c.CityID == user.CityID);
            Institution institution = repository.Institutions.FirstOrDefault(i => i.InstitutionID == user.InstitutionID);

            UserViewModel viewModel = new UserViewModel()
            {
                UserName = user.UserName,
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
                GradeLevel = user.GradeLevel,

                RolesIds = System.Web.Security.Roles.GetRolesForUser(user.UserName)
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Update(UserViewModel Model, int UserID = -1, string Update = null, string Delete = null, string Cancel = null)
        {
            if (UserID == -1 || Model.UserName == null || (Model.UserName == "Admin" && WebSecurity.CurrentUserId != 1))
            {
                logger.Warn("User with name = " + Model.UserName + " not found");
                throw new HttpException(404, "User not found");
            }

            if (Delete != null)
                return DeleteUser(UserID);
            if (Cancel != null)
                return CancelUser();
            if (Update != null)
                return UpdateUser(Model, UserID);

            return CancelUser();
        }

        private ActionResult UpdateUser(UserViewModel model, int UserID = -1)
        {
            UserProfile user = repository.Users.FirstOrDefault(u => u.UserId == UserID);

            if (user == null)
            {
                logger.Warn("User not found");
                throw new HttpException(404, "User not found");
            }

            if (ModelState.IsValid)
            {
                Country country = repository
                    .Country
                    .FirstOrDefault(c => c.CountryID == model.CountryID);

                int? countryID = null;
                if (country != null)
                {
                    countryID = country.CountryID;
                }
                else if (model.Country != null)
                {
                    ModelState.AddModelError("Country", "Страна не существует в базе");
                }

                City city = repository
                    .City
                    .FirstOrDefault(c => c.CityID == model.CityID);

                int? cityID = null;
                if (city != null)
                {
                    cityID = city.CityID;
                }
                else if (model.City != null)
                {
                    ModelState.AddModelError("City", "Город не существует в базе");
                }

                Institution institution = repository
                    .Institutions
                    .FirstOrDefault(i => i.InstitutionID == model.InstitutionID);

                int? institutionID = null;
                if (institution != null)
                {
                    institutionID = institution.InstitutionID;
                }
                else if (model.Institution != null)
                {
                    ModelState.AddModelError("Institution", "Образовательное учреждение не существует в базе");
                }

                logger.Info("User " + WebSecurity.CurrentUserId + " change accaunt information for user " + user.UserId + " \nSecondName \"" +
                    user.SecondName + "\" -> \"" + model.SecondName + "\"\nFirstName \"" +
                    user.FirstName + "\" -> \"" + model.FirstName + "\"\nThirdName \"" +
                    user.ThirdName + "\" -> \"" + model.ThirdName + "\"\nCategory \"" +
                    user.Category + "\" -> \"" + (UserCategories)model.CategoryListID + "\"\nCity \"" +
                    user.CityID + "\" -> \"" + model.CityID + "\"\nInstitution \"" +
                    user.InstitutionID + "\" -> \"" + model.InstitutionID + "\"\nGradeLevel \"" +
                    user.GradeLevel + "\" -> \"" + model.GradeLevel + "\"");

                user.FirstName = model.FirstName;
                user.SecondName = model.SecondName;
                user.ThirdName = model.ThirdName;
                user.BirthDay = model.BirthDay;
                user.PhoneNumber = model.PhoneNumber;
                user.Category = (UserCategories)model.CategoryListID;
                user.CountryID = countryID;
                user.CityID = cityID;
                user.InstitutionID = institutionID;
                user.GradeLevel = model.GradeLevel;

                string[] removeRoles, addRoles;
                if (model.RolesIds == null) model.RolesIds = new string[] { };
                removeRoles = Roles.GetRolesForUser(model.UserName).Except(model.RolesIds).ToArray();
                addRoles = model.RolesIds.Except(Roles.GetRolesForUser(model.UserName)).ToArray();

                if (removeRoles.Length > 0)
                    Roles.RemoveUserFromRoles(model.UserName, removeRoles);
                if (addRoles.Length > 0)
                    Roles.AddUserToRoles(model.UserName, addRoles);

                repository.UpdateUserProfile(user);

                TempData["SuccessMessage"] = "Данные успешно обновлены";
                RedirectToAction("Update", new { UserID = UserID });
            }

            // If we got this far, something failed, redisplay form
            TempData["ErrorMessage"] = "Произошла ошибка при обновлении данных";
            return RedirectToAction("Update", new { UserID = UserID });
        }

        #region Ajax methods for Updating the user

        [HttpPost]
        public ActionResult Unlock(string userName)
        {
            JsonResponse response = new JsonResponse();

            //MembershipUser user = membershipService.GetUser(userName);

            try
            {
                //user.UnlockUser();
                response.Success = true;
                response.Message = "User unlocked successfully!";
                response.Locked = false;
                response.LockedStatus = (response.Locked) ? "Locked" : "Unlocked";
            }
            catch (Exception)
            {
                response.Success = false;
                response.Message = "User unlocked failed.";
            }

            return Json(response);
        }

        [HttpPost]
        public ActionResult ApproveDeny(string userName)
        {
            JsonResponse response = new JsonResponse();

            //MembershipUser user = membershipService.GetUser(userName);

            try
            {
                //user.IsApproved = !user.IsApproved;
                //membershipService.UpdateUser(user);

                //string approvedMsg = (user.IsApproved) ? "Approved" : "Denied";

                //response.Success = true;
                //response.Message = "User " + approvedMsg + " successfully!";
                //response.Approved = user.IsApproved;
                //response.ApprovedStatus = (user.IsApproved) ? "Approved" : "Not approved";
            }
            catch (Exception)
            {
                response.Success = false;
                response.Message = "User unlocked failed.";
            }

            return Json(response);
        }

        #endregion

        #endregion

        #region Delete User Methods

        private ActionResult DeleteUser(int UserID = -1)
        {
            if (UserID == 1)
            {
                logger.Warn("User " + WebSecurity.CurrentUserId +
                    " \"" + User.Identity.Name + "\" try to delete Admin");
                throw new HttpException(404, "User not found");
            }

            UserProfile user = repository.Users.FirstOrDefault(u => u.UserId == UserID);

            if (user == null)
            {
                logger.Warn("User not found");
                throw new HttpException(404, "User not found");
            }

            

            try
            {
                var roles = Roles.GetRolesForUser(user.UserName);
                if (roles != null && roles.Length > 0)
                    Roles.RemoveUserFromRoles(user.UserName, Roles.GetRolesForUser(user.UserName));
                ((SimpleMembershipProvider)Membership.Provider).DeleteAccount(user.UserName);
                ((SimpleMembershipProvider)Membership.Provider).DeleteUser(user.UserName, true);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Произошла ошибка при удалении пользователя";
                logger.Error("Error occurred on user " + WebSecurity.GetUserId(User.Identity.Name) +
                    " \"" + User.Identity.Name + "\" deleting user with name = " + user.UserName + ": ", ex);
            }

            return RedirectToAction("Update", new { UserID = UserID });
        }

        #endregion

        #region Cancel User Methods

        [HttpPost]
        public ActionResult CancelUser()
        {
            return RedirectToAction("Index");
        }

        #endregion

        #region Grant Users with Roles Methods

        /// <summary>
        /// Return two lists:
        ///   1)  a list of Roles not granted to the user
        ///   2)  a list of Roles granted to the user
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult GrantRolesToUsers(int UserID = -1, int Page = 1, int PageSize = 25, string FilterBy = "all", string SearchTerm = null)
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited AccountsManagement/Membership/GrantRolesToUser");

            if (UserID == -1)
            {
                ManageUsersViewModel viewModel = new ManageUsersViewModel();
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
                    IQueryable<UserProfile> users = repository.Users;

                    if (!Roles.IsUserInRole("Administrator"))
                    {
                        users = users.Where(u => u.CreatedByUserID == WebSecurity.CurrentUserId);
                    }

                    if (FilterBy == "all" || string.IsNullOrEmpty(SearchTerm))
                    {
                        viewModel.PaginatedUserList = users
                                .Where(u => u.UserId != 1)
                                .OrderBy(u => u.UserId)
                                .ToPaginatedList<UserProfile>(Page, PageSize);
                    }
                    else if (!string.IsNullOrEmpty(SearchTerm))
                    {
                        if (FilterBy == "username")
                        {
                            viewModel.PaginatedUserList = users
                                    .Where(u => u.UserId != 1 && u.UserName.ToLower().IndexOf(SearchTerm.ToLower()) != -1)
                                    .OrderBy(u => u.UserId)
                                    .ToPaginatedList<UserProfile>(Page, PageSize);
                        }
                        else if (FilterBy == "email")
                        {
                            viewModel.PaginatedUserList = users
                                    .Where(u => u.UserId != 1 && u.Email.ToLower().IndexOf(SearchTerm.ToLower()) != -1)
                                    .OrderBy(u => u.UserId)
                                    .ToPaginatedList<UserProfile>(Page, PageSize);
                        }
                    }
                }
                return View("UsersForRoles", viewModel);
            }

            if (UserID == 1)
            {
                logger.Warn("User " + WebSecurity.CurrentUserId +
                    " \"" + User.Identity.Name + "\" try to change roles for Admin");
                throw new HttpException(404, "User not found");
            }

            UserProfile user = repository.Users.FirstOrDefault(u => u.UserId == UserID);

            if (user == null)
            {
                logger.Warn("User not found");
                throw new HttpException(404, "User not found");
            }

            GrantRolesToUserViewModel model = new GrantRolesToUserViewModel();
            model.UserName = user.UserName;

            IEnumerable<string> availableRoles = Roles.GetAllRoles().Except(Roles.GetRolesForUser(user.UserName));
            if (!Roles.IsUserInRole("Administrator"))
                availableRoles = availableRoles.Except(new string[] { "Administrator" });

            IEnumerable<string> grantedRoles = Roles.GetRolesForUser(user.UserName);
            if (!Roles.IsUserInRole("Administrator"))
                grantedRoles = grantedRoles.Except(new string[] { "Administrator" });

            model.AvailableRoles = new SelectList(availableRoles);
            model.GrantedRoles = (string.IsNullOrEmpty(user.UserName) ? new SelectList(new string[] { }) : new SelectList(grantedRoles));

            return View(model);
        }

        /// <summary>
        /// Grant the selected roles to the user.
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="roleNames"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GrantRolesToUser(string UserName, string Roles)
        {
            JsonResponse response = new JsonResponse();

            if (string.IsNullOrEmpty(UserName))
            {
                response.Success = false;
                response.Message = "Имя пользователя не указано.";
                return Json(response);
            }

            if (string.IsNullOrEmpty(Roles))
            {
                response.Success = false;
                response.Message = "Роль не указана";
                return Json(response);
            }

            string[] roleNames = Roles.Substring(0, Roles.Length - 1).Split(',');

            if (roleNames.Length == 0)
            {
                response.Success = false;
                response.Message = "Роль не указана.";
                return Json(response);
            }

            try
            {
                System.Web.Security.Roles.AddUserToRoles(UserName, roleNames);

                response.Success = true;
                response.Message = "Роль(и) успешно привязана(ы) к пользователю " + UserName;

                logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                    " \"" + User.Identity.Name + "\" grant role \"" + Roles + "\" to \"" + UserName + "\" ");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Произошла ошибка при добавлении роли(ей) к пользователю.";

                logger.Warn("Error occurred on user " + WebSecurity.GetUserId(User.Identity.Name) + 
                    " \"" + User.Identity.Name + "\" granting role \"" + Roles + 
                    "\" from \"" + UserName + "\": ", ex);
            }

            return Json(response);
        }

        /// <summary>
        /// Revoke the selected roles for the user.
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="roleNames"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RevokeRolesFromUser(string UserName, string Roles)
        {
            JsonResponse response = new JsonResponse();

            if (string.IsNullOrEmpty(UserName))
            {
                response.Success = false;
                response.Message = "Имя пользователя не указано.";
                return Json(response);
            }

            if (string.IsNullOrEmpty(Roles))
            {
                response.Success = false;
                response.Message = "Роль не указана";
                return Json(response);
            }

            string[] roleNames = Roles.Substring(0, Roles.Length - 1).Split(',');

            if (roleNames.Length == 0)
            {
                response.Success = false;
                response.Message = "Роль не указана.";
                return Json(response);
            }

            try
            {
                System.Web.Security.Roles.RemoveUserFromRoles(UserName, roleNames);

                response.Success = true;
                response.Message = "Роль(и) успешно удалена(ы) у пользователя " + UserName;

                logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                    " \"" + User.Identity.Name + "\" revoke role \"" + Roles + "\" from \"" + UserName + "\" ");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Произошла ошибка при удалении роли(ей) у пользователя.";

                logger.Warn("Error occurred on user " + WebSecurity.GetUserId(User.Identity.Name) + 
                    " \"" + User.Identity.Name + "\" revoking role \"" + Roles + "\" from \"" + UserName + "\": ", ex);
            }

            return Json(response);
        }

        #endregion

    }
}
