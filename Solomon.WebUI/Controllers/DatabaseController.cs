using System;
using System.Transactions;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using Solomon.WebUI.Models;
using Solomon.WebUI.Mailers;
using System.Collections.Generic;
using Solomon.Domain.Concrete;
using Solomon.Domain.Entities;
using System.Linq;
using DotNetOpenAuth.AspNet;
using Solomon.Domain.Abstract;
using log4net;
using log4net.Config;
using Solomon.TypesExtensions;
using System.Web;
using System.Data.Entity;
using System.IO;

namespace Solomon.WebUI.Controllers
{
    [Authorize]
    public class DatabaseController : Controller
    {
        private IRepository repository;
        private IUserMailer userMailer;
        private readonly ILog logger = LogManager.GetLogger(typeof(DatabaseController));
        
        /// <summary>
        /// Controller constructor.
        /// Initialize repository.
        /// </summary>
        /// <param name="Repository">Repository object</param>
        public DatabaseController(IRepository Repository, IUserMailer UserMailer)
        {
            XmlConfigurator.Configure();
            repository = Repository;
            userMailer = UserMailer;
        }
        
        /// <summary>
        /// An Ajax method to check if a username is unique.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public JsonResult CheckForUniqueUser(string UserName)
        {
            UserProfile user = repository.Users.FirstOrDefault(u => u.UserName == UserName);
            JsonResponse response = new JsonResponse();
            response.Exists = (user == null) ? false : true;

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// An Ajax method to check if a email is unique.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public JsonResult CheckForUniqueEmail(string Email)
        {
            UserProfile user = repository.Users.Where(u => u.Email == Email).FirstOrDefault();
            JsonResponse response = new JsonResponse();
            response.Exists = (user == null) ? false : true;

            return Json(response, JsonRequestBehavior.AllowGet);
        }
        
        //
        // POST: /Account/Manage

        public JsonResult Country(string term, int limit)
        {
            var country = repository
                .Country
                .Where(c => c.Name.Contains(term))
                .Take(limit)
                .Select(c => new { value = c.CountryID, label = c.Name });

            return this.Json(country, JsonRequestBehavior.AllowGet);
        }
        public JsonResult City(string term, int limit, int countryID = 0)
        {
            var city = repository
                .City
                .Where(c => c.Name.ToLower() == term.ToLower() && c.CountryID == countryID)
                .OrderByDescending(c => c.Important)
                .Take(limit)
                .ToList();

            if (city.Count < limit)
            {
                city.AddRange(repository
                    .City
                    .Where(c => c.Name.ToLower() != term.ToLower() && c.Name.Contains(term) && c.CountryID == countryID)
                    .OrderByDescending(c => c.Important)
                    .Take(limit)
                    );
            }

            var cityConverted = city
                    .Select(c => new
                    {
                        value = c.CityID,
                        label = ((c.Important == 1 ? "<b>" + c.Name + "</b>" : c.Name) +
                            "<br/><span style=\"color: grey;\">" +
                            (!String.IsNullOrEmpty(c.Region) ? c.Region : String.Empty) +
                            (!String.IsNullOrEmpty(c.Area) ? ", " + c.Area : String.Empty) + "</span>")
                    });

            return this.Json(cityConverted, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Institution(string term, int limit, int cityID = 0)
        {
            var institutions = repository
                .Institutions
                .Where(i => i.Name.Contains(term) && i.CityID == cityID)
                .Take(limit)
                .Select(i => new { value = i.InstitutionID, label = i.Name });

            return this.Json(institutions, JsonRequestBehavior.AllowGet);
        }
        
        /// <summary>
        /// An Ajax method to check if a name of team is unique.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult CheckForUniqueTeam(string Name, int TeamID = -1)
        {
            Team team = repository.Teams.FirstOrDefault(t => t.Name == Name && t.TeamID != TeamID);
            JsonResponse response = new JsonResponse();
            response.Exists = (team == null) ? false : true;

            return Json(response, JsonRequestBehavior.AllowGet);
        }

    }
}
