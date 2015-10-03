using log4net;
using log4net.Config;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Solomon.Domain.Abstract;
using Solomon.Domain.Entities;
using Solomon.TypesExtensions;
using Solomon.WebUI.Helpers;
using Solomon.WebUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebMatrix.WebData;

namespace Solomon.WebUI.Controllers
{
    public class TournamentController : Controller
    {
        private IRepository repository;
        private readonly ILog logger = LogManager.GetLogger(typeof(TournamentController));
        
        public TournamentController(IRepository Repository)
        {
            XmlConfigurator.Configure();
            repository = Repository;
        }

        public ViewResult List()
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited Tournament/List");

            TournamentListViewModel viewModel = new TournamentListViewModel();

            viewModel.ActiveTournaments = repository
                .Tournaments
                .Where(t => t.StartDate <= DateTime.Now && t.EndDate > DateTime.Now)
                .OrderByDescending(t => t.StartDate)
                .AsEnumerable();

            viewModel.NotBegunTournaments = repository
                .Tournaments
                .Where(t => t.StartDate > DateTime.Now)
                .OrderByDescending(t => t.StartDate)
                .AsEnumerable();

            viewModel.FinishTournaments = repository
                .Tournaments
                .Where(t => t.EndDate < DateTime.Now)
                .OrderByDescending(t => t.StartDate)
                .AsEnumerable();

            if (!Roles.IsUserInRole("Administrator"))
            {
                UserProfile user = repository.Users.FirstOrDefault(u => u.UserId == WebSecurity.CurrentUserId);

                if (user != null && user.Generated == 1)
                {
                    viewModel.ActiveTournaments = user.Tournaments
                    .AsEnumerable()
                    .Where(t => t.StartDate <= DateTime.Now && t.EndDate > DateTime.Now)
                    .Where(t =>
                        t.Type == TournamentTypes.Close
                            ? t.Users.Contains(u => u.UserId == WebSecurity.GetUserId(User.Identity.Name))
                            : true)
                    .OrderByDescending(t => t.StartDate);

                    viewModel.NotBegunTournaments = user.Tournaments
                        .AsEnumerable()
                        .Where(t => t.StartDate > DateTime.Now)
                        .Where(t =>
                            t.Type == TournamentTypes.Close
                                ? t.Users.Contains(u => u.UserId == WebSecurity.GetUserId(User.Identity.Name))
                                : true)
                        .OrderByDescending(t => t.StartDate);

                    viewModel.FinishTournaments = user.Tournaments
                        .AsEnumerable()
                        .Where(t => t.EndDate < DateTime.Now)
                        .Where(t =>
                            t.Type == TournamentTypes.Close
                                ? t.Users.Contains(u => u.UserId == WebSecurity.GetUserId(User.Identity.Name))
                                : true)
                        .OrderByDescending(t => t.StartDate);
                }
                else
                {
                    viewModel.ActiveTournaments = viewModel.ActiveTournaments
                        .Where(t => t.StartDate <= DateTime.Now && t.EndDate > DateTime.Now)
                        .Where(t =>
                            t.Type == TournamentTypes.Close
                                ? t.Users.Contains(u => u.UserId == WebSecurity.GetUserId(User.Identity.Name))
                                : true)
                        .OrderByDescending(t => t.StartDate)
                        .AsEnumerable();

                    viewModel.NotBegunTournaments = viewModel.NotBegunTournaments
                        .Where(t => t.StartDate > DateTime.Now)
                        .Where(t =>
                            t.Type == TournamentTypes.Close
                                ? t.Users.Contains(u => u.UserId == WebSecurity.GetUserId(User.Identity.Name))
                                : true)
                        .OrderByDescending(t => t.StartDate)
                        .AsEnumerable();

                    viewModel.FinishTournaments = viewModel.FinishTournaments
                        .Where(t => t.EndDate < DateTime.Now)
                        .Where(t =>
                            t.Type == TournamentTypes.Close
                                ? t.Users.Contains(u => u.UserId == WebSecurity.GetUserId(User.Identity.Name))
                                : true)
                        .OrderByDescending(t => t.StartDate)
                        .AsEnumerable();
                }
            }

            return View(viewModel);
        }

