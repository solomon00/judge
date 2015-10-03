using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Solomon.WebUI.Areas.ProblemsManagement.ViewModels
{
    public class HomeViewModel
    {
        public string TotalProblemCount { get; set; }
        public string StandartProblemCount { get; set; }
        public string InteractiveProblemCount { get; set; }
        public string OpenProblemCount { get; set; }
    }
}
