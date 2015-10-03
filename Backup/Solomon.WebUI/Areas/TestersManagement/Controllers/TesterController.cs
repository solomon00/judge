using System;
using System.Web.Mvc;
using Solomon.TypesExtensions;
using System.Collections.Generic;
using Solomon.Domain.Abstract;
using System.Linq;
using Solomon.WebUI.Areas.TestersManagement.ViewModels;
using System.Web;
using Solomon.WebUI.Testers;
using log4net;
using log4net.Config;
using WebMatrix.WebData;
using Solomon.Domain.Entities;
using System.IO;

namespace Solomon.WebUI.Areas.TestersManagement.Controllers
{
    [Authorize(Roles = "Administrator")]
    public partial class TesterController : Controller
    {
        private IRepository repository;
        private TestersSingleton testers;
        private readonly ILog logger = LogManager.GetLogger(typeof(TesterController));

        /// <summary>
        /// Controller constructor.
        /// </summary>
        /// <param name="Repository">Repository object</param>
        public TesterController(IRepository Repository)
        {
            XmlConfigurator.Configure();
            testers = TestersSingleton.Instance;
            repository = Repository;
        }

        #region Index Method
        public ActionResult Index()
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited TestersManagement/Tester/Index");

            ManageTestersViewModel viewModel = new ManageTestersViewModel();

            foreach (SocketClient client in testers.Clients)
            {
                viewModel.Testers.Add(new ShowTesterViewModel()
                {
                    Address = client.Address.ToString(),
                    CPULoad = client.ClientCPULoad,
                    IsConnected = client.IsConnected
                });
            }

            return View(viewModel);
        }

        [HttpGet]
        public JsonResult GetTesterInfo(string Address = null)
        {
            var response = new JsonResponseTesters();

            IEnumerable<SocketClient> clients = Address == null
                ? testers.Clients
                : testers.Clients.Where(t => t.Address.ToString() == Address);

            foreach (SocketClient client in clients)
            {
                response.Testers.Add(new ShowTesterViewModel()
                {
                    Address = client.Address.ToString(),
                    CPULoad = client.ClientCPULoad,
                    IsConnected = client.IsConnected,
                    ProcessorsCount = client.ClientVirtualProcessorsCount,
                    CheckingSolutionsCount = client.ClientVirtualProcessorsCount * 3 / 2 - client.ClientFreeThreadsCount
                });
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteDisconnectedTesters()
        {
            TestersSingleton.Instance.RemoveDisconnectedClients();

            return RedirectToAction("Index", "Tester");
        }
        #endregion

        public ActionResult Show(string Address = null, 
            int Page = 1, int PageSize = 25, string FilterBy = "all", string SearchTerm = null)
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited TestersManagement/Tester/Show");

            SocketClient client = testers.Clients.FirstOrDefault(c => c.Address.ToString() == Address);

            if (client == null)
            {
                logger.Warn("Client with address = " + Address + " not found");
                throw new HttpException(404, "Client end point not found");
            }

            ShowTesterViewModel viewModel = new ShowTesterViewModel()
                {
                    Address = Address,
                    CPULoad = client.ClientCPULoad,
                    IsConnected = client.IsConnected,
                    ProcessorsCount = client.ClientVirtualProcessorsCount,
                    CheckingSolutionsCount = client.ClientVirtualProcessorsCount * 3 / 2 - client.ClientFreeThreadsCount,
                    Compilers = client.ClientCompilers,

                    FilterBy = FilterBy,
                    SearchTerm = SearchTerm,
                    PageSize = PageSize == 0 ? 25 : PageSize
                };

            if (!string.IsNullOrEmpty(FilterBy))
            {
                if (FilterBy == "all" || string.IsNullOrEmpty(SearchTerm))
                {
                    viewModel.PaginatedProblemList = client
                        .Problems
                        .ToPaginatedList<ProblemInfo>(Page, PageSize);
                }
                else if (!string.IsNullOrEmpty(SearchTerm))
                {
                    if (FilterBy == "name")
                    {
                        viewModel.PaginatedProblemList = client
                            .Problems
                            .Where(p => p.Name.ToLower().IndexOf(SearchTerm.ToLower()) != -1)
                            .OrderBy(p => p.Name)
                            .ToPaginatedList<ProblemInfo>(Page, PageSize);
                    }
                    else if (FilterBy == "id")
                    {
                        viewModel.PaginatedProblemList = client
                            .Problems
                            .Where(p => p.ProblemID == Int32.Parse(SearchTerm))
                            .OrderBy(p => p.Name)
                            .ToPaginatedList<ProblemInfo>(Page, PageSize);
                    }
                }
            }

            viewModel.PaginatedProblemList.Each(pi =>
                {
                    if (repository.Problems.Contains(p => p.ProblemID == pi.ProblemID))
                    {
                        pi.Name = repository.Problems.First(p => p.ProblemID == pi.ProblemID).Name;
                    }
                });

            return View(viewModel);
        }

        public ActionResult Solutions(int Page = 1, int PageSize = 25)
        {
            int userID = WebSecurity.CurrentUserId;

            UserProfile user = repository
                .Users
                .FirstOrDefault(u => u.UserId == userID);
            PaginatedList<Solution> solutions = repository
                    .Solutions
                    .OrderByDescending(s => s.SendTime)
                    .ToPaginatedList<Solution>(Page, PageSize);

            ViewData["Solutions"] = solutions;
            repository.ProgrammingLanguages.Each(pl => ViewData[pl.ProgrammingLanguageID.ToString()] = pl.Title);

            SolutionsViewModel viewModel = new SolutionsViewModel()
            {
                HasNextPage = solutions.HasNextPage,
                HasPreviousPage = solutions.HasPreviousPage,
                PageIndex = Page,
                PageSize = PageSize,
                TotalPages = solutions.TotalPages,
                TotalCount = solutions.TotalCount
            };

            return View(viewModel);
        }

        /// <summary>
        /// Partial view for solutions table
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetSolutionsData(int Page = 1, int PageSize = 25)
        {
            int UserID = WebSecurity.CurrentUserId;

            PaginatedList<Solution> solutions = repository
                .Solutions
                .OrderByDescending(s => s.SendTime)
                .ToPaginatedList<Solution>(Page, PageSize);

            var response = new JsonResponseSolutionsData();

            // Get text view.
            using (var sw = new StringWriter())
            {
                ViewData["Solutions"] = solutions;
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
    }
}
