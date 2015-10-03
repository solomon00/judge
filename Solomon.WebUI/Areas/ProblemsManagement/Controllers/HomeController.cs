using Solomon.Domain.Abstract;
using System.Web.Mvc;
using System.Linq;
using Solomon.Domain.Entities;
using System.Web.Security;
using System;
using Solomon.WebUI.Areas.ProblemsManagement.ViewModels;
using log4net;
using log4net.Config;
using Solomon.TypesExtensions;
using WebMatrix.WebData;

namespace Solomon.WebUI.Areas.ProblemsManagement.Controllers
{
    [Authorize(Roles = "Judge, Administrator")]
    public class HomeController : Controller
    {
        private IRepository repository;
        private readonly ILog logger = LogManager.GetLogger(typeof(HomeController));

        /// <summary>
        /// Controller constructor.
        /// Initialize repository.
        /// </summary>
        /// <param name="Repository">Repository object</param>
        public HomeController(IRepository Repository)
        {
            XmlConfigurator.Configure();
            repository = Repository;
        }

        public virtual ActionResult Index()
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited ProblemsManagement/Home/Index");

            HomeViewModel viewModel = new HomeViewModel();

            viewModel.TotalProblemCount = repository.Problems.Count().ToString();
            viewModel.StandartProblemCount = repository.Problems.Count(p => p.Type == ProblemTypes.Standart).ToString();
            viewModel.InteractiveProblemCount = repository.Problems.Count(p => p.Type == ProblemTypes.Interactive).ToString();
            viewModel.OpenProblemCount = repository.Problems.Count(p => p.Type == ProblemTypes.Open).ToString();

            return View(viewModel);
        }
    }
}
