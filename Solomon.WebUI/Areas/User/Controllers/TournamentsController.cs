using log4net;
using log4net.Config;
using Solomon.Domain.Abstract;
using Solomon.Domain.Entities;
using Solomon.TypesExtensions;
using Solomon.WebUI.Areas.User.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace Solomon.WebUI.Areas.User.Controllers
{
    [Authorize]
    public class TournamentsController : Controller
    {
        private IRepository repository;
        private readonly ILog logger = LogManager.GetLogger(typeof(TournamentsController));
        /// <summary>
        /// Controller constructor.
        /// Initialize repository.
        /// </summary>
        /// <param name="Repository">Repository object</param>
        public TournamentsController(IRepository Repository)
        {
            XmlConfigurator.Configure();
            repository = Repository;
        }

        //
        // GET: /Account/Profile/

        public ActionResult Index()
        {
            int userID = WebSecurity.CurrentUserId;
            UserProfile user = repository
                .Users
                .FirstOrDefault(u => u.UserId == userID);

            TournamentListViewModel viewModel = new TournamentListViewModel();

            viewModel.ActiveTournaments = repository
                .Tournaments
                .Where(t => (t.Users.FirstOrDefault(u => u.UserId == userID) != null ||
                        t.Teams.FirstOrDefault(team => team.UserID == userID) != null) && 
                        t.StartDate <= DateTime.Now && t.EndDate > DateTime.Now)
                .OrderByDescending(t => t.StartDate)
                .AsEnumerable();

            viewModel.NotBegunTournaments = repository
                .Tournaments
                .Where(t => (t.Users.FirstOrDefault(u => u.UserId == userID) != null ||
                        t.Teams.FirstOrDefault(team => team.UserID == userID) != null) &&
                        t.StartDate > DateTime.Now)
                .OrderByDescending(t => t.StartDate)
                .AsEnumerable();

            viewModel.FinishTournaments = repository
                .Tournaments
                .Where(t => (t.Users.FirstOrDefault(u => u.UserId == userID) != null ||
                        t.Teams.FirstOrDefault(team => team.UserID == userID) != null) &&
                        t.EndDate < DateTime.Now)
                .OrderByDescending(t => t.StartDate)
                .AsEnumerable();

            return View(viewModel);
        }

    }
}