        [Authorize]
        [HttpGet]
        public ViewResult Register(string ReturnUrl, int TournamentID = -1)
        {
            ViewBag.ReturnUrl = ReturnUrl;

            int userID = WebSecurity.GetUserId(User.Identity.Name);
            UserProfile user = repository.Users.FirstOrDefault(u => u.UserId == userID);

            Tournament tournament = repository
                .Tournaments
                .FirstOrDefault(t => t.TournamentID == TournamentID);

            if (tournament == null)
            {
                logger.Warn("Tournament with id = " + TournamentID + " not found");
                throw new HttpException(404, "Tournament with id = " + TournamentID + " not found");
            }

            List<SelectListItem> teams = new List<SelectListItem>();
            teams.Add(new SelectListItem() { Value = "0", Text = "Как индивидуальный участник" });
            user.Teams.Each(t => teams.Add(new SelectListItem() { Value = t.UserProfileTeamID.ToString(), Text = "В составе команды " + t.Team.Name }));

            RegisterForTournamentViewModel viewModel = new RegisterForTournamentViewModel()
                {
                    tournament = tournament,
                    TeamList = teams
                };

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Register(string ReturnUrl, int TournamentID = -1, int UserProfileTeamID = -1, string Register = null, string Cancel = null)
        {
            ViewBag.ReturnUrl = ReturnUrl;

            int userID = WebSecurity.GetUserId(User.Identity.Name);
            UserProfile user = repository.Users.FirstOrDefault(u => u.UserId == userID);

            Tournament tournament = repository
                .Tournaments
                .FirstOrDefault(t => t.TournamentID == TournamentID);

            if (tournament == null)
            {
                logger.Warn("Tournament with id = " + TournamentID + " not found");
                throw new HttpException(404, "Tournament with id = " + TournamentID + " not found");
            }

            if (Register != null)
            {
                if (UserProfileTeamID == 0)
                {
                    tournament.Users.Add(user);
                    repository.AddTournament(tournament);

                    return RedirectToLocal(ReturnUrl);
                }

                UserProfileTeam userProfileTeam = repository
                    .UserProfileTeam
                    .FirstOrDefault(ut => ut.UserProfileTeamID == UserProfileTeamID);

                if (userProfileTeam == null)
                {
                    logger.Warn("UserProfileTeam with id = " + UserProfileTeamID + " not found");
                    throw new HttpException(404, "UserProfileTeam with id = " + UserProfileTeamID + " not found");
                }

                tournament.Teams.Add(userProfileTeam);
                repository.AddTournament(tournament);

                return RedirectToLocal(ReturnUrl);
            }
            if (Cancel != null)
            {
                return RedirectToLocal(ReturnUrl);
            }

            return RedirectToLocal(ReturnUrl);
        }

        public ViewResult Results(int TournamentID)
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited Tournament/Results");

            var tournament = repository
                .Tournaments
                .FirstOrDefault(t => t.TournamentID == TournamentID);

            if (tournament == null)
            {
                logger.Warn("Tournament with id = " + TournamentID + " not found");
                throw new HttpException(404, "Tournament not found");
            }

            int userID = WebSecurity.CurrentUserId;
            TournamentResultsViewModel viewModel = new TournamentResultsViewModel()
                {
                    TournamentID = tournament.TournamentID,
                    TournamentName = tournament.Name,
                    TournamentStartDate = tournament.StartDate,
                    TournamentEndDate = tournament.EndDate,
                    CurrentTime = DateTime.Now,
                    TF = tournament.Format,

                    ShowSolutionSendingTime = tournament.ShowSolutionSendingTime,
                    ShowTimer = tournament.ShowTimer,

                    Problems = tournament.Problems.OrderBy(p => p.ProblemID),

                    CanExportResults = User.IsInRole("Administrator") || tournament.UsersCanModify.FirstOrDefault(u => u.UserId == userID) != null
                };

            // Change problem name to number.
            int i = 1;
            foreach (var problem in viewModel.Problems)
            {
                problem.Name = i.ToAlpha();
                i++;
            }

            IEnumerable<ParticipantTournamentResult> userTournamentResults = null;
            if (tournament.Format == TournamentFormats.ACM)
            {
                ACMResults(TournamentID, out userTournamentResults);
            }
            else if (tournament.Format == TournamentFormats.IOI)
            {
                IOIResults(TournamentID, out userTournamentResults);
            }

            

            viewModel.TournamentResults = userTournamentResults;

            return View(viewModel);
        }

