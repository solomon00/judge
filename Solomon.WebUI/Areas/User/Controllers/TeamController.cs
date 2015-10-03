using log4net;
using log4net.Config;
using Solomon.Domain.Abstract;
using Solomon.Domain.Entities;
using Solomon.TypesExtensions;
using Solomon.WebUI.Areas.User.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace Solomon.WebUI.Areas.User.Controllers
{
    [Authorize]
    public class TeamController : Controller
    {
        private IRepository repository;
        private readonly ILog logger = LogManager.GetLogger(typeof(ProfileController));
        /// <summary>
        /// Controller constructor.
        /// Initialize repository.
        /// </summary>
        /// <param name="Repository">Repository object</param>
        public TeamController(IRepository Repository)
        {
            XmlConfigurator.Configure();
            repository = Repository;
        }


        //
        // GET: /Account/Team

        public ActionResult Index()
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

            TeamViewModel viewModel = new TeamViewModel();

            List<Team> teams = new List<Team>();
            user.Teams.Each(t =>
            {
                if (t.Confirm == 1)
                    teams.Add(new Team() { TeamID = t.TeamID, Name = t.Team.Name, Members = t.Team.Members });
            });

            viewModel.Teams = teams;

            return View(viewModel);
        }

        public ActionResult Invites()
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

            InvitesViewModel viewModel = new InvitesViewModel();

            List<Team> invites = new List<Team>();
            user.Teams.Each(t =>
            {
                if (t.Confirm == 0)
                    invites.Add(new Team() { TeamID = t.TeamID, Name = t.Team.Name, Members = t.Team.Members });
            });

            viewModel.Invites = invites;

            return View(viewModel);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View(new NewTeamViewModel());
        }

        [HttpPost]
        public ActionResult Create(string Name)
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

            if (Name == null || Name == "")
            {
                TempData["ErrorMessage"] = "Некорректное имя команды";
                return Create();
            }

            Team team = repository.Teams.FirstOrDefault(t => t.Name == Name);

            if (team != null)
            {
                TempData["ErrorMessage"] = "Команда с таким имененм уже существует";
                return Create();
            }

            team = new Team() { Name = Name };

            repository.AddTeam(team);

            UserProfileTeam userProfileTeam = new UserProfileTeam() { UserID = userID, TeamID = team.TeamID, Confirm = 1 };

            repository.AddUserProfileTeam(userProfileTeam);

            TempData["SuccessMessage"] = "Команда успешно создана";
            return RedirectToAction("Manage", new { TeamID = team.TeamID });
        }

        public ActionResult Manage(int TeamID = -1)
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
                t.TeamID == TeamID &&
                t.Members.FirstOrDefault(up => up.UserID == userID && up.Confirm == 1) != null);

            if (team == null)
            {
                //return RedirectToAction("Team", new { Message = TeamMessageId.TeamNotExist });
            }

            ManageTeamViewModel viewModel = new ManageTeamViewModel()
            {
                TeamID = team.TeamID,
                Name = team.Name,
                Members = team.Members
            };

            return View(viewModel);
        }

        [HttpPost]
        public JsonResult ChangeTeamName(string Name, int TeamID)
        {
            Team team = repository.Teams.FirstOrDefault(t => t.TeamID == TeamID);
            Solomon.WebUI.Models.JsonResponse response = new Solomon.WebUI.Models.JsonResponse();
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
                    u.UserId != 1 &&
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

            Solomon.WebUI.Models.JsonResponse response = new Solomon.WebUI.Models.JsonResponse();
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

                response.Message = "Приглашение для " + UserName + " отправлено";
                response.Success = true;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult InviteResponse(int TeamID = -1, string Accept = null, string Reject = null)
        {
            int userID = WebSecurity.CurrentUserId;
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
                TempData["ErrorMessage"] = "Команда не найдена";
                return RedirectToAction("Index");
            }

            if (Accept != null)
            {
                UserProfileTeam userProfileTeam = repository
                    .UserProfileTeam
                    .FirstOrDefault(ut => ut.UserID == userID && ut.TeamID == TeamID);

                if (userProfileTeam == null)
                {
                    logger.Warn("UserProfileTeam with userID = " + userID + " and teamID = " + TeamID + " not found");
                    TempData["ErrorMessage"] = "Команда не найдена";
                    return RedirectToAction("Index");
                }

                userProfileTeam.Confirm = 1;

                repository.AddUserProfileTeam(userProfileTeam);
                TempData["SuccessMessage"] = "Вы приняли приглашение в команду " + team.Name;
                return RedirectToAction("Index");
            }
            if (Reject != null)
            {
                UserProfileTeam userProfileTeam = repository
                    .UserProfileTeam
                    .FirstOrDefault(ut => ut.UserID == userID && ut.TeamID == TeamID);

                if (userProfileTeam == null)
                {
                    logger.Warn("UserProfileTeam with userID = " + userID + " and teamID = " + TeamID + " not found");
                    TempData["ErrorMessage"] = "Команда не найдена";
                    return RedirectToAction("Index");
                }

                userProfileTeam.Confirm = -1;

                repository.AddUserProfileTeam(userProfileTeam);
                TempData["SuccessMessage"] = "Вы отклонили приглашение в команду " + team.Name;
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Произошла ошибка при обработке ответа";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Partial view for members table
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetMembersData(int TeamID)
        {
            var response = new JsonResponseMembersData() { Success = true };

            List<UserProfileTeam> members = repository.UserProfileTeam
                .Where(ut => ut.TeamID == TeamID).ToList();

            if (members.Count == 0)
            {
                response.Success = false;
            }

            // Get text view.
            using (var sw = new StringWriter())
            {
                ViewData["Members"] = members;

                var viewResult = ViewEngines.Engines.FindPartialView(this.ControllerContext, "GetMembersData");
                var viewContext = new ViewContext(this.ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(this.ControllerContext, viewResult.View);
                response.HtmlTable = sw.GetStringBuilder().ToString();
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }


    }
}
