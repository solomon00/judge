using System;
using System.Web.Mvc;
using System.Web.Security;
using Solomon.WebUI.Controllers;
using Solomon.TypesExtensions;
using viewModels = Solomon.WebUI.Areas.TournamentsManagement.ViewModels;
using System.Collections.Generic;
using WebMatrix.WebData;
using Solomon.Domain.Abstract;
using Solomon.Domain.Entities;
using System.Linq;
using Solomon.WebUI.Models;
using Solomon.WebUI.Areas.TournamentsManagement.ViewModels;
using System.Web;
using log4net;
using log4net.Config;
using System.Globalization;

namespace Solomon.WebUI.Areas.TournamentsManagement.Controllers
{
    [Authorize(Roles = "Judge, Administrator")]
    public partial class TournamentController : Controller
    {
        private IRepository repository;
        private readonly ILog logger = LogManager.GetLogger(typeof(TournamentController));

        /// <summary>
        /// Controller constructor.
        /// Initialize repository.
        /// </summary>
        /// <param name="Repository">Repository object</param>
        public TournamentController(IRepository Repository)
        {
            XmlConfigurator.Configure();
            repository = Repository;

            //System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
            //System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");
        }

        #region Index Method
        public ActionResult Index(int Page = 1, int PageSize = 25, string FilterBy = "all", string SearchTerm = null)
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited TournamentsManagement/Tournament/Index");

            ManageTournamentsViewModel viewModel = new ManageTournamentsViewModel();
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
                if (FilterBy == "all" || string.IsNullOrEmpty(SearchTerm))
                {
                    viewModel.PaginatedTournamentList = repository.Tournaments
                            .OrderByDescending(t => t.TournamentID)
                            .ToPaginatedList<Tournament>(Page, PageSize);
                }
                else if (!string.IsNullOrEmpty(SearchTerm))
                {
                    if (FilterBy == "name")
                    {
                        viewModel.PaginatedTournamentList = repository.Tournaments
                            .Where(t => t.Name.ToLower().IndexOf(SearchTerm.ToLower()) != -1)
                            .OrderByDescending(t => t.TournamentID)
                            .ToPaginatedList<Tournament>(Page, PageSize);
                    }
                }
            }