        public ViewResult Statistic(int TournamentID)
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) +
                " \"" + User.Identity.Name + "\" visited Tournament/Statistic");
            
            var tournament = repository
                .Tournaments
                .FirstOrDefault(t => t.TournamentID == TournamentID);

            if (tournament == null)
            {
                logger.Warn("Tournament with id = " + TournamentID + " not found");
                throw new HttpException(404, "Tournament not found");
            }

            int userID = WebSecurity.CurrentUserId;

            TournamentStatisticViewModel viewModel = new TournamentStatisticViewModel()
            {
                TournamentID = tournament.TournamentID,
                TournamentName = tournament.Name
            };

            return View(viewModel);
        }

        [HttpGet]
        public JsonResult GetSolutionsStatistic(int TournamentID)
        {
            var tournament = repository
                .Tournaments
                .FirstOrDefault(t => t.TournamentID == TournamentID);

            if (tournament == null)
            {
                logger.Warn("Tournament with id = " + TournamentID + " not found");
                throw new HttpException(404, "Tournament not found");
            }

            int userID = WebSecurity.CurrentUserId;

            JsonSolutions response = new JsonSolutions();
            List<Solution> solutions = new List<Solution>();
            repository.Solutions
                .Where(s => s.TournamentID == TournamentID)
                .Each(s =>
                    {
                        if (!Roles.IsUserInRole(s.User.UserName, "Administrator") && !Roles.IsUserInRole(s.User.UserName, "Judge"))
                        {
                            solutions.Add(s);
                        }
                    });

            DateTime minValue = tournament.StartDate;
            int groupSeconds = (int)(tournament.EndDate - tournament.StartDate).TotalSeconds / 10;
            int drilldownGroupSeconds = groupSeconds / 10;
            var groupedSolutions = solutions.GroupBy(s => minValue.AddSeconds((int)(s.SendTime - minValue).TotalSeconds / groupSeconds * groupSeconds));

            foreach (var group in groupedSolutions)
            {
                response.SolutionsData.Add(new SolutionsData() 
                    { 
                        name = group.Key.AddSeconds(groupSeconds / 2).ToString(), 
                        y = group.Count(),
                        drilldown = group.Key.AddSeconds(groupSeconds / 2).ToString() 
                    });

                var drilldownGroups = group.GroupBy(s => minValue.AddSeconds((int)(s.SendTime - minValue).TotalSeconds / drilldownGroupSeconds * drilldownGroupSeconds));
                
                response.Drilldown.Add(new Drilldown()
                    {
                        name = group.Key.AddSeconds(groupSeconds / 2).ToString(),
                        data = drilldownGroups.Select(g => new { Key = g.Key.AddSeconds(drilldownGroupSeconds / 2).ToString(), Value = g.Count() }),
                        id = group.Key.AddSeconds(groupSeconds / 2).ToString()
                    });
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void ExportResultsToExcel(int TournamentID,
            string IncludeName = null,
            string IncludeFIO = null,
            string IncludeBirthDay = null,
            string IncludePhoneNumber = null,
            string IncludeEmail = null,
            string IncludeCountry = null,
            string IncludeCity = null,
            string IncludeInstitution = null,
            string IncludeCategory = null,
            string IncludeGradeLevel = null,
            string IncludeProblemsResults = null,
            string IncludeTotalScoreIOI = null,
            string IncludeTotalAcceptedACM = null,
            string IncludeTotalPenaltiesACM = null
            )
        {
            logger.Info("User " + WebSecurity.CurrentUserId +
                " \"" + User.Identity.Name + "\" download results for tournament " + TournamentID);

            var tournament = repository
                .Tournaments
                .FirstOrDefault(t => t.TournamentID == TournamentID);

            if (tournament == null)
            {
                logger.Warn("Tournament with id = " + TournamentID + " not found");
                throw new HttpException(404, "Tournament not found");
            }

            IEnumerable<ParticipantTournamentResult> userTournamentResults = null;

            if (tournament.Format == TournamentFormats.IOI)
            {
                IOIResults(TournamentID, out userTournamentResults);
            }
            else if (tournament.Format == TournamentFormats.ACM)
            {
                ACMResults(TournamentID, out userTournamentResults);
            }

            using (ExcelPackage pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add(tournament.Name);

                int row = 1, col = 1;
                ws.Cells[row, col].Value = "Место";
                ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                col++;

                if (IncludeName != null)
                {
                    ws.Cells[row, col].Value = "Участник";
                    ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    ws.Column(col).Width = 20;
                    col++;
                }

                if (IncludeEmail != null)
                {
                    ws.Cells[row, col].Value = "Email";
                    ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    ws.Column(col).Width = 25;
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
                    ws.Column(col).Width = 20;
                    col++;
                }

                if (IncludeCity != null)
                {
                    ws.Cells[row, col].Value = "Город";
                    ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    ws.Column(col).Width = 20;
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

                if (IncludeProblemsResults != null)
                {
                    for (int i = 1; i < tournament.Problems.Count + 1; i++)
                    {
                        ws.Cells[row, col].Value = i.ToAlpha();
                        ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        col++;
                    }
                }

                if (tournament.Format == TournamentFormats.IOI)
                {
                    if (IncludeTotalScoreIOI != null)
                    {
                        ws.Cells[row, col].Value = "Всего";
                        ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        col++;
                    }
                }
                else if (tournament.Format == TournamentFormats.ACM)
                {
                    if (IncludeTotalAcceptedACM != null)
                    {
                        ws.Cells[row, col].Value = "Всего зачтено";
                        ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ws.Column(col).Width = 15;
                        col++;
                    }

                    if (IncludeTotalPenaltiesACM != null)
                    {
                        ws.Cells[row, col].Value = "Всего штрафных";
                        ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ws.Column(col).Width = 15;
                        col++;
                    }
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

                foreach (var utr in userTournamentResults)
                {
                    row++;
                    int usersCount = utr.Users.Count() - 1;
                    col = 1;

                    // Place
                    if (usersCount > 0)
                        ws.Cells[row, col, row + usersCount, col].Merge = true;
                    ws.Cells[row, col, row + usersCount, col].Value = utr.Place;
                    ws.Cells[row, col, row + usersCount, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    ws.Cells[row, col, row + usersCount, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[row, col, row + usersCount, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    col++;

                    // Name
                    if (IncludeName != null)
                    {
                        if (usersCount > 0)
                            ws.Cells[row, col, row + usersCount, col].Merge = true;
                        ws.Cells[row, col, row + usersCount, col].Value = utr.Name;
                        ws.Cells[row, col, row + usersCount, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ws.Cells[row, col, row + usersCount, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[row, col, row + usersCount, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        col++;
                    }

                    // Email
                    if (IncludeEmail != null)
                    {
                        int i = 0;
                        foreach (var u in utr.Users)
                        {
                            ws.Cells[row + i, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            ws.Cells[row + i++, col].Value = u.Email;
                        }
                        col++;
                    }

                    // FIO
                    if (IncludeFIO != null)
                    {
                        int i = 0;
                        foreach (var u in utr.Users)
                        {
                            ws.Cells[row + i, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            ws.Cells[row + i++, col].Value = (u.SecondName + " " + u.FirstName + " " + u.ThirdName).Trim();
                        }
                        col++;
                    }

                    // BirthDay
                    if (IncludeBirthDay != null)
                    {
                        int i = 0;
                        foreach (var u in utr.Users)
                        {
                            ws.Cells[row + i, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            ws.Cells[row + i++, col].Value = u.BirthDay != null ? ((DateTime)u.BirthDay).ToString("dd.MM.yyyy") : String.Empty;
                        }
                        col++;
                    }

                    // Phone number
                    if (IncludeBirthDay != null)
                    {
                        int i = 0;
                        foreach (var u in utr.Users)
                        {
                            ws.Cells[row + i, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            ws.Cells[row + i++, col].Value = u.PhoneNumber;
                        }
                        col++;
                    }

                    // Country
                    if (IncludeCountry != null)
                    {
                        int i = 0;
                        foreach (var u in utr.Users)
                        {
                            // Country entity don't loaded from u.Country, becouse it's IEnumarable collection
                            var country = repository.Country.FirstOrDefault(c => c.CountryID == u.CountryID);
                            ws.Cells[row + i, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            ws.Cells[row + i++, col].Value = country != null ? country.Name : String.Empty;
                        }
                        col++;
                    }

                    // City
                    if (IncludeCity != null)
                    {
                        int i = 0;
                        foreach (var u in utr.Users)
                        {
                            var city = repository.City.FirstOrDefault(c => c.CityID == u.CityID);
                            ws.Cells[row + i, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            ws.Cells[row + i++, col].Value = city != null ? (city.Name + " " + city.Area + " " + city.Region) : String.Empty;
                        }
                        col++;
                    }

                    // Institution
                    if (IncludeInstitution != null)
                    {
                        int i = 0;
                        foreach (var u in utr.Users)
                        {
                            var institution = repository.Institutions.FirstOrDefault(ins => ins.InstitutionID == u.InstitutionID);
                            ws.Cells[row + i, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            ws.Cells[row + i++, col].Value = institution != null ? institution.Name : String.Empty;
                        }
                        col++;
                    }

                    // Category
                    if (IncludeCategory != null)
                    {
                        int i = 0;
                        foreach (var u in utr.Users)
                        {
                            ws.Cells[row + i, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            string category = "";
                            switch (u.Category)
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
                            ws.Cells[row + i++, col].Value = category;
                        }
                        col++;
                    }

                    // Grade level
                    if (IncludeGradeLevel != null)
                    {
                        int i = 0;
                        foreach (var u in utr.Users)
                        {
                            ws.Cells[row + i, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            ws.Cells[row + i++, col].Value = u.GradeLevel;
                        }
                        col++;
                    }

                    // Problems results
                    if (IncludeProblemsResults != null)
                    {
                        foreach (var pr in utr.ProblemsResults)
                        {
                            string color = "transparent";
                            string text = null;
                            if (tournament.Format == TournamentFormats.IOI)
                                text = pr.Penalties > 0 ? pr.Score.ToString() : String.Empty;
                            else if (tournament.Format == TournamentFormats.ACM)
                                text = pr.Accept ? "+" : "-" + pr.Penalties;
                            if (pr.Accept == true)
                            {
                                color = "#9BE69B";
                            }
                            else if (pr.Penalties > 0)
                            {
                                if (pr.Score > 0)
                                {
                                    color = "#FFD798";
                                }
                                else
                                {
                                    color = "#FF9090";
                                }
                            }
                            else
                            {
                                text = "";
                            }

                            if (usersCount > 0)
                                ws.Cells[row, col, row + usersCount, col].Merge = true;
                            ws.Cells[row, col, row + usersCount, col].Value = text;
                            ws.Cells[row, col, row + usersCount, col].Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                            ws.Cells[row, col, row + usersCount, col].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml(color));  //Set color to dark blue
                            ws.Cells[row, col, row + usersCount, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            ws.Cells[row, col, row + usersCount, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws.Cells[row, col, row + usersCount, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            col++;
                        }
                    }

                    if (tournament.Format == TournamentFormats.IOI)
                    {
                        // Total score
                        if (IncludeTotalScoreIOI != null)
                        {
                            if (usersCount > 0)
                                ws.Cells[row, col, row + usersCount, col].Merge = true;
                            ws.Cells[row, col, row + usersCount, col].Value = utr.TotalScore;
                            ws.Cells[row, col, row + usersCount, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            ws.Cells[row, col, row + usersCount, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws.Cells[row, col, row + usersCount, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            col++;
                        }
                    }
                    else if (tournament.Format == TournamentFormats.ACM)
                    {
                        // Total accepted
                        if (IncludeTotalAcceptedACM != null)
                        {
                            if (usersCount > 0)
                                ws.Cells[row, col, row + usersCount, col].Merge = true;
                            ws.Cells[row, col, row + usersCount, col].Value = utr.TotalAccepted;
                            ws.Cells[row, col, row + usersCount, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            ws.Cells[row, col, row + usersCount, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws.Cells[row, col, row + usersCount, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            col++;
                        }

                        // Total penalties
                        if (IncludeTotalPenaltiesACM != null)
                        {
                            if (usersCount > 0)
                                ws.Cells[row, col, row + usersCount, col].Merge = true;
                            ws.Cells[row, col, row + usersCount, col].Value = utr.TotalPenalties;
                            ws.Cells[row, col, row + usersCount, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            ws.Cells[row, col, row + usersCount, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws.Cells[row, col, row + usersCount, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            col++;
                        }
                    }

                    row += usersCount;
                }


                ////Example how to Format Column 1 as numeric 
                //using (ExcelRange col = ws.Cells[2, 1, 2 + tbl.Rows.Count, 1])
                //{
                //    col.Style.Numberformat.Format = "#,##0.00";
                //    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                //}

                //Write it back to the client
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;  filename=" + tournament.Name + ".xlsx");
                Response.BinaryWrite(pck.GetAsByteArray());
            }
            return;
        }


        private void GetUsersSolutions(int TournamentID, out IEnumerable<ParticipantSolutionResult> UsersSolutions)
        {
            var tournament = repository
                .Tournaments
                .FirstOrDefault(t => t.TournamentID == TournamentID);

            if (tournament == null)
            {
                logger.Warn("Tournament with id = " + TournamentID + " not found");
                throw new HttpException(404, "Tournament not found");
            }

            List<ParticipantSolutionResult> usersSolutions = new List<ParticipantSolutionResult>();
            repository
                .Solutions
                .Where(s => s.TournamentID == TournamentID &&
                    s.Result != TestResults.Disqualified)
                .AsEnumerable()
                .Where(s => tournament.Users.FirstOrDefault(u => u.UserId == s.UserID) != null &&
                    !Roles.IsUserInRole(s.User.UserName, "Judge") &&
                    !Roles.IsUserInRole(s.User.UserName, "Administrator")
                    )
                .Each(s =>
                {
                    usersSolutions.Add(new ParticipantSolutionResult()
                    {
                        ID = s.UserID,
                        ProblemID = s.ProblemID,
                        Online = repository.IsUserOnline(s.User),
                        Name = s.User.UserName,
                        FullName = (s.User.SecondName + " " + s.User.FirstName + " " + s.User.ThirdName).Trim(),
                        Users = new List<UserProfile>() { s.User },
                        Result = s.Result,
                        Score = s.Score,
                        SendTime = s.SendTime
                    });
                }
                );

            UsersSolutions = usersSolutions;
        }
        private void GetTeamsSolutions(int TournamentID, out IEnumerable<ParticipantSolutionResult> TeamsSolutions)
        {
            var tournament = repository
                .Tournaments
                .FirstOrDefault(t => t.TournamentID == TournamentID);

            if (tournament == null)
            {
                logger.Warn("Tournament with id = " + TournamentID + " not found");
                throw new HttpException(404, "Tournament not found");
            }

            List<ParticipantSolutionResult> teamsSolutions = new List<ParticipantSolutionResult>();
            repository
                .Solutions
                .Where(s => s.TournamentID == TournamentID &&
                    s.Result != TestResults.Disqualified)
                .AsEnumerable()
                .Where(s => tournament.Teams.FirstOrDefault(t => t.UserID == s.UserID) != null &&
                    !Roles.IsUserInRole(s.User.UserName, "Judge") &&
                    !Roles.IsUserInRole(s.User.UserName, "Administrator")
                    )
                //.Select(s => new ParticipantSolutionResult()
                //{
                //    ID = s.UserID,
                //    ProblemID = s.ProblemID,
                //    Name = s.User.UserName,
                //    FullName = (s.User.SecondName + " " + s.User.FirstName + " " + s.User.ThirdName).Trim(),
                //    Result = s.Result,
                //    Score = s.Score,
                //    SendTime = s.SendTime
                //})
                .Each(s =>
                {
                    var team = tournament.Teams.First(t => t.UserID == s.UserID).Team;
                    List<UserProfile> users = new List<UserProfile>();
                    string fullName = "";
                    team.Members
                        .Where(m => tournament.Teams.FirstOrDefault(t => t.UserID == m.UserID) != null)
                        .Each(m =>
                        {
                            if ((m.User.FirstName == null || m.User.FirstName == "") &&
                                (m.User.SecondName == null || m.User.SecondName == "") &&
                                (m.User.ThirdName == null || m.User.ThirdName == ""))
                                fullName += m.User.UserName + ",\n";
                            else
                                fullName += (m.User.SecondName + " " + m.User.FirstName + " " + m.User.ThirdName).Trim() + ",\n";

                            users.Add(m.User);
                        });
                    fullName = fullName.TrimEnd('\n').TrimEnd(',');

                    bool online = false;
                    users.Each(u => online = online || repository.IsUserOnline(u));

                    teamsSolutions.Add(new ParticipantSolutionResult()
                    {
                        ID = team.TeamID,
                        ProblemID = s.ProblemID,
                        Online = online,
                        Name = team.Name,
                        FullName = fullName.Replace("\n", "<br />"),
                        Users = users,
                        Result = s.Result,
                        Score = s.Score,
                        SendTime = s.SendTime
                    });
                }
                );

            TeamsSolutions = teamsSolutions;
        }

        private void ACMResults(int TournamentID, out IEnumerable<ParticipantTournamentResult> GroupResults)
        {
            var tournament = repository
                .Tournaments
                .FirstOrDefault(t => t.TournamentID == TournamentID);

            if (tournament == null)
            {
                logger.Warn("Tournament with id = " + TournamentID + " not found");
                throw new HttpException(404, "Tournament not found");
            }

            IEnumerable<ParticipantSolutionResult> UsersSolutions;
            IEnumerable<ParticipantSolutionResult> TeamsSolutions;

            GetUsersSolutions(TournamentID, out UsersSolutions);
            GetTeamsSolutions(TournamentID, out TeamsSolutions);

            #region Solutions grouped by users
            var usersSolutions = UsersSolutions.GroupBy(s => s.ID);

            // List contains users and each user results for all problems
            List<ParticipantTournamentResult> tempUsersTournamentResults = new List<ParticipantTournamentResult>();
            List<ParticipantProblemResult> tempUserProblemsResult;

            foreach (var userSolutions in usersSolutions)
            {
                var userProblemsSolutions = userSolutions
                    .GroupBy(s => s.ProblemID);

                // List of results for problems
                tempUserProblemsResult = new List<ParticipantProblemResult>();
                tournament.Problems.Each(p =>
                    tempUserProblemsResult.Add(new ParticipantProblemResult() { ProblemID = p.ProblemID })
                );
                foreach (var userProblemSolutions in userProblemsSolutions)
                {
                    var accept = userProblemSolutions.Contains(s => s.Result == TestResults.OK);
                    var acceptTime =
                        accept == true
                        ? userProblemSolutions.First(s => s.Result == TestResults.OK).SendTime
                        : DateTime.MinValue;
                    var penalties = userProblemSolutions.OrderBy(s => s.SendTime).TakeWhile(s => s.Result != TestResults.OK).Count();

                    tempUserProblemsResult.First(pr => pr.ProblemID == userProblemSolutions.First().ProblemID).Accept = accept;
                    tempUserProblemsResult.First(pr => pr.ProblemID == userProblemSolutions.First().ProblemID).AcceptTime = acceptTime - tournament.StartDate;
                    tempUserProblemsResult.First(pr => pr.ProblemID == userProblemSolutions.First().ProblemID).Penalties = penalties;
                }

                int totalPenalties = 0;
                tempUserProblemsResult.Each(r => totalPenalties +=
                    r.Accept == true
                    ? (int)r.AcceptTime.TotalMinutes + r.Penalties * 20
                    : 0);

                tempUsersTournamentResults.Add(new ParticipantTournamentResult()
                {
                    ID = userSolutions.First().ID,
                    Online = userSolutions.First().Online,
                    Name = userSolutions.First().Name,
                    FullName = userSolutions.First().FullName,
                    Users = userSolutions.First().Users,
                    ProblemsResults = tempUserProblemsResult,
                    TotalAccepted = tempUserProblemsResult.Count(r => r.Accept == true),
                    TotalPenalties = totalPenalties
                }
                );
            }
            #endregion

            #region Solutions grouped by teams
            var teamsSolutions = TeamsSolutions.GroupBy(s => s.ID);

            // List contains users and each user results for all problems
            List<ParticipantTournamentResult> tempTeamsTournamentResults = new List<ParticipantTournamentResult>();
            List<ParticipantProblemResult> tempTeamProblemsResult;

            foreach (var teamSolutions in teamsSolutions)
            {
                var teamProblemsSolutions = teamSolutions
                    .GroupBy(s => s.ProblemID);

                // List of results for problems
                tempTeamProblemsResult = new List<ParticipantProblemResult>();
                tournament.Problems.Each(p =>
                    tempTeamProblemsResult.Add(new ParticipantProblemResult() { ProblemID = p.ProblemID })
                );
                foreach (var teamProblemSolutions in teamProblemsSolutions)
                {
                    var accept = teamProblemSolutions.Contains(s => s.Result == TestResults.OK);
                    var acceptTime =
                        accept == true
                        ? teamProblemSolutions.First(s => s.Result == TestResults.OK).SendTime
                        : DateTime.MinValue;
                    var penalties = teamProblemSolutions.OrderBy(s => s.SendTime).TakeWhile(s => s.Result != TestResults.OK).Count();

                    tempTeamProblemsResult.First(pr => pr.ProblemID == teamProblemSolutions.First().ProblemID).Accept = accept;
                    tempTeamProblemsResult.First(pr => pr.ProblemID == teamProblemSolutions.First().ProblemID).AcceptTime = acceptTime - tournament.StartDate;
                    tempTeamProblemsResult.First(pr => pr.ProblemID == teamProblemSolutions.First().ProblemID).Penalties = penalties;
                }

                int totalPenalties = 0;
                tempTeamProblemsResult.Each(r => totalPenalties +=
                    r.Accept == true
                    ? (int)r.AcceptTime.TotalMinutes + r.Penalties * 20
                    : 0);

                tempTeamsTournamentResults.Add(new ParticipantTournamentResult()
                {
                    ID = teamSolutions.First().ID,
                    Online = teamSolutions.First().Online,
                    Name = teamSolutions.First().Name,
                    FullName = teamSolutions.First().FullName,
                    Users = teamSolutions.First().Users,
                    ProblemsResults = tempTeamProblemsResult,
                    TotalAccepted = tempTeamProblemsResult.Count(r => r.Accept == true),
                    TotalPenalties = totalPenalties
                }
                );
            }
            #endregion

            // Add users who have not sent solutions
            tempUserProblemsResult = new List<ParticipantProblemResult>();
            tournament.Problems.Each(p =>
                tempUserProblemsResult.Add(new ParticipantProblemResult() { ProblemID = p.ProblemID })
            );
            tournament.Users.Each(u =>
            {
                if (!Roles.IsUserInRole(u.UserName, "Judge") &&
                    !Roles.IsUserInRole(u.UserName, "Administrator"))
                {
                    if (!tempUsersTournamentResults.Contains(tr => tr.ID == u.UserId))
                    {
                        tempUsersTournamentResults.Add(new ParticipantTournamentResult()
                        {
                            ID = u.UserId,
                            Online = repository.IsUserOnline(u),
                            Name = u.UserName,
                            FullName = (u.SecondName + " " + u.FirstName + " " + u.ThirdName).Trim(),
                            Users = new List<UserProfile>() { u },
                            ProblemsResults = tempUserProblemsResult,
                            TotalAccepted = 0,
                            TotalPenalties = 0
                        });
                    }
                }
            });

            // Add teams who have not sent solutions
            tempTeamProblemsResult = new List<ParticipantProblemResult>();
            tournament.Problems.Each(p =>
                tempTeamProblemsResult.Add(new ParticipantProblemResult() { ProblemID = p.ProblemID })
            );
            tournament.Teams.GroupBy(ut => ut.TeamID).Each(t =>
            {
                if (!tempTeamsTournamentResults.Contains(tr => tr.ID == t.First().TeamID))
                {
                    string fullName = "";
                    List<UserProfile> users = new List<UserProfile>();
                    t.Each(ut =>
                    {
                        if ((ut.User.FirstName == null || ut.User.FirstName == "") &&
                            (ut.User.SecondName == null || ut.User.SecondName == "") &&
                            (ut.User.ThirdName == null || ut.User.ThirdName == ""))
                            fullName += ut.User.UserName + ",\n";
                        else
                            fullName += (ut.User.SecondName + " " + ut.User.FirstName + " " + ut.User.ThirdName).Trim() + ",\n";

                        users.Add(ut.User);
                    });
                    fullName = fullName.TrimEnd('\n').TrimEnd(',');

                    bool online = false;
                    users.Each(u => online = online || repository.IsUserOnline(u));

                    tempTeamsTournamentResults.Add(new ParticipantTournamentResult()
                    {
                        ID = t.First().TeamID,
                        Online = online,
                        Name = t.First().Team.Name,
                        FullName = fullName.Replace("\n", "<br />"),
                        Users = users,
                        ProblemsResults = tempTeamProblemsResult,
                        TotalScore = 0
                    });
                }
            });

            List<ParticipantTournamentResult> tempTournamentResults = new List<ParticipantTournamentResult>();
            tempTournamentResults.AddRange(tempUsersTournamentResults);
            tempTournamentResults.AddRange(tempTeamsTournamentResults);

            GroupResults = tempTournamentResults.OrderByDescending(r => r.TotalAccepted).ThenBy(r => r.TotalPenalties);

            int i;
            if (GroupResults.Count() > 0)
            {
                i = 1;
                int accepted = GroupResults.First().TotalAccepted;
                int penalt = GroupResults.First().TotalPenalties;
                foreach (var result in GroupResults)
                {
                    if (accepted != result.TotalAccepted || penalt != result.TotalPenalties)
                        i++;

                    accepted = result.TotalAccepted;
                    penalt = result.TotalPenalties;
                    result.Place = i.ToString();
                }

            }
        }
        private void IOIResults(int TournamentID, out IEnumerable<ParticipantTournamentResult> GroupResults)
        {
            var tournament = repository
                .Tournaments
                .FirstOrDefault(t => t.TournamentID == TournamentID);

            if (tournament == null)
            {
                logger.Warn("Tournament with id = " + TournamentID + " not found");
                throw new HttpException(404, "Tournament not found");
            }

            IEnumerable<ParticipantSolutionResult> UsersSolutions;
            IEnumerable<ParticipantSolutionResult> TeamsSolutions;

            GetUsersSolutions(TournamentID, out UsersSolutions);
            GetTeamsSolutions(TournamentID, out TeamsSolutions);

            #region Solutions grouped by users
            var usersSolutions = UsersSolutions.GroupBy(s => s.ID);

            // List contains users and each user results for all problems
            List<ParticipantTournamentResult> tempUsersTournamentResults = new List<ParticipantTournamentResult>();
            List<ParticipantProblemResult> tempUserProblemsResult;

            foreach (var userSolutions in usersSolutions)
            {
                var userProblemsSolutions = userSolutions
                    .GroupBy(s => s.ProblemID);

                // List of results for problems
                tempUserProblemsResult = new List<ParticipantProblemResult>();
                tournament.Problems.Each(p =>
                    tempUserProblemsResult.Add(new ParticipantProblemResult() { ProblemID = p.ProblemID })
                );
                foreach (var userProblemSolutions in userProblemsSolutions)
                {
                    var accept = userProblemSolutions.Contains(s => s.Result == TestResults.OK);
                    var score = userProblemSolutions.OrderByDescending(s => s.Score).First().Score;
                    var acceptTime = userProblemSolutions.First(s => s.Score == score).SendTime;
                    var penalties = userProblemSolutions.Count();

                    var userProblemResult = tempUserProblemsResult.FirstOrDefault(pr => pr.ProblemID == userProblemSolutions.Key);
                    if (userProblemResult != null)
                    {
                        userProblemResult.Accept = accept;
                        userProblemResult.Score = score;
                        userProblemResult.AcceptTime = acceptTime - tournament.StartDate;
                        userProblemResult.Penalties = penalties;
                    }
                }

                int totalScore = 0;
                tempUserProblemsResult.Each(r => totalScore += r.Score);

                tempUsersTournamentResults.Add(new ParticipantTournamentResult()
                {
                    ID = userSolutions.First().ID,
                    Online = userSolutions.First().Online,
                    Name = userSolutions.First().Name,
                    FullName = userSolutions.First().FullName,
                    Users = userSolutions.First().Users,
                    ProblemsResults = tempUserProblemsResult,
                    TotalScore = totalScore
                }
                );
            }
            #endregion

            #region Solutions grouped by teams
            var teamsSolutions = TeamsSolutions.GroupBy(s => s.ID);

            // List contains users and each user results for all problems
            List<ParticipantTournamentResult> tempTeamsTournamentResults = new List<ParticipantTournamentResult>();
            List<ParticipantProblemResult> tempTeamProblemsResult;

            foreach (var teamSolutions in teamsSolutions)
            {
                var teamProblemsSolutions = teamSolutions
                    .GroupBy(s => s.ProblemID);

                // List of results for problems
                tempTeamProblemsResult = new List<ParticipantProblemResult>();
                tournament.Problems.Each(p =>
                    tempTeamProblemsResult.Add(new ParticipantProblemResult() { ProblemID = p.ProblemID })
                );
                foreach (var teamProblemSolutions in teamProblemsSolutions)
                {
                    var accept = teamProblemSolutions.Contains(s => s.Result == TestResults.OK);
                    var score = teamProblemSolutions.OrderByDescending(s => s.Score).First().Score;
                    var acceptTime = teamProblemSolutions.First(s => s.Score == score).SendTime;
                    var penalties = teamProblemSolutions.Count();

                    var teamProblemResult = tempTeamProblemsResult.First(pr => pr.ProblemID == teamProblemSolutions.Key);
                    if (teamProblemResult != null)
                    {
                        teamProblemResult.Accept = accept;
                        teamProblemResult.Score = score;
                        teamProblemResult.AcceptTime = acceptTime - tournament.StartDate;
                        teamProblemResult.Penalties = penalties;
                    }
                }

                int totalScore = 0;
                tempTeamProblemsResult.Each(r => totalScore += r.Score);

                tempTeamsTournamentResults.Add(new ParticipantTournamentResult()
                {
                    ID = teamSolutions.First().ID,
                    Online = teamSolutions.First().Online,
                    Name = teamSolutions.First().Name,
                    FullName = teamSolutions.First().FullName,
                    Users = teamSolutions.First().Users,
                    ProblemsResults = tempTeamProblemsResult,
                    TotalScore = totalScore
                }
                );
            }
            #endregion

            // Add users who have not sent solutions
            tempUserProblemsResult = new List<ParticipantProblemResult>();
            tournament.Problems.Each(p =>
                tempUserProblemsResult.Add(new ParticipantProblemResult() { ProblemID = p.ProblemID })
            );
            tournament.Users.Each(u =>
            {
                if (!Roles.IsUserInRole(u.UserName, "Judge") &&
                    !Roles.IsUserInRole(u.UserName, "Administrator"))
                {
                    if (!tempUsersTournamentResults.Contains(tr => tr.ID == u.UserId))
                    {
                        tempUsersTournamentResults.Add(new ParticipantTournamentResult()
                        {
                            ID = u.UserId,
                            Online = repository.IsUserOnline(u),
                            Name = u.UserName,
                            FullName = (u.SecondName + " " + u.FirstName + " " + u.ThirdName).Trim(),
                            Users = new List<UserProfile>() { u },
                            ProblemsResults = tempUserProblemsResult,
                            TotalScore = 0
                        });
                    }
                }
            });

            // Add teams who have not sent solutions
            tempTeamProblemsResult = new List<ParticipantProblemResult>();
            tournament.Problems.Each(p =>
                tempTeamProblemsResult.Add(new ParticipantProblemResult() { ProblemID = p.ProblemID })
            );
            tournament.Teams.GroupBy(ut => ut.TeamID).Each(t =>
            {
                if (!tempTeamsTournamentResults.Contains(tr => tr.ID == t.First().TeamID))
                {
                    string fullName = "";
                    List<UserProfile> users = new List<UserProfile>();
                    t.Each(ut =>
                    {
                        if ((ut.User.FirstName == null || ut.User.FirstName == "") &&
                            (ut.User.SecondName == null || ut.User.SecondName == "") &&
                            (ut.User.ThirdName == null || ut.User.ThirdName == ""))
                            fullName += ut.User.UserName + ",\n";
                        else
                            fullName += (ut.User.SecondName + " " + ut.User.FirstName + " " + ut.User.ThirdName).Trim() + ",\n";

                        users.Add(ut.User);
                    });
                    fullName = fullName.TrimEnd('\n').TrimEnd(',');

                    bool online = false;
                    users.Each(u => online = online || repository.IsUserOnline(u));

                    tempTeamsTournamentResults.Add(new ParticipantTournamentResult()
                    {
                        ID = t.First().TeamID,
                        Online = online,
                        Name = t.First().Team.Name,
                        FullName = fullName.Replace("\n", "<br />"),
                        Users = users,
                        ProblemsResults = tempTeamProblemsResult,
                        TotalScore = 0
                    });
                }
            });

            List<ParticipantTournamentResult> tempTournamentResults = new List<ParticipantTournamentResult>();
            tempTournamentResults.AddRange(tempUsersTournamentResults);
            tempTournamentResults.AddRange(tempTeamsTournamentResults);

            GroupResults = tempTournamentResults.OrderByDescending(r => r.TotalScore);

            int i;
            if (GroupResults.Count() > 0)
            {
                i = 1;
                int score = GroupResults.First().TotalScore;
                foreach (var result in GroupResults)
                {
                    if (score != result.TotalScore)
                        i++;

                    score = result.TotalScore;
                    result.Place = i.ToString();
                }

            }
        }




        #region Helpers
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
        #endregion
    }
}
