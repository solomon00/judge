using System;
using System.Web.Mvc;
using System.Web.Security;
using Solomon.WebUI.Controllers;
using Solomon.TypesExtensions;
using viewModels = Solomon.WebUI.Areas.ProblemsManagement.ViewModels;
using System.Collections.Generic;
using WebMatrix.WebData;
using Solomon.Domain.Abstract;
using Solomon.Domain.Entities;
using System.Linq;
using Solomon.WebUI.Models;
using Solomon.WebUI.Areas.ProblemsManagement.ViewModels;
using System.Web;
using System.Xml;
using System.IO;
using System.IO.Compression;
using Solomon.WebUI.Infrastructure;
using Solomon.WebUI.Helpers;
using log4net;
using log4net.Config;
using Solomon.WebUI.Testers;

namespace Solomon.WebUI.Areas.ProblemsManagement.Controllers
{
    [Authorize(Roles = "Judge, Administrator")]
    public partial class ProblemController : Controller
    {
        private IRepository repository;
        private readonly ILog logger = LogManager.GetLogger(typeof(ProblemController));

        /// <summary>
        /// Controller constructor.
        /// Initialize repository.
        /// </summary>
        /// <param name="Repository">Repository object</param>
        public ProblemController(IRepository Repository)
        {
            XmlConfigurator.Configure();
            this.repository = Repository;
        }

        #region Index Method
        public ActionResult Index(int Page = 1, int PageSize = 25, string FilterBy = "all", string SearchTerm = null)
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited ProblemsManagement/Problem/Index");

