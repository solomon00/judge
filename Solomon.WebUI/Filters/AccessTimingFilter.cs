using System.Web.Mvc;
using log4net;
using System.Web;
using System.Diagnostics;
using System;
using log4net.Config;
using Solomon.Domain.Abstract;
using WebMatrix.WebData;
using System.Linq;
using Solomon.TypesExtensions;
using Solomon.Domain.Concrete;

namespace Solomon.WebUI.Filters
{
    public class AccessTimingFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
        }
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (!(filterContext.Result is PartialViewResult) && !(filterContext.Result is JsonResult))
            {
                var repository = new EFRepository();
                var user = repository.Users.FirstOrDefault(u => u.UserId == WebSecurity.CurrentUserId);
                if (user != null)
                {
                    user.LastAccessTime = DateTime.Now.Truncate(TimeSpan.FromSeconds(1));
                    repository.UpdateUserProfile(user);
                }
            }
        }
    }
}
