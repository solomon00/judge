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

namespace Solomon.WebUI.Areas.User.ViewModels
{
    public class NewTeamViewModel : UserViewModel
    {
        [Display(Name = "Название команды")]
        [Required(ErrorMessage = "Обязательное поле")]
        public string Name { get; set; }
    }

    public class ManageTeamViewModel : UserViewModel
    {
        public int TeamID { get; set; }

        [Display(Name = "Название команды")]
        [Required(ErrorMessage = "Обязательное поле")]
        public string Name { get; set; }

        public IEnumerable<UserProfileTeam> Members { get; set; }
    }

    public class TeamViewModel : UserViewModel
    {
        public IEnumerable<Team> Teams { get; set; }
    }

    public class InvitesViewModel : UserViewModel
    {
        public IEnumerable<Team> Invites { get; set; }
    }

    public class JsonResponseMembersData
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string HtmlTable { get; set; }
    }
}
