using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Web.Mvc;
using WebMatrix.WebData;
using Solomon.WebUI.Models;
using Solomon.Domain.Concrete;
using System.Web.Security;
using log4net;
using System.Web;

namespace Solomon.WebUI.Filters
{
    public class CustomHandleErrorAttribute : HandleErrorAttribute
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(CustomHandleErrorAttribute));

        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled || !filterContext.HttpContext.IsCustomErrorEnabled)
            {
                return;
            }

            if (new HttpException(null, filterContext.Exception).GetHttpCode() != 500)
            {
                return;
            }

            if (!ExceptionType.IsInstanceOfType(filterContext.Exception))
            {
                return;
            }

            // if the request is AJAX return JSON else view.
            if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                filterContext.Result = new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        error = true,
                        message = filterContext.Exception.Message
                    }
                };
            }
            else
            {
                var controllerName = (string)filterContext.RouteData.Values["controller"];
                var actionName = (string)filterContext.RouteData.Values["action"];
                var model = new HandleErrorInfo(filterContext.Exception, controllerName, actionName);

                filterContext.Result = new ViewResult
                {
                    ViewName = View,
                    MasterName = Master,
                    ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
                    TempData = filterContext.Controller.TempData
                };
            }

            // log the error
            
            string errMessage = "";
            foreach (var item in filterContext.RouteData.Values)
            {
                errMessage += "\r\n" + item.Key + " = " + item.Value;
            }

            logger.Error(filterContext.Exception.Message + errMessage, filterContext.Exception);

            filterContext.ExceptionHandled = false;
            //filterContext.HttpContext.Response.Clear();
            //filterContext.HttpContext.Response.StatusCode = 500;

            //filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }
    }
}