            ManageProblemsViewModel viewModel = new ManageProblemsViewModel();
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
                    viewModel.PaginatedProblemList = repository.Problems
                            .OrderByDescending(p => p.ProblemID)
                            .ToPaginatedList<Problem>(Page, PageSize);
                }
                else if (!string.IsNullOrEmpty(SearchTerm))
                {
                    if (FilterBy == "name")
                    {
                        viewModel.PaginatedProblemList = repository.Problems
                            .Where(p => p.Name.ToLower().IndexOf(SearchTerm.ToLower()) != -1)
                            .OrderByDescending(p => p.ProblemID)
                            .ToPaginatedList<Problem>(Page, PageSize);
                    }
                }
            }

            return View(viewModel);
        }
        #endregion


        #region Create, Update Problem Helpers
        private void SaveChecker(string CheckerID, HttpPostedFileBase Checker, string DirToSave)
        {
            if (CheckerID == "-1") return;

            // Store the file inside ~/App_Data/uploads folder
            var path = Path.Combine(LocalPath.AbsoluteProblemsDirectory + DirToSave, "checker.cpp");
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
            path = Path.Combine(LocalPath.AbsoluteProblemsDirectory + DirToSave, "checker.pas");
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
            path = Path.Combine(LocalPath.AbsoluteProblemsDirectory + DirToSave, "checker.dpr");
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            if (CheckerID == "other" && Checker != null && Checker.ContentLength > 0)
            {
                string checkerName = "checker" + Path.GetExtension(Checker.FileName);

                if (checkerName != "checker.cpp" && checkerName != "checker.pas" && checkerName != "checker.dpr")
                {
                    ModelState.AddModelError("Checker", "Чекер неккоректен");
                    throw new ArgumentException("Checker is not valid");
                }

                path = Path.Combine(LocalPath.AbsoluteProblemsDirectory + DirToSave, checkerName);

                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }

                Checker.SaveAs(path);

                return;
            }

            // std checkers on cpp
            path = Path.Combine(LocalPath.AbsoluteProblemsDirectory + DirToSave, "checker.cpp");
            StdCheckers.Checker checker = StdCheckers.Checkers.FirstOrDefault(ch => ch.CheckerID == CheckerID && ch.Available);
            if (checker == null)
            {
                throw new ArgumentException("Checker id is not valid");
            }

            if (!System.IO.File.Exists(LocalPath.AbsoluteCheckersDirectory + checker.FileName))
            {
                throw new ArgumentException("Checker is not exists");
            }

            System.IO.File.Copy(LocalPath.AbsoluteCheckersDirectory + checker.FileName, path);
        }

        private void SaveTests(HttpPostedFileBase Tests, string DirToSave)
        {
            if (Tests != null && Tests.ContentLength > 0)
            {
                // Extract only the fielname
                var fileName = Path.GetFileName(Tests.FileName);

                // Store the file inside ~/App_Data/uploads folder
                var path = Path.Combine(LocalPath.AbsoluteProblemsDirectory + DirToSave, fileName);

                Tests.SaveAs(path);

                if (!Directory.Exists(LocalPath.AbsoluteProblemsDirectory + DirToSave + "/Tests"))
                {
                    Directory.CreateDirectory(LocalPath.AbsoluteProblemsDirectory + DirToSave + "/Tests");
                }

                ZipFile.ExtractToDirectory(path, LocalPath.AbsoluteProblemsDirectory + DirToSave + "/Tests");

                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
        }

        private void SaveSamples(HttpPostedFileBase Samples, string DirToSave)
        {
            if (Samples != null && Samples.ContentLength > 0)
            {
                // Extract only the fielname
                var fileName = Path.GetFileName(Samples.FileName);

                // Store the file inside ~/App_Data/uploads folder
                var path = Path.Combine(LocalPath.AbsoluteProblemsDirectory + DirToSave, fileName);
                Samples.SaveAs(path);

                if (!Directory.Exists(LocalPath.AbsoluteProblemsDirectory + DirToSave + "/Samples"))
                {
                    Directory.CreateDirectory(LocalPath.AbsoluteProblemsDirectory + DirToSave + "/Samples");
                }

                ZipFile.ExtractToDirectory(path, LocalPath.AbsoluteProblemsDirectory + DirToSave + "/Samples");

                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
        }

        private void SaveOpenProblemResult(HttpPostedFileBase Result, string DirToSave)
        {
            if (Result != null && Result.ContentLength > 0)
            {
                // Extract only the fielname
                //var fileName = Path.GetFileName(Result.FileName);

                // Store the file inside ~/App_Data/uploads folder
                var path = Path.Combine(LocalPath.AbsoluteProblemsDirectory + DirToSave, "OpenProblemResult");

                Result.SaveAs(path);

                //if (!Directory.Exists(LocalPath.AbsoluteProblemsDirectory + DirToSave + "/Tests"))
                //{
                //    Directory.CreateDirectory(LocalPath.AbsoluteProblemsDirectory + DirToSave + "/Tests");
                //}

                //ZipFile.ExtractToDirectory(path, LocalPath.AbsoluteProblemsDirectory + DirToSave + "/Tests");

                //if (System.IO.File.Exists(path))
                //{
                //    System.IO.File.Delete(path);
                //}
            }
        }

        private void SaveImages(List<string> Path, string DirToSave)
        {
            if (!Directory.Exists(System.IO.Path.Combine(DirToSave, LocalPath.RelativeProblemsAttachDirectory)))
                Directory.CreateDirectory(System.IO.Path.Combine(DirToSave, LocalPath.RelativeProblemsAttachDirectory));

            foreach (var item in Path)
            {
                if (System.IO.File.Exists(Server.MapPath(item)))
                {
                    if (System.IO.File.Exists(
                        System.IO.Path.Combine(DirToSave, LocalPath.RelativeProblemsAttachDirectory, System.IO.Path.GetFileName(item))))
                    {
                        System.IO.File.Delete(
                            System.IO.Path.Combine(DirToSave, LocalPath.RelativeProblemsAttachDirectory, System.IO.Path.GetFileName(item)));
                    }

                    System.IO.File.Move(Server.MapPath(item),
                        System.IO.Path.Combine(DirToSave, LocalPath.RelativeProblemsAttachDirectory, System.IO.Path.GetFileName(item)));
                }
            }
            
        }

        private List<string> ParseImagesPath(string Text)
        {
            List<string> path = new List<string>();

            int startIndex = 0;
            int count = 0;
            while (Text != null && startIndex < Text.Length && startIndex != -1)
            {
                try
                {
                    startIndex = Text.IndexOf("[attach=", startIndex);
                    count = Text.IndexOf("]", startIndex) - startIndex;
                    path.Add(Text.Substring(startIndex + "[attach=".Length, count - "[attach=".Length));

                    startIndex += count;
                }
                catch (Exception) { }
            }

            return path;
        }
        private void SaveProblemStandart(viewModels.NewProblemViewModel Model, int ProblemID, DateTime LastModifiedTime)
        {
            // Write model data to App_Data

            string dirName = ProblemID.ToString();
            if (!Directory.Exists(LocalPath.AbsoluteProblemsDirectory + dirName))
            {
                Directory.CreateDirectory(LocalPath.AbsoluteProblemsDirectory + dirName);
            }

            // Write problem legend
            ProblemLegend.Write(LocalPath.AbsoluteProblemsDirectory + dirName,
                Model.Name, Model.TimeLimit, Model.MemoryLimit, LastModifiedTime, ProblemTypes.Standart,
                Model.Description, Model.InputFormat, Model.OutputFormat);

            SaveChecker(Model.CheckerListID, Model.Checker, dirName);

            SaveTests(Model.Tests, dirName);

            // Create zip for tester
            ZipFile.CreateFromDirectory(
                LocalPath.AbsoluteProblemsDirectory + dirName,
                LocalPath.AbsoluteProblemsDirectory + dirName + ".zip");

            // Save attachment
            List<string> attach = new List<string>();
            attach.AddRange(ParseImagesPath(Model.Description));
            attach.AddRange(ParseImagesPath(Model.InputFormat));
            attach.AddRange(ParseImagesPath(Model.OutputFormat));
            SaveImages(attach, LocalPath.AbsoluteProblemsDirectory + dirName);

            // Delete description, inputFormat and outputFormat from archive
            using (ZipArchive archive = ZipFile.Open(
                LocalPath.AbsoluteProblemsDirectory + dirName + ".zip", ZipArchiveMode.Update))
            {
                List<ZipArchiveEntry> forDelete = new List<ZipArchiveEntry>();

                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.Name == "Description" ||
                        entry.Name == "InputFormat" ||
                        entry.Name == "OutputFormat")
                    {
                        forDelete.Add(entry);
                    }
                }

                forDelete.Each(e => archive.Entries.First(en => en == e).Delete());
            }

            // Move tester zip
            if (System.IO.File.Exists(LocalPath.AbsoluteProblemsDirectory + dirName + ".zip"))
            {
                System.IO.File.Move(LocalPath.AbsoluteProblemsDirectory + dirName + ".zip",
                    LocalPath.AbsoluteProblemsDirectory + dirName + "/" + dirName + ".zip");
            }

            SaveSamples(Model.Samples, dirName);
        }
        private void SaveProblemStandart(viewModels.EditProblemViewModel Model, int ProblemID, DateTime LastModifiedTime)
        {
            // Write model data to App_Data

            string dirName = ProblemID.ToString();
            if (!Directory.Exists(LocalPath.AbsoluteProblemsDirectory + dirName))
            {
                throw new DirectoryNotFoundException("Problem directory not found");
            }

            if (System.IO.File.Exists(LocalPath.AbsoluteProblemsDirectory + dirName + "/" + dirName + ".zip"))
                System.IO.File.Delete(LocalPath.AbsoluteProblemsDirectory + dirName + "/" + dirName + ".zip");

            // Write problem legend
            ProblemLegend.Write(LocalPath.AbsoluteProblemsDirectory + dirName,
                Model.Name, Model.TimeLimit, Model.MemoryLimit, LastModifiedTime, ProblemTypes.Standart,
                Model.Description, Model.InputFormat, Model.OutputFormat);

            SaveChecker(Model.CheckerListID, Model.Checker, dirName);

            if (Model.TestsDropDownID == "other")
            {
                if (Directory.Exists(LocalPath.AbsoluteProblemsDirectory + dirName + "/Tests"))
                {
                    Directory.Delete(LocalPath.AbsoluteProblemsDirectory + dirName + "/Tests", true);
                }
                SaveTests(Model.Tests, dirName);
            }

            // Save attachment
            List<string> attach = new List<string>();
            attach.AddRange(ParseImagesPath(Model.Description));
            attach.AddRange(ParseImagesPath(Model.InputFormat));
            attach.AddRange(ParseImagesPath(Model.OutputFormat));
            SaveImages(attach, LocalPath.AbsoluteProblemsDirectory + dirName);

            // Create zip for tester
            if (System.IO.File.Exists(LocalPath.AbsoluteProblemsDirectory + dirName + ".zip"))
            {
                System.IO.File.Delete(LocalPath.AbsoluteProblemsDirectory + dirName + ".zip");
            }
            ZipFile.CreateFromDirectory(
                LocalPath.AbsoluteProblemsDirectory + dirName,
                LocalPath.AbsoluteProblemsDirectory + dirName + ".zip");

            // Delete description, inputFormat and outputFormat from archive
            using (ZipArchive archive = ZipFile.Open(
                LocalPath.AbsoluteProblemsDirectory + dirName + ".zip", ZipArchiveMode.Update))
            {
                List<ZipArchiveEntry> entriesForDelete = new List<ZipArchiveEntry>();
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.Name == "Description" ||
                        entry.Name == "InputFormat" ||
                        entry.Name == "OutputFormat")
                    {
                        entriesForDelete.Add(entry);
                    }
                }

                entriesForDelete.Each(e => e.Delete());
            }

            // Move tester zip
            if (System.IO.File.Exists(LocalPath.AbsoluteProblemsDirectory + dirName + ".zip"))
            {
                System.IO.File.Move(LocalPath.AbsoluteProblemsDirectory + dirName + ".zip",
                    LocalPath.AbsoluteProblemsDirectory + dirName + "/" + dirName + ".zip");
            }

            if (Model.SamplesDropDownID == "other")
            {
                if (Directory.Exists(LocalPath.AbsoluteProblemsDirectory + dirName + "/Samples"))
                {
                    Directory.Delete(LocalPath.AbsoluteProblemsDirectory + dirName + "/Samples", true);
                }
                SaveSamples(Model.Samples, dirName);
            }
        }

        private void SaveProblemOpen(viewModels.NewProblemViewModel Model, int ProblemID, DateTime LastModifiedTime)
        {
            // Write model data to App_Data
            ParseImagesPath(Model.Description);
            string dirName = ProblemID.ToString();
            if (!Directory.Exists(LocalPath.AbsoluteProblemsDirectory + dirName))
            {
                Directory.CreateDirectory(LocalPath.AbsoluteProblemsDirectory + dirName);
            }

            // Write problem legend
            ProblemLegend.Write(LocalPath.AbsoluteProblemsDirectory + dirName,
                Model.Name, Model.TimeLimit, Model.MemoryLimit, LastModifiedTime, ProblemTypes.Open,
                Model.Description, Model.InputFormat, Model.OutputFormat);

            SaveChecker(Model.CheckerListID, Model.Checker, dirName);

            SaveOpenProblemResult(Model.OpenProblemResult, dirName);

            // Create zip for tester
            ZipFile.CreateFromDirectory(
                LocalPath.AbsoluteProblemsDirectory + dirName,
                LocalPath.AbsoluteProblemsDirectory + dirName + ".zip");

            // Save attachment
            List<string> attach = new List<string>();
            attach.AddRange(ParseImagesPath(Model.Description));
            attach.AddRange(ParseImagesPath(Model.OutputFormat));
            SaveImages(attach, LocalPath.AbsoluteProblemsDirectory + dirName);

            // Delete description, inputFormat and outputFormat from archive
            using (ZipArchive archive = ZipFile.Open(
                LocalPath.AbsoluteProblemsDirectory + dirName + ".zip", ZipArchiveMode.Update))
            {
                List<ZipArchiveEntry> forDelete = new List<ZipArchiveEntry>();

                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.Name == "Description" ||
                        entry.Name == "InputFormat" ||
                        entry.Name == "OutputFormat")
                    {
                        forDelete.Add(entry);
                    }
                }

                forDelete.Each(e => archive.Entries.First(en => en == e).Delete());
            }

            // Move tester zip
            if (System.IO.File.Exists(LocalPath.AbsoluteProblemsDirectory + dirName + ".zip"))
            {
                System.IO.File.Move(LocalPath.AbsoluteProblemsDirectory + dirName + ".zip",
                    LocalPath.AbsoluteProblemsDirectory + dirName + "/" + dirName + ".zip");
            }
        }
        private void SaveProblemOpen(viewModels.EditProblemViewModel Model, int ProblemID, DateTime LastModifiedTime)
        {
            // Write model data to App_Data

            string dirName = ProblemID.ToString();
            if (!Directory.Exists(LocalPath.AbsoluteProblemsDirectory + dirName))
            {
                Directory.CreateDirectory(LocalPath.AbsoluteProblemsDirectory + dirName);
            }

            if (System.IO.File.Exists(LocalPath.AbsoluteProblemsDirectory + dirName + "/" + dirName + ".zip"))
                System.IO.File.Delete(LocalPath.AbsoluteProblemsDirectory + dirName + "/" + dirName + ".zip");

            // Write problem legend
            ProblemLegend.Write(LocalPath.AbsoluteProblemsDirectory + dirName,
                Model.Name, Model.TimeLimit, Model.MemoryLimit, LastModifiedTime, ProblemTypes.Open,
                Model.Description, Model.InputFormat, Model.OutputFormat);

            SaveChecker(Model.CheckerListID, Model.Checker, dirName);

            if (Model.OpenProblemResultDropDownID == "other")
            {
                if (System.IO.File.Exists(LocalPath.AbsoluteProblemsDirectory + dirName + "OpenProblemResult"))
                    System.IO.File.Delete(LocalPath.AbsoluteProblemsDirectory + dirName + "OpenProblemResult");
                SaveOpenProblemResult(Model.OpenProblemResult, dirName);
            }

            // Save attachment
            List<string> attach = new List<string>();
            attach.AddRange(ParseImagesPath(Model.Description));
            attach.AddRange(ParseImagesPath(Model.InputFormat));
            attach.AddRange(ParseImagesPath(Model.OutputFormat));
            SaveImages(attach, LocalPath.AbsoluteProblemsDirectory + dirName);

            // Create zip for tester
            ZipFile.CreateFromDirectory(
                LocalPath.AbsoluteProblemsDirectory + dirName,
                LocalPath.AbsoluteProblemsDirectory + dirName + ".zip");

            // Delete description, inputFormat and outputFormat from archive
            using (ZipArchive archive = ZipFile.Open(
                LocalPath.AbsoluteProblemsDirectory + dirName + ".zip", ZipArchiveMode.Update))
            {
                List<ZipArchiveEntry> forDelete = new List<ZipArchiveEntry>();

                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.Name == "Description" ||
                        entry.Name == "InputFormat" ||
                        entry.Name == "OutputFormat")
                    {
                        forDelete.Add(entry);
                    }
                }

                forDelete.Each(e => archive.Entries.First(en => en == e).Delete());
            }

            // Move tester zip
            if (System.IO.File.Exists(LocalPath.AbsoluteProblemsDirectory + dirName + ".zip"))
            {
                System.IO.File.Move(LocalPath.AbsoluteProblemsDirectory + dirName + ".zip",
                    LocalPath.AbsoluteProblemsDirectory + dirName + "/" + dirName + ".zip");
            }
        }


        private void SaveProblem(viewModels.NewProblemViewModel Model, int ProblemID, DateTime LastModifiedTime)
        {
            switch ((ProblemTypes)Model.ProblemTypesListID)
            {
                case ProblemTypes.Standart:
                    SaveProblemStandart(Model, ProblemID, LastModifiedTime);
                    break;
                case ProblemTypes.Open:
                    SaveProblemOpen(Model, ProblemID, LastModifiedTime);
                    break;
            }
        }
        private void SaveProblem(viewModels.EditProblemViewModel Model, int ProblemID, DateTime LastModifiedTime)
        {
            switch ((ProblemTypes)Model.ProblemTypesListID)
            {
                case ProblemTypes.Standart:
                    SaveProblemStandart(Model, ProblemID, LastModifiedTime);
                    break;
                case ProblemTypes.Open:
                    SaveProblemOpen(Model, ProblemID, LastModifiedTime);
                    break;
            }
        }
        #endregion

        #region Create Problem Methods

        //TODO: Tests format - *.in *.out
        public ActionResult Create()
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited ProblemsManagement/Problem/Create");

            var model = new viewModels.NewProblemViewModel();
            return View(model);
        }

        /// <summary>
        /// This method redirects to the BindTournamentsToProblem method.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(viewModels.NewProblemViewModel Model)
        {
            if (ModelState.IsValid)
            {
                int problemID = -1;

                // Attempt to create new problem
                try
                {
                    Problem problem = new Problem
                    {
                        Name = Model.Name,
                        TimeLimit = Model.TimeLimit,
                        MemoryLimit = Model.MemoryLimit,
                        LastModifiedTime = DateTime.Now.Truncate(TimeSpan.FromSeconds(1)), // Truncate need for comparing dates
                        Type = (ProblemTypes)Model.ProblemTypesListID,
                        Tags = new List<ProblemTag>()
                    };

                    foreach (var tagId in Model.ProblemTagsListIDs)
                    {
                        var tag = repository.ProblemTags.FirstOrDefault(t => t.ProblemTagID == tagId);
                        if (tag != null)
                            problem.Tags.Add(tag);
                    }

                    int userID = WebSecurity.CurrentUserId;
                    if (userID != 1)
                    {
                        UserProfile user = repository.Users.FirstOrDefault(u => u.UserId == userID);
                        problem.UsersCanModify = new List<UserProfile>();
                        problem.UsersCanModify.Add(user);
                    }
                    problemID = repository.AddProblem(problem);

                    SaveProblem(Model, problemID, problem.LastModifiedTime);

                    problem.Path = LocalPath.RelativeProblemsDirectory + problemID.ToString();

                    // Update problem in repository
                    repository.AddProblem(problem);

                    TestersSingleton.Instance.SendProblem(problem.ProblemID);

                    logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                        " \"" + User.Identity.Name + "\" create problem " + problemID + " \"" + Model.Name + "\"");

                    return RedirectToAction("BindTournamentsToProblem", new { problemID = problem.ProblemID });
                }
                catch (Exception ex)
                {
                    logger.Error("Error occurred on user " + WebSecurity.GetUserId(User.Identity.Name) +
                        " \"" + User.Identity.Name + "\" creating problem " + problemID + " \"" + Model.Name + "\": ", ex);

                    ModelState.AddModelError("", "Произошла ошибка при создании задачи");

                    repository.DeleteProblem(problemID);
                    if (Directory.Exists(LocalPath.AbsoluteProblemsDirectory + problemID.ToString()))
                    {
                        Directory.Delete(LocalPath.AbsoluteProblemsDirectory + problemID.ToString(), true);
                    }
                }
            }

            return View(Model);
        }

        /// <summary>
        /// An Ajax method to check if a name is unique.
        /// </summary>
        /// <param name="ProblemName"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult CheckForUniqueProblemName(string ProblemName, int ProblemID = -1)
        {
            Problem problem = repository
                .Problems
                .FirstOrDefault(p => p.Name == ProblemName && p.ProblemID != ProblemID);
            JsonResponse response = new JsonResponse();
            response.Exists = (problem == null) ? false : true;

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

        #region View Problem Details Methods

        [HttpGet]
        public ActionResult Update(int ProblemID = -1)
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited ProblemsManagement/Problem/Update");

            Problem problem = repository.Problems.FirstOrDefault(p => p.ProblemID == ProblemID);

            if (problem == null)
            {
                logger.Warn("Problem with id = " + ProblemID + " not found");
                throw new HttpException(404, "Problem not found");
            }

            // Read problem info
            string name, description = "Error occurred", 
                inputFormat = "Error occurred",
                outputFormat = "Error occurred",
                checkerCode = "Error occurred";
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

                string checkerPath = "";
                if (System.IO.File.Exists(LocalPath.AbsoluteProblemsDirectory + problem.ProblemID + "/checker.cpp"))
                {
                    checkerPath = LocalPath.AbsoluteProblemsDirectory + problem.ProblemID + "/checker.cpp";
                }
                else if (System.IO.File.Exists(LocalPath.AbsoluteProblemsDirectory + problem.ProblemID + "/checker.pas"))
                {
                    checkerPath = LocalPath.AbsoluteProblemsDirectory + problem.ProblemID + "/checker.pas";
                }

                using (StreamReader sr = new StreamReader(checkerPath))
                {
                    checkerCode = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred on problem legend reading:", ex);
            }

            EditProblemViewModel viewModel = new EditProblemViewModel()
            {
                ProblemID = problem.ProblemID,
                Name = problem.Name,
                ProblemTypesListID = (int)problem.Type,
                
                TimeLimit = problem.TimeLimit,
                MemoryLimit = problem.MemoryLimit,

                Description = description,
                InputFormat = inputFormat,
                OutputFormat = outputFormat,

                CheckerCode = checkerCode,

                Tournaments = problem.Tournaments.Select(t => t.Name),

                CheckPending = problem.CheckPending
            };
            repository.ProblemTags.Each(t => 
                viewModel.ProblemTagsList.Add(new SelectListItem() 
                    { 
                        Text = t.Name, 
                        Value = t.ProblemTagID.ToString(),
                        Selected = problem.Tags.Count(tag => tag.ProblemTagID == t.ProblemTagID) > 0
                    }
                )
            );
            
            return View(viewModel);
        }

        [HttpGet]
        [Authorize]
        public FilePathResult GetProblemFile(int ProblemID)
        {
            Problem problem = repository
                .Problems
                .FirstOrDefault(p => p.ProblemID == ProblemID);

            if (problem == null)
            {
                logger.Warn("Problem with id = " + ProblemID + " not found");
                throw new HttpException(404, "Problem with id = " + ProblemID + " not found");
            }

            if (!Roles.IsUserInRole("Judge") &&
                !Roles.IsUserInRole("Administrator") &&
                problem.UsersCanModify.Count(u => u.UserId == WebSecurity.CurrentUserId) == 0)
            {
                logger.Warn("Unauthorized explained: User " + WebSecurity.CurrentUserId + " try get access to solution " + ProblemID);
                throw new HttpException(401, "Unauthorized explained");
            }

            logger.Info("User " + WebSecurity.CurrentUserId +
                " \"" + WebSecurity.CurrentUserName + "\" download solution " + ProblemID);

            Response.AppendHeader("Content-Disposition", "attachment; filename=" + problem.Name + ".zip");

            return new FilePathResult(Path.Combine(LocalPath.RootDirectory, problem.Path, ProblemID + ".zip"), MimeMapping.GetMimeMapping(".zip"));
        }

        [HttpPost]
        public ActionResult Update(viewModels.EditProblemViewModel Model, int ProblemID = -1, string Update = null, string Delete = null, string Cancel = null)
        {
            if (ProblemID == -1)
            {
                logger.Warn("Problem with id = " + ProblemID + " not found");
                throw new HttpException(404, "Problem not found");
            }

            if (Delete != null)
                return DeleteProblem(ProblemID);
            if (Cancel != null)
                return CancelProblem();
            if (Update != null)
                return UpdateProblem(Model, ProblemID);

            return CancelProblem();

        }
        
        private ActionResult UpdateProblem(viewModels.EditProblemViewModel Model, int ProblemID = -1)
        {
            try
            {
                Problem problem = repository.Problems.FirstOrDefault(p => p.ProblemID == ProblemID);

                if (problem == null)
                {
                    logger.Warn("Problem with id = " + ProblemID + " not found");
                    throw new HttpException(404, "Problem not found");
                }

                problem.Name = Model.Name;
                problem.TimeLimit = Model.TimeLimit;
                problem.MemoryLimit = Model.MemoryLimit;
                problem.LastModifiedTime = DateTime.Now.Truncate(TimeSpan.FromSeconds(1));
                problem.CheckPending = Model.CheckPending;
                //problem.Type = (ProblemTypes)Model.ProblemTypesListID;
                if (Model.ProblemTagsListIDs != null)
                {
                    foreach (var tagId in Model.ProblemTagsListIDs)
                    {
                        var tag = repository.ProblemTags.FirstOrDefault(t => t.ProblemTagID == tagId);
                        if (tag != null)
                            problem.Tags.Add(tag);
                    }
                }

                repository.AddProblem(problem);

                SaveProblem(Model, ProblemID, problem.LastModifiedTime);

                logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) +
                    " \"" + User.Identity.Name + "\" update problem \"" + ProblemID + "\"");

                TestersSingleton.Instance.SendProblem(ProblemID);

                TempData["SuccessMessage"] = "Задача успешно обновлена!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Произошла ошибка при обновлении задачи.";
                logger.Error("Error occured on user " + WebSecurity.GetUserId(User.Identity.Name) + 
                    " \"" + User.Identity.Name + "\" updating problem \"" + ProblemID + "\": ", ex);
            }

            return RedirectToAction("Update", new { ProblemID = ProblemID });
        }

        #endregion

        #region Delete Problem Methods

        private ActionResult DeleteProblem(int ProblemID)
        {
            if (ProblemID == -1)
            {
                logger.Warn("Problem with id = " + ProblemID + " not found");
                throw new HttpException(404, "Problem not found");
            }

            try
            {
                repository.DeleteProblem(ProblemID);
                TestersSingleton.Instance.DeleteProblem(ProblemID);

                if (Directory.Exists(LocalPath.AbsoluteProblemsDirectory + ProblemID.ToString()))
                {
                    Directory.Delete(LocalPath.AbsoluteProblemsDirectory + ProblemID.ToString(), true);
                }

                logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                    " \"" + User.Identity.Name + "\" delete problem with id = " + ProblemID);
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Произошла ошибка при удалении задачи";
                logger.Error("Error occurred on user " + WebSecurity.GetUserId(User.Identity.Name) + 
                    " \"" + User.Identity.Name + "\" deleting problem with id = " + ProblemID + ": ", ex);
            }

            return RedirectToAction("Update", new { ProblemID = ProblemID });
        }



        #endregion

        #region Cancel Problem Methods

        private ActionResult CancelProblem()
        {
            return RedirectToAction("Index");
        }

        #endregion


        #region Bind Tournaments To Problem Methods

        /// <summary>
        /// Return two lists:
        ///   1)  a list of Problems not bound to the tournament
        ///   2)  a list of Problems granted to the tournament
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult BindTournamentsToProblem(int ProblemID = -1, int Page = 1, int PageSize = 25, string FilterBy = "all", string SearchTerm = null)
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited ProblemsManagement/Problem/BindTournamentsToProblem");

            if (ProblemID == -1)
            {
                ManageProblemsViewModel viewModel = new ManageProblemsViewModel();
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
                        viewModel.PaginatedProblemList = repository.Problems
                                .OrderByDescending(p => p.ProblemID)
                                .ToPaginatedList<Problem>(Page, PageSize);
                    }
                    else if (!string.IsNullOrEmpty(SearchTerm))
                    {
                        if (FilterBy == "name")
                        {
                            viewModel.PaginatedProblemList = repository.Problems
                                .Where(p => p.Name.ToLower().IndexOf(SearchTerm.ToLower()) != -1)
                                .OrderByDescending(p => p.ProblemID)
                                .ToPaginatedList<Problem>(Page, PageSize);
                        }
                    }
                }
                return View("TournamentsForProblem", viewModel);
            }

            BindTournamentsToProblemViewModel model = new BindTournamentsToProblemViewModel();
            Problem problem = repository.Problems
                .FirstOrDefault(p => p.ProblemID == ProblemID);
            if (problem == null)
            {
                logger.Warn("Problem with id = " + ProblemID + " not found");
                throw new HttpException(404, "Problem not found");
            }

            model.ProblemID = problem.ProblemID;
            model.ProblemName = problem.Name;

            // AsEnumerable() need, because Except() for IQueryable work in DB.
            if (problem.Tournaments != null)
            {

                model.AvailableTournaments = new SelectList(repository.Tournaments.AsEnumerable().Except(problem.Tournaments).OrderByDescending(t => t.TournamentID), "TournamentID", "Name");
                model.BoundTournaments = new SelectList(problem.Tournaments, "TournamentID", "Name");
            }
            else
            {
                model.AvailableTournaments = new SelectList(repository.Tournaments.AsEnumerable().OrderByDescending(t => t.TournamentID), "TournamentID", "Name");
                model.BoundTournaments = null;
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
        public ActionResult BindTournamentToProblem(int tournamentID = -1, int problemID = -1)
        {
            JsonResponse response = new JsonResponse();

            if (tournamentID == -1)
            {
                response.Success = false;
                response.Message = "Идентификатор турнира некорректен.";
                return Json(response);
            }

            if (problemID == -1)
            {
                response.Success = false;
                response.Message = "Идентификатор задачи некорректен.";
                return Json(response);
            }

            try
            {
                repository.BindProblemToTournament(tournamentID, problemID);

                response.Success = true;
                response.Message = "Задача успешно добавлена в турнир.";

                logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) +
                    " \"" + User.Identity.Name + "\" bind problem \"" + problemID + "\" to tournament \"" + tournamentID + "\" ");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Произошла ошибка при добавлении задачи в турнир.";

                logger.Warn("Error occurred on user " + WebSecurity.GetUserId(User.Identity.Name) +
                    " \"" + User.Identity.Name + "\" binding problem \"" + problemID + "\" to tournament \"" + tournamentID + "\": ", ex);
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
        public ActionResult UnbindTournamentFromProblem(int tournamentID = -1, int problemID = -1)
        {
            JsonResponse response = new JsonResponse();

            if (tournamentID == -1)
            {
                response.Success = false;
                response.Message = "Идентификатор турнира некорректен.";
                return Json(response);
            }

            if (problemID == -1)
            {
                response.Success = false;
                response.Message = "Идентификатор задачи некорректен.";
                return Json(response);
            }

            try
            {
                repository.UnbindProblemFromTournament(tournamentID, problemID);

                response.Success = true;
                response.Message = "Задача успешно удалена из турнира.";

                logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) +
                    " \"" + User.Identity.Name + "\" unbind problem \"" + problemID + "\" from tournament \"" + tournamentID + "\" ");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Произошла ошибка при удалении задачи из турнира.";

                logger.Warn("Error occurred on user " + WebSecurity.GetUserId(User.Identity.Name) +
                    " \"" + User.Identity.Name + "\" unbinding problem \"" + problemID + "\" from tournament \"" + tournamentID + "\": ", ex);
            }

            return Json(response);
        }

        #endregion




        [HttpPost]
        public WrappedJsonResult UploadImage(HttpPostedFileWrapper ImageFile)
        {
            if (ImageFile == null || ImageFile.ContentLength == 0)
            {
                return new WrappedJsonResult
                {
                    Data = new
                    {
                        IsValid = false,
                        Message = "Не удалось загрузить изображение",
                        ImagePath = string.Empty
                    }
                };
            }

            var fileName = String.Format("{0}{1}", Guid.NewGuid().ToString(), Path.GetExtension(ImageFile.FileName));
            var imagePath = Path.Combine(LocalPath.AbsoluteUploadsDirectory, fileName);

            ImageFile.SaveAs(imagePath);

            return new WrappedJsonResult
            {
                Data = new
                {
                    IsValid = true,
                    Message = string.Empty,
                    ImagePath = Url.Content("~/" + Path.Combine(LocalPath.RelativeUploadsDirectory, fileName))
                }
            };
        }
    }
}
