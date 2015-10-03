using System.Collections.Generic;
using System.Web.Mvc;

namespace Solomon.WebUI.Areas.TestersManagement.ViewModels
{
    public class JsonResponseSolutionsData
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool Reload { get; set; }
        public string HtmlTable { get; set; }
    }

    public class JsonResponseSolutionResults
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Html { get; set; }
    }

    public class SolutionsViewModel
    {
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
    }
}