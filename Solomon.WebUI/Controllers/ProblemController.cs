using Solomon.Domain.Abstract;
using Solomon.Domain.Entities;
using Solomon.WebUI.Helpers;
using Solomon.WebUI.Infrastructure;
using Solomon.WebUI.Models;
using Solomon.WebUI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml;
using Solomon.TypesExtensions;
using Solomon.WebUI.Testers;
using log4net;
using WebMatrix.WebData;
using log4net.Config;
using System.Web.Routing;

namespace Solomon.WebUI.Controllers
{
    public class ProblemController : Controller
    {
        private IRepository repository;
        private char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private readonly ILog logger = LogManager.GetLogger(typeof(ProblemController));

        /// <summary>
        /// Controller constructor.
        /// Initialize repository.
        /// </summary>
        /// <param name="Repository">Repository object</param>
        public ProblemController(IRepository repository)
        {
            XmlConfigurator.Configure();
            this.repository = repository;
        }

        private string ReplaceImagesWithHTMLTags(string Text, int ProblemID)
        {
            List<Tuple<string, string>> imgs = new List<Tuple<string, string>>();
            int startIndex = 0;
            int count = 0;
            string imgPath;
            while (startIndex < Text.Length && startIndex != -1)
            {
                try
                {
                    startIndex = Text.IndexOf("[attach=", startIndex);
                    count = Text.IndexOf("]", startIndex) - startIndex;
                    imgPath = Text.Substring(startIndex + "[attach=".Length, count - "[attach=".Length);

                    if (!System.IO.File.Exists(Server.MapPath(imgPath)))
                    {
                        if (System.IO.File.Exists(Path.Combine(
                            LocalPath.AbsoluteProblemsDirectory, ProblemID.ToString(),
                            LocalPath.RelativeProblemsAttachDirectory, Path.GetFileName(imgPath))))
                        {
                            System.IO.File.Copy(Path.Combine(
                                LocalPath.AbsoluteProblemsDirectory, ProblemID.ToString(),
                                LocalPath.RelativeProblemsAttachDirectory, Path.GetFileName(imgPath)),
                                Server.MapPath(imgPath));
                        }
                    }

                    if (System.IO.File.Exists(Server.MapPath(imgPath)))
                        imgs.Add(new Tuple<string, string>(
                            Text.Substring(startIndex, count + 1), imgPath));

                    startIndex += count;
                }
                catch (Exception) { }
            }

            foreach (var item in imgs)
            {
                Text = Text.Replace(item.Item1, "<img src=\"" + item.Item2 + "\" style=\"max-width:760px;\">");
            }

            return Text;
        }

        /// <summary>
        /// Partial view for navigation bar.
        /// </summary>
        /// <param name="TournamentID">ID of tournament</param>
        /// <returns>ProblemsListViewModel</returns>
        public PartialViewResult ProblemsList(int tournamentID)
        {
            Tournament tournament = repository.Tournaments.FirstOrDefault(t => t.TournamentID == tournamentID);
            ProblemsListViewModel viewModel = new ProblemsListViewModel() { TournamentID = -1 };

            if (tournament == null)
            {
                logger.Warn("Tournament with id = " + tournamentID + " not found");
                throw new HttpException(404, "Tournament with id = " + tournamentID + " not found");
            }

            viewModel.Problems = tournament
                    .Problems
                    .OrderBy(p => p.ProblemID);
            viewModel.TournamentID = tournamentID;

            // Add numbering prefix to problem name.
            int i = 1;
            foreach (var problem in viewModel.Problems)
            {
                problem.Name = i.ToAlpha() + ". " + problem.Name;
                i++;
            }

            return PartialView(viewModel);
        }

