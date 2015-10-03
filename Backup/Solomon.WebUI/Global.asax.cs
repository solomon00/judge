using log4net;
using log4net.Config;
using Solomon.Domain.Concrete;
using Solomon.WebUI.Controllers;
using Solomon.WebUI.Filters;
using Solomon.WebUI.Infrastructure;
using Solomon.WebUI.Testers;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using WebMatrix.WebData;

namespace Solomon.WebUI
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static int OnlineUsersCount { get; protected set; }
        public static int AuthenticatedUsersCount { get; protected set; }

        private readonly ILog logger = LogManager.GetLogger(typeof(MvcApplication));

        private void InitializeSimpleMembership()
        {
            Database.SetInitializer<EFDbContext>(new MigrateDatabaseToLatestVersion<EFDbContext, Solomon.WebUI.Migrations.Configuration>());

            try
            {
                using (var context = new EFDbContext())
                {
                    context.Database.Initialize(true);
                    if (!context.Database.Exists())
                    {
                        // Create the SimpleMembership database without Entity Framework migration schema
                        //((IObjectContextAdapter)context).ObjectContext.CreateDatabase();

                        
                    }
                }

                WebSecurity.InitializeDatabaseConnection("EFDbContext", "UserProfile", "UserId", "UserName", autoCreateTables: true);

                if (!Roles.RoleExists("Administrator"))
                    Roles.CreateRole("Administrator");

                if (!Roles.RoleExists("Judge"))
                    Roles.CreateRole("Judge");

                if (!Roles.RoleExists("User"))
                    Roles.CreateRole("User");

                if (Membership.GetUser("Admin", false) == null)
                {
                    WebSecurity.CreateUserAndAccount("Admin", "alborov");
                }
                if (!Roles.IsUserInRole("Admin", "Administrator"))
                {
                    Roles.AddUserToRole("Admin", "Administrator");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("The ASP.NET Simple Membership database could not be initialized. For more information, please see http://go.microsoft.com/fwlink/?LinkId=256588", ex);
            }
        }

        //private void _deleteUnconfirmedAccounts()
        //{
        //    WebSecurity.
        //    using (var context = new EFDbContext())
        //    {
        //        foreach (var item in collection)
        //        {
                    
        //        }
        //    }
        //}
        
        public MvcApplication()
        {
            XmlConfigurator.Configure();
        }

        protected void Application_Start()
        {
            try
            {
                AreaRegistration.RegisterAllAreas();

                WebApiConfig.Register(GlobalConfiguration.Configuration);
                FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                RouteConfig.RegisterRoutes(RouteTable.Routes);
                BundleConfig.RegisterBundles(BundleTable.Bundles);
                AuthConfig.RegisterAuth();

                InitializeSimpleMembership();
                GlobalFilters.Filters.Add(new AccessTimingFilter());

                TestersSingleton.Initialize();

                //ControllerBuilder.Current.SetControllerFactory(new NinjectControllerFactory());
                DependencyResolver.SetResolver(new NinjectDependencyResolver());
                
                OnlineUsersCount = 0;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                //foreach (Exception exSub in ((ReflectionTypeLoadException)ex.InnerException).LoaderExceptions)
                //{
                //    sb.AppendLine(exSub.Message);
                //    FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                //    if (exFileNotFound != null)
                //    {
                //        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                //        {
                //            sb.AppendLine("Fusion Log:");
                //            sb.AppendLine(exFileNotFound.FusionLog);
                //        }
                //    }
                //    sb.AppendLine();
                //}
                string errorMessage = sb.ToString();
                logger.Info(errorMessage, ex);
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            if (!HttpContext.Current.IsCustomErrorEnabled)
                return;

            var httpContext = ((MvcApplication)sender).Context;

            var currentRouteData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));
            var currentController = " ";
            var currentAction = " ";

            if (currentRouteData != null)
            {
                if (currentRouteData.Values["controller"] != null && !String.IsNullOrEmpty(currentRouteData.Values["controller"].ToString()))
                {
                    currentController = currentRouteData.Values["controller"].ToString();
                }

                if (currentRouteData.Values["action"] != null && !String.IsNullOrEmpty(currentRouteData.Values["action"].ToString()))
                {
                    currentAction = currentRouteData.Values["action"].ToString();
                }
            }

            var ex = Server.GetLastError();

            var controller = new ErrorsController();
            var routeData = new RouteData();
            var action = "Index";

            if (ex is HttpException)
            {
                var httpEx = ex as HttpException;

                switch (httpEx.GetHttpCode())
                {
                    case 404:
                        action = "Error404";
                        break;

                    // others if any

                    default:
                        action = "Index";
                        break;
                }
            }

            httpContext.ClearError();
            httpContext.Response.Clear();
            httpContext.Response.StatusCode = ex is HttpException ? ((HttpException)ex).GetHttpCode() : 500;
            httpContext.Response.TrySkipIisCustomErrors = true;
            routeData.Values["controller"] = "Errors";
            routeData.Values["action"] = action;

            controller.ViewData.Model = new HandleErrorInfo(ex, currentController, currentAction);
            ((IController)controller).Execute(new RequestContext(new HttpContextWrapper(httpContext), routeData));
        }

        //protected void Session_Start(object sender, EventArgs e)
        //{
        //    // Code that runs when a new session is started
        //    Application.Lock();
        //    OnlineUsersCount++;
        //    Application.UnLock();
        //}

        //protected void Session_End(object sender, EventArgs e)
        //{
        //    // Code that runs when a session ends. 
        //    // Note: The Session_End event is raised only when the sessionstate mode
        //    // is set to InProc in the Web.config file. If session mode is set to StateServer 
        //    // or SQLServer, the event is not raised.
        //    Application.Lock();
        //    OnlineUsersCount--;
        //    Application.UnLock();
        //}
    }
}