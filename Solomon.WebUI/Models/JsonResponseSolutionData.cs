using System.Collections.Generic;
using System.Web.Mvc;

namespace Solomon.WebUI.Models
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
}