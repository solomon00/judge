using log4net;
using log4net.Config;
using Solomon.WebUI.Areas.AccountsManagement.ViewModels;
using Solomon.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;

namespace Solomon.WebUI.Areas.AccountsManagement.Controllers
{
    [Authorize(Roles="Administrator")]
    public partial class RoleController : Controller
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(RoleController));

        public RoleController()
        {
            XmlConfigurator.Configure();
        }

        public ActionResult Index()
        {
            ManageRolesViewModel model = new ManageRolesViewModel();
            model.Roles = new SelectList(Roles.GetAllRoles());
            model.RoleList = Roles.GetAllRoles();

            return View(model);
        }

        #region Create Roles Methods

        [HttpGet]
        public ActionResult CreateRole()
        {
            logger.Debug("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" visited AccountsManagement/Role/CreateRole");

            return View(new RoleViewModel());
        }

        [HttpPost]
        public ActionResult CreateRole(string roleName)
        {
            JsonResponse response = new JsonResponse();

            if (string.IsNullOrEmpty(roleName))
            {
                response.Success = false;
                response.Message = "Пожалуйста, введите имя роли.";
                response.CssClass = "red";

                return Json(response);
            }

            try
            {
                Roles.CreateRole(roleName);

                if (Request.IsAjaxRequest())
                {
                    response.Success = true;
                    response.Message = "Роль успешно создана!";
                    response.CssClass = "green";

                    logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                        " \"" + User.Identity.Name + "\" created role \"" + roleName + "\"");

                    return Json(response);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                {
                    response.Success = false;
                    response.Message = "Произошла ошибка при создании роли";
                    response.CssClass = "red";

                    logger.Warn("Error occurred on user " + WebSecurity.GetUserId(User.Identity.Name) + 
                        " \"" + User.Identity.Name + "\" creating role \"" + roleName + "\": ", ex);

                    return Json(response);
                }

                ModelState.AddModelError("", "Произошла ошибка при создании роли");
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Delete Roles Methods

        /// <summary>
        /// This is an Ajax method.
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteRole(string roleName)
        {
            JsonResponse response = new JsonResponse();

            if (string.IsNullOrEmpty(roleName))
            {
                response.Success = false;
                response.Message = "Пожалуйста, выберите роль для удаления.";
                response.CssClass = "red";

                return Json(response);
            }

            Roles.DeleteRole(roleName);

            response.Success = true;
            response.Message = "Роль \"" + roleName + "\" успешно удалена!";
            response.CssClass = "green";

            logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                " \"" + User.Identity.Name + "\" delete role \"" + roleName + "\"");

            return Json(response);
        }

        [HttpPost]
        public ActionResult DeleteRoles(string roles, bool throwOnPopulatedRole)
        {
            JsonResponse response = new JsonResponse();
            response.Messages = new List<ResponseItem>();

            if (string.IsNullOrEmpty(roles))
            {
                response.Success = false;
                response.Message = "Пожалуйста, выберите роли.";
                return Json(response);
            }

            string[] roleNames = roles.Split(',');
            StringBuilder sb = new StringBuilder();

            ResponseItem item = null;

            foreach (var role in roleNames)
            {
                if (!string.IsNullOrEmpty(role))
                {
                    try
                    {
                        Roles.DeleteRole(role, throwOnPopulatedRole);

                        item = new ResponseItem();
                        item.Success = true;
                        item.Message = "Роль \"" + role + "\" успешно удалена";
                        item.CssClass = "green";
                        response.Messages.Add(item);

                        sb.AppendLine("Роль \"" + role + "\" успешно удалена <br />");

                        logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                            " \"" + User.Identity.Name + "\" delete role \"" + role + "\"");
                    }
                    catch (System.Configuration.Provider.ProviderException)
                    {
                        sb.AppendLine("Произошла ошибка при удалении роли \"" + role + "\" <br />");

                        item = new ResponseItem();
                        item.Success = false;
                        item.Message = "Произошла ошибка при удалении роли \"" + role + "\"";
                        item.CssClass = "yellow";
                        response.Messages.Add(item);

                        logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) + 
                            " \"" + User.Identity.Name + "\" delete role \"" + role + "\"");
                    }
                }
            }

            response.Success = true;
            response.Message = sb.ToString();

            return Json(response);
        }

        #endregion

        #region Get Users In Role methods

        /// <summary>
        /// This is an Ajax method that populates the 
        /// Roles drop down list.
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAllRoles()
        {
            var list = Roles.GetAllRoles();

            List<SelectObject> selectList = new List<SelectObject>();

            foreach (var item in list)
            {
                selectList.Add(new SelectObject() { caption = item, value = item });
            }

            return Json(selectList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetUsersInRole(string roleName)
        {
            var list = Roles.GetUsersInRole(roleName);

            return Json(list, JsonRequestBehavior.AllowGet);
        }


        #endregion
    }
}
