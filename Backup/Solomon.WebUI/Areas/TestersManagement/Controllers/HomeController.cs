using Solomon.Domain.Abstract;
using System.Web.Mvc;
using System.Linq;
using Solomon.Domain.Entities;
using System.Web.Security;
using System;
using Solomon.WebUI.Areas.TestersManagement.ViewModels;
using Solomon.WebUI.Testers;
using log4net;
using log4net.Config;
using WebMatrix.WebData;
using Solomon.WebUI.Helpers;
using System.IO;

namespace Solomon.WebUI.Areas.TestersManagement.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class HomeController : Controller
    {
        private TestersSingleton testers;
        private readonly ILog logger = LogManager.GetLogger(typeof(HomeController));

        /// <summary>
        /// Controller constructor.
        /// </summary>
        public HomeController()
        {
            XmlConfigurator.Configure();
            testers = TestersSingleton.Instance;
        }

        public ActionResult Index()
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited TestersManagement/Home/Index");

            HomeViewModel viewModel = new HomeViewModel() { 
                TotalTestersCount = testers.Count,
                HostName = testers.HostName,
                LocalAddress = testers.LocalAddress,
                PortListen = testers.PortListen
            };
            
            return View(viewModel);
        }

        [HttpGet]
        public FilePathResult GetLogFile(string Date, string LogBy = null)
        {
            string path = Directory.GetFiles(LocalPath.AbsoluteLogFileDirectory[0])[0];
            
            foreach (var dir in LocalPath.AbsoluteLogFileDirectory)
            {
                foreach (var item in Directory.GetFiles(dir))
                {
                    if (Path.GetFileNameWithoutExtension(item) == Date)
                    {
                        path = item;
                        break;
                    }
                }
            }
            Response.AppendHeader("Content-Disposition", "attachment; filename=" + Path.GetFileName(path));

            return new FilePathResult(path, "text/plain");
        }
    }
}
