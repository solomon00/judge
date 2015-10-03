using System.Collections.Generic;
using System.Web.Mvc;

namespace Solomon.WebUI.Areas.TestersManagement.ViewModels
{
    public class JsonResponseTesters
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<ShowTesterViewModel> Testers { get; set; }

        public JsonResponseTesters()
        {
            Testers = new List<ShowTesterViewModel>();
        }
    }
}