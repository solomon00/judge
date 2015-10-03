using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Web.Mvc;
using WebMatrix.WebData;
using Solomon.WebUI.Models;
using Solomon.Domain.Concrete;
using System.Web.Security;

namespace Solomon.WebUI.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class InitializeSimpleMembershipAttribute : ActionFilterAttribute
    {
        private static SimpleMembershipInitializer _initializer;
        private static object _initializerLock = new object();
        private static bool _isInitialized;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Ensure ASP.NET Simple Membership is initialized only once per app start
            LazyInitializer.EnsureInitialized(ref _initializer, ref _isInitialized, ref _initializerLock);
        }

        private class SimpleMembershipInitializer
        {
            public SimpleMembershipInitializer()
            {
                Database.SetInitializer<EFDbContext>(null);

                try
                {
                    using (var context = new EFDbContext())
                    {
                        if (!context.Database.Exists())
                        {
                            // Create the SimpleMembership database without Entity Framework migration schema
                            ((IObjectContextAdapter)context).ObjectContext.CreateDatabase();
                        }
                    }

                    WebSecurity.InitializeDatabaseConnection("EFDbContext", "UserProfile", "UserId", "UserName", autoCreateTables: true);

                    if (!Roles.RoleExists("Administrator"))
                        Roles.CreateRole("Administrator");

                    if (Membership.GetUser("Admin", false) == null)
                    {
                        WebSecurity.CreateUserAndAccount("Admin", "alborov");
                    }
                    if (!Roles.IsUserInRole("Admin", "Administrator"))
                    {
                        Roles.AddUserToRole("Admin", "Administrator");
                    }

                    if (!Roles.RoleExists("User"))
                        Roles.CreateRole("User");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("The ASP.NET Simple Membership database could not be initialized. For more information, please see http://go.microsoft.com/fwlink/?LinkId=256588", ex);
                }
            }
        }
    }
}
