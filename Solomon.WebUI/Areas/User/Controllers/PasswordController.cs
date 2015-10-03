using log4net;
using log4net.Config;
using Microsoft.Web.WebPages.OAuth;
using Solomon.Domain.Abstract;
using Solomon.Domain.Entities;
using Solomon.TypesExtensions;
using Solomon.WebUI.Areas.User.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace Solomon.WebUI.Areas.User.Controllers
{
    [Authorize]
    public class PasswordController : Controller
    {
        private IRepository repository;
        private readonly ILog logger = LogManager.GetLogger(typeof(PasswordController));

        /// <summary>
        /// Controller constructor.
        /// Initialize repository.
        /// </summary>
        /// <param name="Repository">Repository object</param>
        public PasswordController(IRepository Repository)
        {
            XmlConfigurator.Configure();
            repository = Repository;
        }

        public ActionResult Index()
        {
            //ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            //ViewBag.ReturnUrl = Url.Action("Index");
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(ChangePasswordViewModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            //ViewBag.HasLocalPassword = hasLocalAccount;
            //ViewBag.ReturnUrl = Url.Action("Index");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    int userID = WebSecurity.CurrentUserId;
                    UserProfile user = repository.Users.FirstOrDefault(u => u.UserId == userID);
                    if (user != null && user.CreatedByUser != null)
                        throw new HttpException(401, "Unauthorised");

                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        logger.Info("User " + WebSecurity.GetUserId(User.Identity.Name) +
                                " \"" + User.Identity.Name + "\" successfully change password");
                        TempData["SuccessMessage"] = "Пароль успешно изменен";
                        return Index();
                    }
                    else
                    {
                        logger.Info("Password changing for user " + WebSecurity.GetUserId(User.Identity.Name) +
                            " \"" + User.Identity.Name + "\" failed with error: \"" + "Текущий пароль неверен или новый пароль некорректен." + "\"");
                        ModelState.AddModelError("", "Текущий пароль неверен или новый пароль некорректен");
                    }
                }
            }
            //else
            //{
            //    // User does not have a local password so remove any validation errors caused by a missing
            //    // OldPassword field
            //    ModelState state = ModelState["OldPassword"];
            //    if (state != null)
            //    {
            //        state.Errors.Clear();
            //    }

            //    if (ModelState.IsValid)
            //    {
            //        try
            //        {
            //            WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
            //            //return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
            //        }
            //        catch (Exception e)
            //        {
            //            ModelState.AddModelError("", e);
            //        }
            //    }
            //}

            // If we got this far, something failed, redisplay form
            return View(model);
        }
    }
}
