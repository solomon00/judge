using Solomon.WebUI.Areas.AccountsManagement.ViewModels;
using Solomon.Domain.Abstract;
using System.Web.Mvc;
using System.Linq;
using Solomon.Domain.Entities;
using System.Web.Security;
using System.Web;
using log4net;
using log4net.Config;
using WebMatrix.WebData;
using System;
using System.Data.Objects;

namespace Solomon.WebUI.Areas.AccountsManagement.Controllers
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

        public ActionResult Index()
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited AccountsManagement/Home/Index");

            HomeViewModel viewModel = new HomeViewModel();

            IQueryable<UserProfile> users = repository.Users;

            if (!Roles.IsUserInRole("Administrator"))
            {
                users = users.Where(u => u.CreatedByUserID == WebSecurity.CurrentUserId);
            }

            //membershipService.GetAllUsers(0, 20, out totalRecords);
            viewModel.TotalUserCount = users.Count(u => u.UserId != 1).ToString();
            viewModel.TotalOnlineUsersCount = repository
                .Users
                .Where(u => u.UserId != 1)
                .Count(u => EntityFunctions.DiffMinutes(u.LastAccessTime, DateTime.Now) < 10).ToString();
            viewModel.TotalRolesCount = Roles.GetAllRoles().Length.ToString();

            return View(viewModel);
        }
    }
}
