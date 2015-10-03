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
using Solomon.WebUI.Helpers;

namespace Solomon.WebUI.Areas.TestersManagement.Controllers
{
    [Authorize(Roles = "Administrator")]
    public partial class CompilersController : Controller
    {
        private IRepository repository;
        private TestersSingleton testers;
        private readonly ILog logger = LogManager.GetLogger(typeof(TesterController));

        /// <summary>
        /// Controller constructor.
        /// </summary>
        /// <param name="Repository">Repository object</param>
        public CompilersController(IRepository Repository)
        {
            XmlConfigurator.Configure();
            testers = TestersSingleton.Instance;
            repository = Repository;
        }

        #region Index Method
        [HttpGet]
        public ActionResult Index()
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited TestersManagement/Compilers/Index");

            ManageCompilersViewModel viewModel = new ManageCompilersViewModel();

            viewModel.Compilers = new List<Compiler>();
            repository.ProgrammingLanguages.Each(c =>
            {
                if (c.ProgrammingLanguageID != ProgrammingLanguages.Open)
                {
                    viewModel.Compilers.Add(new Compiler()
                    {
                        CompilerID = c.ProgrammingLanguageID,
                        Name = c.Title,
                        Command = Compilers.GetCommand(c.ProgrammingLanguageID),
                        Options = Compilers.GetOptions(c.ProgrammingLanguageID),
                        Available = c.Available,
                        Enable = c.Enable
                    });
                }
            });

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Index(IEnumerable<Compiler> Compilers)
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) +
                " \"" + User.Identity.Name + "\" visited TestersManagement/Compilers/Index");

            Dictionary<ProgrammingLanguages, string> command = new Dictionary<ProgrammingLanguages, string>();
            Dictionary<ProgrammingLanguages, string> options = new Dictionary<ProgrammingLanguages,string>();

            foreach (var c in Compilers)
            {
                repository.SetProgrammingLanguageName(c.CompilerID, c.Name);
                if (c.Enable)
                {
                    repository.EnableProgrammingLanguage(c.CompilerID);
                }
                else
                {
                    repository.DisableProgrammingLanguage(c.CompilerID);
                }

                command.Add(c.CompilerID, c.Command);
                options.Add(c.CompilerID, c.Options);
            }

            Solomon.WebUI.Helpers.Compilers.SaveOptions(LocalPath.AbsoluteCompilerOptionsDirectory, command, options);

            TestersSingleton.Instance.SendCompilerOptions();

            return RedirectToAction("Index");
        }

        #endregion

    }
}
