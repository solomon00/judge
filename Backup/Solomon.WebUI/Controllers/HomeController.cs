using log4net;
using log4net.Config;
using Solomon.Domain.Abstract;
using Solomon.WebUI.ViewModels;
using System.Web.Mvc;
using System.Web.Security;
using System.Linq;
using WebMatrix.WebData;
using System;
using Solomon.TypesExtensions;
using Solomon.Domain.Entities;

namespace Solomon.WebUI.Controllers
{
    public class HomeController : Controller
    {
        private IRepository repository;
        private readonly ILog logger = LogManager.GetLogger(typeof(HomeController));

        public HomeController(IRepository Repository)
        {
            XmlConfigurator.Configure();
            repository = Repository;
        }

        public ActionResult Index()
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited Home/Index");

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

        public ActionResult About()
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited Home/About");
            //ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited Home/Contacts");
            //ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
