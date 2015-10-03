using Solomon.Domain.Entities;
using Solomon.TypesExtensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Security;

namespace Solomon.WebUI.Areas.TestersManagement.ViewModels
{
    public class ManageTestersViewModel
    {
        public List<ShowTesterViewModel> Testers { get; set; }

        public ManageTestersViewModel()
        {
            Testers = new List<ShowTesterViewModel>();
        }
    }
}
