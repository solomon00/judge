using Solomon.Domain.Entities;
using Solomon.TypesExtensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Security;

namespace Solomon.WebUI.Areas.TestersManagement.ViewModels
{
    public class ShowTesterViewModel
    {
        public String Address { get; set; }
        public Int32 CPULoad { get; set; }
        public Boolean IsConnected { get; set; }
        public Int32 ProcessorsCount { get; set; }
        public Int32 CheckingSolutionsCount { get; set; }
        public List<ProgrammingLanguages> Compilers { get; set; }

        public PaginatedList<ProblemInfo> PaginatedProblemList { get; set; }
        public string FilterBy { get; set; }
        public string SearchTerm { get; set; }
        public int PageSize { get; set; }
    }
}