        /// <summary>
        /// Partial view for problem content.
        /// </summary>
        /// <param name="ProblemID">ID of problem</param>
        public PartialViewResult ProblemContent(int ProblemID)
        {
            Problem problem = repository.Problems.FirstOrDefault(p => p.ProblemID == ProblemID);

            if (problem == null && ProblemID != -1)
            {
                logger.Warn("Problem with id = " + ProblemID + " not found");
                throw new HttpException(404, "Problem with id = " + ProblemID + " not found");
            }

            if (problem == null)
                return PartialView(new ProblemContentViewModel() { ProblemID = -1 });

            // Read problem info
            string name, description, inputFormat, outputFormat;
            double timeLimit;
            int memoryLimit;
            ProblemTypes pt;
            try
            {
                ProblemLegend.Read(LocalPath.AbsoluteProblemsDirectory + problem.ProblemID,
                    out name,
                    out timeLimit,
                    out memoryLimit,
                    out pt,
                    out description,
                    out inputFormat,
                    out outputFormat);

                if (pt != problem.Type)
                    throw new ArgumentException("Problem types in database and in legend file are different");
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred on problem legend reading:", ex);
                return PartialView(new ProblemContentViewModel() { ProblemID = -1 });
            }

            List<Tuple<string, string, string>> samples = null;
            bool comments = false;
            if (problem.Type != ProblemTypes.Open)
            {
                #region Read problem samples
                samples = new List<Tuple<string, string, string>>();
                string[] files = null;
                try
                {
                    files = System.IO.Directory
                        .GetFiles(LocalPath.AbsoluteProblemsDirectory + problem.ProblemID + "/Samples", "*")
                        .Where(f => Path.GetExtension(f) == ".in")
                        .ToArray();

                    if (files.Length == 0)
                    {
                    }
                    else
                    {
                        string input = "", output = "", comment = "";
                        foreach (string file in files)
                        {
                            if (System.IO.File.Exists(LocalPath.AbsoluteProblemsDirectory + problem.ProblemID + "/Samples/" +
                                Path.GetFileNameWithoutExtension(file) + ".cmt"))
                            {
                                comments = true;
                            }

                            using (StreamReader srIn = new StreamReader(file))
                            {
                                using (StreamReader srOut = new StreamReader(
                                    LocalPath.AbsoluteProblemsDirectory + problem.ProblemID + "/Samples/" +
                                    Path.GetFileNameWithoutExtension(file) + ".out"))
                                {
                                    input = HttpUtility.HtmlEncode(srIn.ReadToEnd()).Replace("\n", "<br />");
                                    output = HttpUtility.HtmlEncode(srOut.ReadToEnd()).Replace("\n", "<br />");

                                    if (comments)
                                    {
                                        try
                                        {
                                            using (StreamReader srCmt = new StreamReader(
                                                LocalPath.AbsoluteProblemsDirectory + problem.ProblemID + "/Samples/" +
                                                Path.GetFileNameWithoutExtension(file) + ".cmt"))
                                            {
                                                comment = HttpUtility.HtmlEncode(srCmt.ReadToEnd()).Replace("\n", "<br />");
                                            }
                                        }
                                        catch (FileNotFoundException)
                                        {
                                            comment = "";
                                        }
                                    }
                                }
                            }

                            samples.Add(new Tuple<string, string, string>(input, output, comment));
                        }
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error occurred on problem samples reading:", ex);
                    return PartialView(new ProblemContentViewModel() { ProblemID = -1 });
                }
                #endregion
            }

            ProblemContentViewModel viewModel = new ProblemContentViewModel
            {
                ProblemID = problem.ProblemID,
                PT = problem.Type,
                
                Name = name,
                TimeLimit = timeLimit,
                MemoryLimit = memoryLimit,
                Description = ReplaceImagesWithHTMLTags(HttpUtility.HtmlEncode(description), problem.ProblemID).Replace("\n", "<br />"),
                InputFormat = ReplaceImagesWithHTMLTags(HttpUtility.HtmlEncode(inputFormat), problem.ProblemID).Replace("\n", "<br />"),
                OutputFormat = ReplaceImagesWithHTMLTags(HttpUtility.HtmlEncode(outputFormat), problem.ProblemID).Replace("\n", "<br />"),
                TestSamplesComments = comments,
                TestSamples = samples
            };

            return PartialView(viewModel);
        }

        /// <summary>
        /// GET: /Problem?TournamentID=1&ProblemID=1
        /// 
        /// Show problem at tournament.
        /// </summary>
        /// <param name="TournamentID">ID of tournament</param>
        /// <param name="ProblemID">ID of problem</param>
        /// <returns>ProblemViewModel</returns>
        public ViewResult Problem(int TournamentID, int ProblemID = -1, int Page = 1, int PageSize = 25)
        {
            Tournament tournament = repository
                .Tournaments
                .FirstOrDefault(t => t.TournamentID == TournamentID);

            if (tournament == null)
            {
                logger.Warn("Tournament with id = " + TournamentID + " not found");
                throw new HttpException(404, "Tournament with id = " + TournamentID + " not found");
            }

            UserProfile user = repository.Users.FirstOrDefault(u => u.UserId == WebSecurity.CurrentUserId);

            if ((tournament.Type == TournamentTypes.Close && 
                !Roles.IsUserInRole("Administrator") &&
                !tournament.Users.Contains(u => u.UserId == WebSecurity.GetUserId(User.Identity.Name))) ||
                (user != null && user.Generated == 1 && !user.Tournaments.Contains(tournament)))
            {
                logger.Warn("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                    " (" + User.Identity.Name + ") try get access to close tournament with id = " + TournamentID);
                throw new HttpException(401, "Unauthorized Explained to close tournament " + TournamentID);
            }

            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited Tournament " + TournamentID + " Problem " + ProblemID);

            // Select problems bound with tournament.
            IQueryable<Problem> problems = repository
                .Problems
                .Where(p =>
                    p.Tournaments.FirstOrDefault(t => t.TournamentID == TournamentID) != null
                );
            
            Problem problem;
            if (ProblemID == -1)
            {
                problem = problems.FirstOrDefault();
                if (problem != null )
                    ProblemID = problem.ProblemID;
            }
            else
            {
                problem = problems.FirstOrDefault(p => p.ProblemID == ProblemID);
            }

            if (problem == null && ProblemID != -1)
            {
                logger.Warn("Problem with id = " + ProblemID + " not found");
                throw new HttpException(404, "Problem with id = " + ProblemID + " not found");
            }

            if (problem == null)
                return View(new ProblemViewModel() { ProblemID = -1 });

            int userID = WebSecurity.CurrentUserId;
            PaginatedList<Solution> solutions;
            if ((user != null && (user.CanModifyProblems.Contains(problem) || user.CanModifyTournaments.Contains(tournament))) ||
                Roles.IsUserInRole("Administrator"))
            {
                solutions = repository
                    .Solutions
                    .Where(s =>
                        (s.Tournament.TournamentID == TournamentID) &&
                        (s.Problem.ProblemID == ProblemID)
                    )
                    .OrderByDescending(s => s.SendTime)
                    .ToPaginatedList<Solution>(Page, PageSize);
            }
            else
            {
                solutions = repository
                    .Solutions
                    .Where(s =>
                        (s.User.UserId == userID) &&
                        (s.Tournament.TournamentID == TournamentID) &&
                        (s.Problem.ProblemID == ProblemID)
                    )
                    .OrderByDescending(s => s.SendTime)
                    .ToPaginatedList<Solution>(Page, PageSize);
            }

            bool isUserRegisterForTournament = false;
            if (tournament.Users.FirstOrDefault(u => u.UserId == userID) != null ||
                tournament.Teams.FirstOrDefault(ut => ut.UserID == userID) != null)
            {
                isUserRegisterForTournament = true;
            }

            ProblemViewModel ViewModel = new ProblemViewModel
            {
                IsUserRegisterForTournament = isUserRegisterForTournament,

                PT = problem.Type,
                ProblemID = problem.ProblemID,
                TournamentID = TournamentID,
                TournamentName = tournament.Name,
                TF = tournament.Format,
                TournamentStartDate = tournament.StartDate,
                TournamentEndDate = tournament.EndDate,
                CurrentTime = DateTime.Now,
                ShowTimer = tournament.ShowTimer,

                HasNextPage = solutions.HasNextPage,
                HasPreviousPage = solutions.HasPreviousPage,
                PageIndex = Page,
                PageSize = PageSize,
                TotalPages = solutions.TotalPages,
                TotalCount = solutions.TotalCount
            };

            return View(ViewModel);
        }

        /// <summary>
        /// Partial view for solutions table
        /// </summary>
        /// <param name="TournamentID">ID of tournament</param>
        /// <param name="ProblemID">ID of problem</param>
        /// <returns></returns>
        public PartialViewResult GetSolutionsData(int TournamentID, int ProblemID = -1, int Page = 1, int PageSize = 25)
        {
            int userID = WebSecurity.CurrentUserId;

            // Select problems bound with tournament.
            IEnumerable<Problem> problems = repository
                .Problems
                .Where(p =>
                    p.Tournaments.FirstOrDefault(t => t.TournamentID == TournamentID) != null
                );

            if (ProblemID == -1)
            {
                ProblemID = problems.First().ProblemID;
            }

            Tournament tournament = repository
                .Tournaments
                .FirstOrDefault(t => t.TournamentID == TournamentID);
            Problem problem = repository
                .Problems
                .FirstOrDefault(p => p.ProblemID == ProblemID);

            UserProfile user = repository
                .Users
                .FirstOrDefault(u => u.UserId == userID);
            PaginatedList<Solution> solutions;
            if ((user != null && (user.CanModifyProblems.Contains(problem) || user.CanModifyTournaments.Contains(tournament))) ||
                Roles.IsUserInRole("Administrator"))
            {
                solutions = repository
                    .Solutions
                    .Where(s =>
                        (s.Tournament.TournamentID == TournamentID) &&
                        (s.Problem.ProblemID == ProblemID)
                    )
                    .OrderByDescending(s => s.SendTime)
                    .ToPaginatedList<Solution>(Page, PageSize);
            }
            else
            {
                solutions = repository
                    .Solutions
                    .Where(s =>
                        (s.User.UserId == userID) &&
                        (s.Tournament.TournamentID == TournamentID) &&
                        (s.Problem.ProblemID == ProblemID)
                    )
                    .OrderByDescending(s => s.SendTime)
                    .ToPaginatedList<Solution>(Page, PageSize);
            }

            if (problem == null)
            {
                logger.Warn("Problem with id = " + ProblemID + " not found");
                throw new HttpException(404, "Problem with id = " + ProblemID + " not found");
            }
            if (tournament == null)
            {
                logger.Warn("Tournament with id = " + TournamentID + " not found");
                throw new HttpException(404, "Tournament with id = " + TournamentID + " not found");
            }

            ViewData["Solutions"] = solutions;
            ViewData["TF"] = tournament.Format;
            ViewData["PT"] = problem.Type;
            repository.ProgrammingLanguages.Each(pl => ViewData[pl.ProgrammingLanguageID.ToString()] = pl.Title);

            return PartialView();
        }

        /// <summary>
        /// Partial view for solutions table
        /// </summary>
        /// <param name="TournamentID">ID of tournament</param>
        /// <param name="ProblemID">ID of problem</param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetSolutionsData_ajax(int TournamentID, int ProblemID = -1, int Page = 1, int PageSize = 25)
        {
            int userID = WebSecurity.CurrentUserId;

            // Select problems bound with tournament.
            IEnumerable<Problem> problems = repository
                .Problems
                .Where(p =>
                    p.Tournaments.FirstOrDefault(t => t.TournamentID == TournamentID) != null
                );

            if (ProblemID == -1)
            {
                ProblemID = problems.First().ProblemID;
            }

            Tournament tournament = repository
                .Tournaments
                .FirstOrDefault(t => t.TournamentID == TournamentID);
            Problem problem = repository
                .Problems
                .FirstOrDefault(p => p.ProblemID == ProblemID);

            UserProfile user = repository
                .Users
                .FirstOrDefault(u => u.UserId == userID);
            PaginatedList<Solution> solutions;
            if ((user != null && (user.CanModifyProblems.Contains(problem) || user.CanModifyTournaments.Contains(tournament))) ||
                Roles.IsUserInRole("Administrator"))
            {
                solutions = repository
                    .Solutions
                    .Where(s =>
                        (s.Tournament.TournamentID == TournamentID) &&
                        (s.Problem.ProblemID == ProblemID)
                    )
                    .OrderByDescending(s => s.SendTime)
                    .ToPaginatedList<Solution>(Page, PageSize);
            }
            else
            {
                solutions = repository
                    .Solutions
                    .Where(s =>
                        (s.User.UserId == userID) &&
                        (s.Tournament.TournamentID == TournamentID) &&
                        (s.Problem.ProblemID == ProblemID)
                    )
                    .OrderByDescending(s => s.SendTime)
                    .ToPaginatedList<Solution>(Page, PageSize);
            }
            
            var response = new JsonResponseSolutionsData();

            if (problem == null)
            {
                logger.Warn("Problem with id = " + ProblemID + " not found");
                throw new HttpException(404, "Problem with id = " + ProblemID + " not found");
            }
            if (tournament == null)
            {
                logger.Warn("Tournament with id = " + TournamentID + " not found");
                throw new HttpException(404, "Tournament with id = " + TournamentID + " not found");
            }

            // Get text view.
            using (var sw = new StringWriter())
            {
                ViewData["Solutions"] = solutions;
                ViewData["TF"] = tournament.Format;
                ViewData["PT"] = problem.Type;
                repository.ProgrammingLanguages.Each(pl => ViewData[pl.ProgrammingLanguageID.ToString()] = pl.Title);
                var viewResult = ViewEngines.Engines.FindPartialView(this.ControllerContext, "GetSolutionsData");
                var viewContext = new ViewContext(this.ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(this.ControllerContext, viewResult.View);
                response.HtmlTable = sw.GetStringBuilder().ToString();
            }

            response.Reload = solutions
                .FirstOrDefault(s =>
                    s.Result == TestResults.Waiting ||
                    s.Result == TestResults.Executing ||
                    s.Result == TestResults.Compiling) != null;

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Partial view for solution results table
        /// </summary>
        /// <param name="SolutionID">ID of solution</param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetSolutionResults(int SolutionID = -1)
        {
            int userID = WebSecurity.CurrentUserId;

            Solution solution = repository
                .Solutions
                .FirstOrDefault(s => s.SolutionID == SolutionID);

            if (solution == null)
            {
                logger.Warn("Solution with id = " + SolutionID + " not found");
                throw new HttpException(404, "Solution with id = " + SolutionID + " not found");
            }

            if (solution.UserID != userID && 
                !Roles.IsUserInRole("Judge") &&
                !Roles.IsUserInRole("Administrator"))
            {
                logger.Warn("Problem.GetSolutionResults: Unauthorized explained - User " + WebSecurity.GetUserId(User.Identity.Name) +
                        " \"" + User.Identity.Name + "\" try to get solution results for solution " + SolutionID);
                throw new HttpException(401, "Problem.GetSolutionResults: Unauthorized explained - User " + WebSecurity.GetUserId(User.Identity.Name) +
                        " \"" + User.Identity.Name + "\" try to get solution results for solution " + SolutionID);
            }

            List<SolutionTestResult> solutionTestResults;
            solutionTestResults = repository.SolutionTestResults
                .Where(str => str.SolutionID == SolutionID)
                .OrderBy(str => str.SolutionTestResultID)
                .ToList();

            var response = new JsonResponseSolutionResults();
            
            // Get text view.
            using (var sw = new StringWriter())
            {
                ViewData["SolutionTestResults"] = solutionTestResults;
                ViewData["HasResults"] = solutionTestResults.Count > 0;
                var viewResult = ViewEngines.Engines.FindPartialView(this.ControllerContext, "GetSolutionResults");
                var viewContext = new ViewContext(this.ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(this.ControllerContext, viewResult.View);
                response.Html = sw.GetStringBuilder().ToString();
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Return solution file.
        /// </summary>
        [HttpGet]
        [Authorize]
        public FilePathResult GetSolutionFile(int SolutionID)
        {
            Solution solution = repository
                .Solutions
                .FirstOrDefault(s => s.SolutionID == SolutionID);

            if (solution == null)
            {
                logger.Warn("Solution with id = " + SolutionID + " not found");
                throw new HttpException(404, "Solution with id = " + SolutionID + " not found");
            }

            if (!Roles.IsUserInRole("Judge") &&
                !Roles.IsUserInRole("Administrator") &&
                WebSecurity.CurrentUserId != solution.UserID)
            {
                logger.Warn("Unauthorized explained: User " + WebSecurity.CurrentUserId + " try get access to solution " + SolutionID);
                throw new HttpException(401, "Unauthorized explained");
            }

            logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) +
                " \"" + User.Identity.Name + "\" download solution " + SolutionID);

            Response.AppendHeader("Content-Disposition", "attachment; filename=" + solution.FileName);

            return new FilePathResult(Path.Combine(LocalPath.RootDirectory, solution.Path), solution.DataType);
        }

        [HttpGet]
        [Authorize(Roles = "Judge, Administrator")]
        public JsonResult ChangeSolutionStatus(int SolutionID, TestResults Result)
        {
            Solution solution = repository.Solutions.FirstOrDefault(s => s.SolutionID == SolutionID);

            if (solution == null)
            {
                logger.Warn("Solution with id = " + SolutionID + " not found");
                throw new HttpException(404, "Solution with id = " + SolutionID + " not found");
            }

            JsonResponse response = new JsonResponse();
            response.Success = true;

            if (Result == TestResults.Waiting)
            {
                TestersSingleton.Instance.RecheckSolution(solution.SolutionID);
            }
            else if (Result == TestResults.Disqualified)
            {
                solution.Result = Result;
                if (!solution.User.SolvedProblems.Contains(solution.Problem))
                {
                    solution.User.NotSolvedProblems.Add(solution.Problem);
                }
                repository.SaveSolution(solution);
            }
            else if (Result == TestResults.OK)
            {
                try
                {
                    logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) +
                        " \"" + User.Identity.Name + "\" change status " + solution.Result + " -> " + Result + 
                        " for solution " + SolutionID);

                    solution.Result = Result;
                    if (solution.Tournament.Format == TournamentFormats.IOI && Result == TestResults.OK)
                    {
                        solution.Score = 100;
                    }

                    if (solution.Result == TestResults.OK)
                    {
                        solution.User.NotSolvedProblems.Remove(solution.Problem);
                        solution.User.SolvedProblems.Add(solution.Problem);
                    }
                    else
                    {
                        if (!solution.User.SolvedProblems.Contains(solution.Problem))
                        {
                            solution.User.NotSolvedProblems.Add(solution.Problem);
                        }
                    }

                    repository.SaveSolution(solution);
                }
                catch (Exception ex)
                {
                    logger.Warn("Error occurred on solution status changing: Solution with id = " + SolutionID, ex);

                    response.Success = false;
                    response.Message = ex.Message;
                }
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Partial view for solutions adding.
        /// </summary>
        /// <returns>ProblemsListViewModel</returns>
        public PartialViewResult AddSolution(int TournamentID, int ProblemID)
        {
            Problem problem = repository.Problems.FirstOrDefault(p => p.ProblemID == ProblemID);

            AddSolutionViewModel viewModel = new AddSolutionViewModel()
                {
                    PT = problem.Type,
                    ProblemID = ProblemID,
                    TournamentID = TournamentID,
                    ProgrammingLanguageID = problem.Type == ProblemTypes.Open ? (int)ProgrammingLanguages.Open : 0
                };

            return PartialView(viewModel);
        }

        /// <summary>
        /// POST: /
        /// 
        /// Send solution.
        /// 
        /// Solution file saves as byte array in db and in a local;
        /// deleted from db after checking by test system.
        /// </summary>
        /// <param name="ProblemTournamentID">ID for identify the problem and the tournament</param>
        /// <param name="SolutionFile">Solution</param>
        /// <returns>Redirect to problem page</returns>
        [HttpPost]
        public ActionResult AddSolution(AddSolutionViewModel Model)
        {
            if (ModelState.IsValid)
            {
                if (Model.SolutionFile.ContentLength > 262144)
                {
                    TempData["ErrorMessage"] = "Максимальный размер файла - 256 кб";
                    return RedirectToAction("Problem",
                        new {
                            tournamentID = Model.TournamentID,
                            problemID = Model.ProblemID
                        });
                }

                Tournament tournament = repository.Tournaments.FirstOrDefault(t => t.TournamentID == Model.TournamentID);
                Problem problem = repository.Problems.FirstOrDefault(p => p.ProblemID == Model.ProblemID);

                if (tournament != null && problem != null && 
                    (Roles.IsUserInRole("Judge") || Roles.IsUserInRole("Administrator") ||
                    (tournament.EndDate >= DateTime.Now && tournament.StartDate <= DateTime.Now)))
                {
                    for (int i = 0; i < 1; i++)
                    {
                        int userID = WebSecurity.GetUserId(User.Identity.Name);
                        Solution solution = new Solution
                            {
                                UserID = userID,
                                TournamentID = Model.TournamentID,
                                ProblemID = Model.ProblemID,
                                FileName = Path.GetFileName(Model.SolutionFile.FileName),
                                DataType = Model.SolutionFile.ContentType,
                                ProgrammingLanguage = (ProgrammingLanguages)Model.ProgrammingLanguageID,
                                SendTime = DateTime.Now,
                                Result = TestResults.Waiting
                            };

                        repository.SaveSolution(solution);

                        string relativePath = Path.Combine(
                            LocalPath.RelativeSolutionsDirectory,
                            userID.ToString(),
                            solution.ProblemID.ToString());
                        string absolutePath = Path.Combine(
                            LocalPath.AbsoluteSolutionsDirectory,
                            userID.ToString(),
                            solution.ProblemID.ToString());

                        if (!Directory.Exists(absolutePath))
                            Directory.CreateDirectory(absolutePath);

                        string fileName = solution.SolutionID.ToString();
                        absolutePath = Path.Combine(absolutePath, fileName);
                        relativePath = Path.Combine(relativePath, fileName);
                        Model.SolutionFile.SaveAs(absolutePath);

                        solution.Path = relativePath;
                        if (problem.CheckPending == true)
                        {
                            solution.Result = TestResults.CHKP;
                        }
                        repository.SaveSolution(solution);

                        if (problem.CheckPending == false)
                        {
                            TestersSingleton.Instance.AddSolutionForChecking(
                                Model.ProblemID, solution.SolutionID, solution.ProgrammingLanguage,
                                tournament.Format, problem.Type, relativePath, solution.FileName);
                        }

                        logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                            " \"" + User.Identity.Name + "\" add solution: PL = " + solution.ProgrammingLanguage +
                            ", TournamentID = " + Model.TournamentID +
                            ", ProblemID = " + Model.ProblemID + ", SolutionID = " + solution.SolutionID);
                    }
                }
            }
            return RedirectToAction("Problem",
                new
                {
                    tournamentID = Model.TournamentID,
                    problemID = Model.ProblemID
                });
        }

        private IEnumerable<Comment> _getComments(int ProblemID, out bool showAll, int TournamentID = -1)
        {
            IEnumerable<Comment> comments;
            if (TournamentID != -1)
            {
                comments = repository
                    .Comments
                    .Where(c => c.TournamentID == TournamentID && c.ProblemID == ProblemID && (c.ParentCommentID == null || c.ParentCommentID == 0))
                    .ToList();
            }
            else
            {
                comments = repository
                    .Comments
                    .Where(c => c.ProblemID == ProblemID && (c.ParentCommentID == null || c.ParentCommentID == 0))
                    .ToList();
            }

            Problem problem = repository.Problems.FirstOrDefault(p => p.ProblemID == ProblemID);
            Tournament tournament = repository.Tournaments.FirstOrDefault(t => t.TournamentID == TournamentID);

            // Show all comments for authors of problem or tournament
            showAll =
                (problem != null && problem.UsersCanModify.FirstOrDefault(u => u.UserId == WebSecurity.CurrentUserId) != null) ||
                (tournament != null && tournament.UsersCanModify.FirstOrDefault(u => u.UserId == WebSecurity.CurrentUserId) != null);

            // Show public and own comments for other users
            if (!Roles.IsUserInRole("Administrator") && !showAll)
            {
                comments = comments.Where(c => c.UserID == WebSecurity.CurrentUserId || c.Public == 1);
            }

            return comments;
        }

        public ActionResult Comments(int ProblemID, int TournamentID = -1)
        {
            int userID = WebSecurity.CurrentUserId;
            bool showAll;
            IEnumerable<Comment> comments = _getComments(ProblemID, out showAll, TournamentID);

            CommentsViewModel viewModel = new CommentsViewModel()
            {
                ProblemID = ProblemID,
                TournamentID = TournamentID,
                ShowAll = showAll
            };

            viewModel.Comments = new List<CommentViewModel>();
            Stack<Comment> stack = new Stack<Comment>(comments.OrderByDescending(c => c.Date));

            Comment temp;
            bool showAllBranch = false;
            while (stack.Count > 0)
            {
                temp = stack.Pop();
                viewModel.Comments.Add(new CommentViewModel(temp, temp.IsRead == 0));

                // Mark as read
                if (temp.IsRead != 1 && temp.UserID != userID)
                {
                    //if (temp.ParentComment == null)
                    //{
                    //    if (!Roles.IsUserInRole("Administrator") || viewModel.ShowAll)
                    //        temp.IsRead = 1;
                    //}
                    //else
                    //{
                    //    temp.IsRead = temp.ParentComment.UserID == userID ? 1 : 0;
                    //}
                    if (temp.ParentComment != null)
                        temp.IsRead = temp.ParentComment.UserID == userID ? 1 : 0;
                }
                repository.AddComment(temp);

                showAllBranch = temp.Level == 0 ? temp.UserID == WebSecurity.CurrentUserId : showAllBranch;
                if (Roles.IsUserInRole("Administrator") || viewModel.ShowAll || temp.UserID == userID || showAllBranch)
                {
                    temp.ChildComments.OrderByDescending(c => c.Date).Each(c => stack.Push(c));
                }
                else if (temp.Public == 1)
                {
                    temp.ChildComments.Where(c => c.Public == 1 || c.UserID == WebSecurity.CurrentUserId).OrderByDescending(c => c.Date).Each(c => stack.Push(c));
                }
            }

            return View(viewModel);
        }

        public PartialViewResult AddComment(int ProblemID, int TournamentID = -1)
        {
            AddCommentViewModel viewModel = new AddCommentViewModel()
            {
                ProblemID = ProblemID,
                TournamentID = TournamentID
            };

            return PartialView(viewModel);
        }

        [HttpPost]
        public ActionResult AddComment(AddCommentViewModel Model)
        {
            Comment parent = repository.Comments.FirstOrDefault(c => c.CommentID == Model.ParentCommentID);

            Comment comment = new Comment()
            {
                Date = DateTime.Now,
                IsRead = 0,
                Level = parent != null ? parent.Level + 1 : 0,
                ParentComment = parent,
                ProblemID = Model.ProblemID,
                Public = 0,
                TournamentID = Model.TournamentID,
                UserID = WebSecurity.CurrentUserId,
                Value = Model.Value.Replace("\n", "<br />")
            };

            int commentID = repository.AddComment(comment);

            if (parent != null && parent.IsRead == 0)
            {
                parent.IsRead = 1;
                repository.AddComment(parent);
            }

            string url = UrlHelper.GenerateUrl(
                null, "Comments", "Problem", null, null, "Comment-" + commentID,
                new RouteValueDictionary(new { ProblemID = Model.ProblemID, TournamentID = Model.TournamentID }),
                Url.RouteCollection, Url.RequestContext, false);

            return Redirect(url);//RedirectToAction("Comments", new { ProblemID = Model.ProblemID, TournamentID = Model.TournamentID });
        }

        [HttpGet]
        [Authorize(Roles = "Judge, Administrator")]
        public JsonResult MakeCommentPublic(int CommentID)
        {
            Comment comment = repository.Comments.FirstOrDefault(c => c.CommentID == CommentID);

            if (comment == null)
            {
                logger.Warn("Comment with id = " + CommentID + " not found");
                throw new HttpException(404, "Comment with id = " + CommentID + " not found");
            }

            JsonResponse response = new JsonResponse();
            response.Success = true;

            try
            {

                logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) +
                    " \"" + User.Identity.Name + "\" make comment " + CommentID + " public");

                while (comment != null)
                {
                    comment.Public = 1;
                    repository.AddComment(comment);
                    comment = comment.ParentComment;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Error occurred on making comment public: Comment with id = " + CommentID, ex);

                response.Success = false;
                response.Message = ex.Message;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        [Authorize(Roles = "Judge, Administrator")]
        public JsonResult MakeCommentPrivate(int CommentID)
        {
            Comment comment = repository.Comments.FirstOrDefault(c => c.CommentID == CommentID);

            if (comment == null)
            {
                logger.Warn("Comment with id = " + CommentID + " not found");
                throw new HttpException(404, "Comment with id = " + CommentID + " not found");
            }

            JsonResponse response = new JsonResponse();
            response.Success = true;

            try
            {

                logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) +
                    " \"" + User.Identity.Name + "\" make comment " + CommentID + " private");

                Stack<Comment> stack = new Stack<Comment>();
                stack.Push(comment);

                while (stack.Count > 0)
                {
                    comment = stack.Pop();
                    comment.ChildComments.Each(c => stack.Push(c));
                    comment.Public = 0;
                    repository.AddComment(comment);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Error occurred on making comment private: Comment with id = " + CommentID, ex);

                response.Success = false;
                response.Message = ex.Message;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize(Roles = "Judge, Administrator")]
        public JsonResult MarkCommentAsRead(int CommentID)
        {
            Comment comment = repository.Comments.FirstOrDefault(c => c.CommentID == CommentID);

            if (comment == null)
            {
                logger.Warn("Comment with id = " + CommentID + " not found");
                throw new HttpException(404, "Comment with id = " + CommentID + " not found");
            }

            JsonResponse response = new JsonResponse();
            response.Success = true;

            try
            {

                logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) +
                    " \"" + User.Identity.Name + "\" mark comment " + CommentID + " as read");

                comment.IsRead = 1;
                repository.AddComment(comment);
            }
            catch (Exception ex)
            {
                logger.Warn("Error occurred on marking comment as read: Comment with id = " + CommentID, ex);

                response.Success = false;
                response.Message = ex.Message;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public JsonResult GetNewCommentsCount(int ProblemID, int TournamentID = -1)
        {
            Problem problem = repository.Problems.FirstOrDefault(p => p.ProblemID == ProblemID);
            Tournament tournament = repository.Tournaments.FirstOrDefault(t => t.TournamentID == TournamentID);

            NewCommentsJsonResponse response = new NewCommentsJsonResponse();
            response.Success = true;

            int userID = WebSecurity.CurrentUserId;

            UserProfile user = repository
                .Users
                .FirstOrDefault(u => u.UserId == userID);

            IEnumerable<Comment> comments = repository.Comments.Where(c => c.IsRead == 0 && c.UserID != userID).AsEnumerable();

            if (userID != 1)
            {
                if (Roles.IsUserInRole("Administrator") || Roles.IsUserInRole("Judge"))
                {
                    comments = comments
                        .Where(c => user.CanModifyProblems.Contains(c.Problem) || user.CanModifyTournaments.Contains(c.Tournament))
                        .Where(c => c.ParentComment != null ? c.ParentComment.UserID == userID : true);

                }
                else
                {
                    comments = comments
                        .Where(c => c.ParentComment != null ? c.ParentComment.UserID == userID : false);
                }
            }

            response.TotalCount = comments.Count();
            
            List<ProblemCommentsCount> problemComments = new List<ProblemCommentsCount>();
            
            comments
                .Where(c => TournamentID != -1 ? c.TournamentID == TournamentID : c.Tournament == null)
                .GroupBy(c => c.Problem)
                .Each(pc => problemComments.Add(new ProblemCommentsCount() { ProblemID = pc.First().ProblemID, CommentsCount = pc.Count() }));

            response.NewComments = problemComments;

            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}
