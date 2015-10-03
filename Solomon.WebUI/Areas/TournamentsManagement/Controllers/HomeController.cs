using Solomon.Domain.Abstract;
using System.Web.Mvc;
using System.Linq;
using Solomon.Domain.Entities;
using System.Web.Security;
using System;
using Solomon.WebUI.Areas.TournamentsManagement.ViewModels;
using log4net;
using log4net.Config;
using Solomon.TypesExtensions;
using WebMatrix.WebData;

namespace Solomon.WebUI.Areas.TournamentsManagement.Controllers
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
                " \"" + User.Identity.Name + "\" visited TournamentsManagement/Home/Index");

            HomeViewModel viewModel = new HomeViewModel();

            viewModel.TotalTournamentCount = repository.Tournaments.Count().ToString();
            viewModel.TotalActiveTournamentCount = repository
                .Tournaments
                .Count(t => t.StartDate < DateTime.Now && t.EndDate > DateTime.Now)
                .ToString();
            viewModel.TotalFinishTournamentCount = repository
                .Tournaments
                .Count(t => t.EndDate < DateTime.Now)
                .ToString();
            viewModel.TotalNotBegunTournamentCount = repository
                .Tournaments
                .Count(t => t.StartDate > DateTime.Now)
                .ToString();

            viewModel.ACMTournamentCount = repository.Tournaments.Count(t => t.Format == TournamentFormats.ACM).ToString();
            viewModel.IOITournamentCount = repository.Tournaments.Count(t => t.Format == TournamentFormats.IOI).ToString();
            viewModel.OpenTournamentCount = repository.Tournaments.Count(t => t.Type == TournamentTypes.Open).ToString();
            viewModel.CloseTournamentCount = repository.Tournaments.Count(t => t.Type == TournamentTypes.Close).ToString();

            return View(viewModel);
        }
    }
}
