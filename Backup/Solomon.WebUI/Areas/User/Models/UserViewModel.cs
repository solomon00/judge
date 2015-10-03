using Solomon.Domain.Abstract;
using Solomon.Domain.Entities;
using Solomon.TypesExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;
using System.Linq;
using Ninject;
using Solomon.WebUI.Infrastructure;

namespace Solomon.WebUI.Areas.User.ViewModels
{
    public class UserViewModel
    {
        public bool CanChangePassword { get; set; }

        public UserViewModel()
        {
            StandardKernel kernel = new StandardKernel(new FullNinjectModule());
            var repository = kernel.Get<IRepository>();

            int userID = WebSecurity.CurrentUserId;
            UserProfile user = repository.Users.FirstOrDefault(u => u.UserId == userID);

            CanChangePassword = user.Generated != 1;
        }
    }

}
