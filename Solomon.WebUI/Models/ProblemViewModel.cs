using Solomon.Domain.Entities;
using Solomon.TypesExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Solomon.WebUI.ViewModels
{
    public class ProblemViewModel
    {
        public bool IsUserRegisterForTournament { get; set; }

        public ProblemTypes PT { get; set; }
        public int ProblemID { get; set; }
        public int TournamentID { get; set; }
        public string TournamentName { get; set; }
        public TournamentFormats TF { get; set; }
        public DateTime TournamentStartDate { get; set; }
        public DateTime TournamentEndDate { get; set; }
        public DateTime CurrentTime { get; set; }
        public bool ShowTimer { get; set; }

        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
    }

    public class ProblemContentViewModel
    {
        public string Name { get; set; }
        public ProblemTypes PT { get; set; }
        public double TimeLimit { get; set; }
        public int MemoryLimit { get; set; }
        public string Description { get; set; }
        public string InputFormat { get; set; }
        public string OutputFormat { get; set; }
        public bool TestSamplesComments { get; set; }
        public IEnumerable<Tuple<string, string, string>> TestSamples { get; set; }

        public int ProblemID { get; set; }
    }

    public class ProblemCommentsCount
    {
        public int ProblemID { get; set; }
        public int CommentsCount { get; set; }
    }

    public class NewCommentsJsonResponse
    {
        public IEnumerable<ProblemCommentsCount> NewComments { get; set; }
        public int TotalCount { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}