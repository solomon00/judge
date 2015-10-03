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

namespace Solomon.WebUI.Controllers
{
    public class TeamController : Controller
    {
        private IRepository repository;
        private readonly ILog logger = LogManager.GetLogger(typeof(TeamController));

        public TeamController(IRepository Repository)
        {
            XmlConfigurator.Configure();
            repository = Repository;
        }

        public ActionResult Index(int UserID = -1)
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited Home/Index");

            return null;
        }

    }
}