            return View(viewModel);
        }
        #endregion

        #region Create Tournament Methods

        public ActionResult Create()
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited TournamentsManagement/Tournament/Create");

            var model = new viewModels.NewTournamentViewModel();
            return View(model);
        }

        /// <summary>
        /// This method redirects to the GrantRolesToUser method.
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(viewModels.NewTournamentViewModel Model)
        {
            if (ModelState.IsValid)
            {
                int tournamentID = -1;

                // Attempt to create new tournament
                try
                {
                    Tournament tournament = new Tournament
                    {
                        Name = Model.Name,
                        Type = (TournamentTypes)Model.TournamentTypesListID,
                        Format = (TournamentFormats)Model.TournamentFormatsListID,

                        ShowSolutionSendingTime = Model.ShowSolutionSendingTime,
                        ShowTimer = Model.ShowTimer,
                        
                        StartDate = Model.StartDate.Add(Model.StartTime.TimeOfDay),
                        EndDate = Model.EndDate.Add(Model.EndTime.TimeOfDay)
                    };
                    int userID = WebSecurity.CurrentUserId;
                    if (userID != 1)
                    {
                        UserProfile user = repository.Users.FirstOrDefault(u => u.UserId == userID);
                        tournament.UsersCanModify = new List<UserProfile>();
                        tournament.UsersCanModify.Add(user);
                    }
                    tournamentID = repository.AddTournament(tournament);

                    if (tournament.Type == TournamentTypes.Close)
                    {
                        repository.BindUserToTournament(tournament.TournamentID, WebSecurity.GetUserId(User.Identity.Name));
                    }

                    logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) +
                        " \"" + User.Identity.Name + "\" create tournament " + tournamentID + " \"" + Model.Name + "\"");

                    return RedirectToAction("BindProblemsToTournament", new { tournamentID = tournament.TournamentID });
                }
                catch (Exception ex)
                {
                    logger.Error("Error occurred on user " + WebSecurity.GetUserId(User.Identity.Name) +
                        " \"" + User.Identity.Name + "\" creating tournament " + tournamentID + " \"" + Model.Name + "\": ", ex);

                    ModelState.AddModelError("", "Произошла ошибка при создании турнира");
                }
            }

            return View(Model);
        }

        /// <summary>
        /// An Ajax method to check if a username is unique.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult CheckForUniqueTournamentName(string TournamentName, int TournamentID = -1)
        {
            Tournament tournament = repository
                .Tournaments
                .FirstOrDefault(t => t.Name == TournamentName && t.TournamentID != TournamentID);
            JsonResponse response = new JsonResponse();
            response.Exists = (tournament == null) ? false : true;

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        #endregion

        #region View Tournament Details Methods

        [HttpGet]
        public ActionResult Update(int TournamentID = -1)
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited TournamentsManagement/Tournament/Update");
     
            Tournament tournament = repository.Tournaments.Where(t => t.TournamentID == TournamentID).FirstOrDefault();

            if (tournament == null)
            {
                logger.Warn("Tournament with id = " + TournamentID + " not found");
                throw new HttpException(404, "Tournament not found");
            }

            EditTournamentViewModel viewModel = new EditTournamentViewModel()
                {
                    TournamentID = tournament.TournamentID,
                    Name = tournament.Name,
                    TournamentTypesListID = (int)tournament.Type,
                    TournamentFormatsListID = (int)tournament.Format,

                    ShowSolutionSendingTime = tournament.ShowSolutionSendingTime,
                    ShowTimer = tournament.ShowTimer,

                    StartDate = tournament.StartDate,
                    StartTime = tournament.StartDate,
                    EndDate = tournament.EndDate,
                    EndTime = tournament.EndDate,
                    Problems = tournament.Problems.Select(p => p.Name)
                };
                        
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Update(viewModels.EditTournamentViewModel Model, int TournamentID = -1, string Update = null, string Delete = null, string Cancel = null)
        {
            if (TournamentID == -1)
            {
                logger.Warn("Tournament with id = " + TournamentID + " not found");
                throw new HttpException(404, "Tournament not found");
            }

            if (Delete != null)
                return DeleteTournament(TournamentID);
            if (Cancel != null)
                return CancelTournament();
            if (Update != null)
                return UpdateTournament(Model, TournamentID);
            
            return CancelTournament();
            
        }

        private ActionResult UpdateTournament(viewModels.EditTournamentViewModel Model, int TournamentID = -1)
        {
            try
            {
                Tournament tournament = repository.Tournaments.FirstOrDefault(t => t.TournamentID == TournamentID);

                if (tournament == null)
                {
                    logger.Warn("Problem with id = " + TournamentID + " not found");
                    throw new HttpException(404, "Tournament not found");
                }

                tournament.Name = Model.Name;
                tournament.Format = (TournamentFormats)Model.TournamentFormatsListID;
                tournament.Type = (TournamentTypes)Model.TournamentTypesListID;

                tournament.ShowSolutionSendingTime = Model.ShowSolutionSendingTime;
                tournament.ShowTimer = Model.ShowTimer;

                tournament.StartDate = Model.StartDate.Add(Model.StartTime.TimeOfDay);
                tournament.EndDate = Model.EndDate.Add(Model.EndTime.TimeOfDay);

                repository.AddTournament(tournament);

                if (tournament.Type == TournamentTypes.Close)
                {
                    repository.BindUserToTournament(tournament.TournamentID, WebSecurity.GetUserId(User.Identity.Name));
                }

                TempData["SuccessMessage"] = "Турнир успешно обновлен!";

                logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                    " \"" + User.Identity.Name + "\" update tournament \"" + TournamentID + "\"");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Произошла ошибка при обновлении турнира.";
                logger.Error("Error occured on user " + WebSecurity.GetUserId(User.Identity.Name) + 
                    " \"" + User.Identity.Name + "\" updating tournament \"" + TournamentID + "\": ", ex);
            }

            return RedirectToAction("Update", new { TournamentID = TournamentID });
        }

        #endregion

        #region Delete Tournament Methods

        private ActionResult DeleteTournament(int TournamentID = -1)
        {
            if (TournamentID == -1)
            {
                logger.Warn("Tournament with id = " + TournamentID + " not found");
                throw new HttpException(404, "Tournament not found");
            }

            try
            {
                repository.DeleteTournament(TournamentID);

                logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                    " \"" + User.Identity.Name + "\" delete tournament with id = " + TournamentID);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Произошла ошибка при удалении турнира";
                logger.Error("Error occurred on user " + WebSecurity.GetUserId(User.Identity.Name) + 
                    " \"" + User.Identity.Name + "\" deleting tournament with id = " + TournamentID + ": ", ex);
            }

            return RedirectToAction("Update", new { TournamentID = TournamentID });
        }

        #endregion

        #region Cancel Tournament Methods

        private ActionResult CancelTournament()
        {
            return RedirectToAction("Index");
        }

        #endregion

        #region Bind Problems To Tournament Methods

        /// <summary>
        /// Return two lists:
        ///   1)  a list of Problems not bound to the tournament
        ///   2)  a list of Problems granted to the tournament
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult BindProblemsToTournament(int TournamentID = -1, int Page = 1, int PageSize = 25, string FilterBy = "all", string SearchTerm = null)
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited TournamentsManagement/Tournament/BindProblemsToTournament");

            if (TournamentID == -1)
            {
                ManageTournamentsViewModel viewModel = new ManageTournamentsViewModel();
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
                    if (FilterBy == "all" || string.IsNullOrEmpty(SearchTerm))
                    {
                        viewModel.PaginatedTournamentList = repository.Tournaments
                                .OrderByDescending(t => t.TournamentID)
                                .ToPaginatedList<Tournament>(Page, PageSize);
                    }
                    else if (!string.IsNullOrEmpty(SearchTerm))
                    {
                        if (FilterBy == "name")
                        {
                            viewModel.PaginatedTournamentList = repository.Tournaments
                                .Where(t => t.Name.ToLower().IndexOf(SearchTerm.ToLower()) != -1)
                                .OrderByDescending(t => t.TournamentID)
                                .ToPaginatedList<Tournament>(Page, PageSize);
                        }
                    }
                }
                return View("ProblemsForTournament", viewModel);
            }

            BindProblemsToTournamentViewModel model = new BindProblemsToTournamentViewModel();
            Tournament tournament = repository.Tournaments
                .FirstOrDefault(t => t.TournamentID == TournamentID);
            if (tournament == null)
            {
                logger.Warn("Tournament with id = " + TournamentID + " not found");
                throw new HttpException(404, "Tournament not found");
            }

            model.TournamentID = tournament.TournamentID;
            model.TournamentName = tournament.Name;

            // AsEnumerable() need, because Except() for IQueryable work in DB 
            // and return 'not supported' (really hard to work) data.
            if (tournament.Problems != null)
            {
                model.AvailableProblems = new SelectList(repository.Problems.AsEnumerable().Except(tournament.Problems).OrderByDescending(p => p.ProblemID), "ProblemID", "Name");
                model.BoundProblems = new SelectList(tournament.Problems, "ProblemID", "Name");
            }
            else
            {
                model.AvailableProblems = new SelectList(repository.Problems.AsEnumerable().OrderByDescending(p => p.ProblemID), "ProblemID", "Name");
                model.BoundProblems = null;
            }
            
            return View(model);
        }

        /// <summary>
        /// Grant the selected roles to the user.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="roleNames"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult BindProblemToTournament(int TournamentID = -1, int ProblemID = -1)
        {
            JsonResponse response = new JsonResponse();

            if (TournamentID == -1)
            {
                response.Success = false;
                response.Message = "Идентификатор турнира некорректен.";
                return Json(response);
            }

            if (ProblemID == -1)
            {
                response.Success = false;
                response.Message = "Идентификатор задачи некорректен.";
                return Json(response);
            }

            try
            {
                repository.BindProblemToTournament(TournamentID, ProblemID);

                response.Success = true;
                response.Message = "Задача успешно добавлена в турнир.";

                logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                    " \"" + User.Identity.Name + "\" bind problem \"" + ProblemID + "\" to tournament \"" + TournamentID + "\" ");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Произошла ошибка при добавлении задачи в турнир.";

                logger.Warn("Error occurred on user " + WebSecurity.GetUserId(User.Identity.Name) + 
                    " \"" + User.Identity.Name + "\" binding problem \"" + ProblemID + "\" to tournament \"" + TournamentID + "\": {3} ", ex);
            }

            return Json(response);
        }

        /// <summary>
        /// Revoke the selected roles for the user.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="roleNames"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UnbindProblemFromTournament(int TournamentID = -1, int ProblemID = -1)
        {
            JsonResponse response = new JsonResponse();

            if (TournamentID == -1)
            {
                response.Success = false;
                response.Message = "Идентификатор турнира некорректен.";
                return Json(response);
            }

            if (ProblemID == -1)
            {
                response.Success = false;
                response.Message = "Идентификатор задачи некорректен.";
                return Json(response);
            }

            try
            {
                repository.UnbindProblemFromTournament(TournamentID, ProblemID);

                response.Success = true;
                response.Message = "Задача успешно удалена из турнира.";

                logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                    " \"" + User.Identity.Name + "\" unbind problem \"" + ProblemID + "\" from tournament \"" + TournamentID + "\" ");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Произошла ошибка при удалении задачи из турнира.";

                logger.Warn("Error occurred on user " + WebSecurity.GetUserId(User.Identity.Name) + 
                    " \"" + User.Identity.Name + "\" unbinding problem \"" + ProblemID + "\" from tournament \"" + TournamentID + "\": ", ex);
            }

            return Json(response);
        }

        #endregion
        
        #region Bind Users To Tournament Methods

        /// <summary>
        /// Return two lists:
        ///   1)  a list of Users not bound to the tournament
        ///   2)  a list of Users granted to the tournament
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult BindUsersToTournament(int TournamentID = -1, int Page = 1, int PageSize = 25, string FilterBy = "all", string SearchTerm = null)
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited TournamentsManagement/Tournament/BindProblemsToTournament");

            if (TournamentID == -1)
            {
                ManageTournamentsViewModel viewModel = new ManageTournamentsViewModel();
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
                    if (FilterBy == "all" || string.IsNullOrEmpty(SearchTerm))
                    {
                        viewModel.PaginatedTournamentList = repository.Tournaments
                                .OrderByDescending(t => t.TournamentID)
                                .ToPaginatedList<Tournament>(Page, PageSize);
                    }
                    else if (!string.IsNullOrEmpty(SearchTerm))
                    {
                        string query = SearchTerm + "%";
                        if (FilterBy == "name")
                        {
                            viewModel.PaginatedTournamentList = repository.Tournaments
                                .Where(t => t.Name.ToLower().IndexOf(SearchTerm.ToLower()) != -1)
                                .OrderByDescending(t => t.TournamentID)
                                .ToPaginatedList<Tournament>(Page, PageSize);
                        }
                    }
                }
                return View("UsersForTournament", viewModel);
            }

            BindUsersToTournamentViewModel model = new BindUsersToTournamentViewModel();
            Tournament tournament = repository.Tournaments
                .FirstOrDefault(t => t.TournamentID == TournamentID);
            if (tournament == null)
            {
                logger.Warn("Tournament with id = " + TournamentID + " not found");
                throw new HttpException(404, "Tournament not found");
            }

            model.TournamentID = tournament.TournamentID;
            model.TournamentName = tournament.Name;

            // AsEnumerable() need, because Except() for IQueryable work in DB 
            // and return 'not supported' (really hard to work) data.
            if (tournament.Users != null)
            {
                model.AvailableUsers = new SelectList(repository.Users.AsEnumerable().Except(tournament.Users), "UserId", "UserName");
                model.BoundUsers = new SelectList(tournament.Users, "UserId", "UserName");
            }
            else
            {
                model.AvailableUsers = new SelectList(repository.Problems.AsEnumerable(), "UserId", "UserName");
                model.BoundUsers = null;
            }

            return View(model);
        }

        /// <summary>
        /// Grant the selected roles to the user.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="roleNames"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult BindUserToTournament(int TournamentID = -1, int UserID = -1)
        {
            JsonResponse response = new JsonResponse();

            if (TournamentID == -1)
            {
                response.Success = false;
                response.Message = "Идентификатор турнира некорректен.";
                return Json(response);
            }

            if (UserID == -1)
            {
                response.Success = false;
                response.Message = "Идентификатор пользователя некорректен.";
                return Json(response);
            }

            try
            {
                repository.BindUserToTournament(TournamentID, UserID);

                response.Success = true;
                response.Message = "Пользователь успешно добавлен в турнир.";

                logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                    " \"" + User.Identity.Name + "\" bind user \"" + UserID + "\" to tournament \"" + TournamentID + "\" ");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Произошла ошибка при добавлении пользователя в турнир.";

                logger.Warn("Error occurred on user " + WebSecurity.GetUserId(User.Identity.Name) + 
                    " \"" + User.Identity.Name + "\" binding user \"" + UserID + "\" to tournament \"" + TournamentID + "\": {3} ", ex);
            }

            return Json(response);
        }

        /// <summary>
        /// Revoke the selected roles for the user.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="roleNames"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UnbindUserFromTournament(int TournamentID = -1, int UserID = -1)
        {
            JsonResponse response = new JsonResponse();

            if (TournamentID == -1)
            {
                response.Success = false;
                response.Message = "Идентификатор турнира некорректен.";
                return Json(response);
            }

            if (UserID == -1)
            {
                response.Success = false;
                response.Message = "Идентификатор пользователя некорректен.";
                return Json(response);
            }

            try
            {
                repository.UnbindUserFromTournament(TournamentID, UserID);

                response.Success = true;
                response.Message = "Пользователь успешно удален из турнира.";

                logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) +
                    " \"" + User.Identity.Name + "\" unbind user \"" + UserID + "\" from tournament \"" + TournamentID + "\" ");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Произошла ошибка при удалении пользователя из турнира.";

                logger.Warn("Error occurred on user " + WebSecurity.GetUserId(User.Identity.Name) + 
                    " \"" + User.Identity.Name + "\" unbinding user \"" + UserID + "\" from tournament \"" + TournamentID + "\": ", ex);
            }

            return Json(response);
        }

        #endregion
    }
}
