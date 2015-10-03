using log4net;
using log4net.Config;
using System.Web.Mvc;
using System.Web.Security;

namespace Solomon.WebUI.Controllers
{
    public class ErrorsController : Controller
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(ErrorsController));

        public ErrorsController()
        {
            XmlConfigurator.Configure();
        }

        public ViewResult Index()
        {
            return View();
        }

        public ViewResult UnauthorizedAccess()
        {
            return View();
        }

        public ViewResult Error404(string AspxErrorPath)
        {
            logger.Debug("404 error generated at \"" + AspxErrorPath + "\"");

            return View();
        }

        public ViewResult Error401(string AspxErrorPath)
        {
            logger.Debug("401 error generated at \"" + AspxErrorPath + "\"");

            return View();
        }
    }
}
