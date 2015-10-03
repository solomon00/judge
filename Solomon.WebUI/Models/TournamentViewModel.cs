using Solomon.Domain.Entities;
using Solomon.TypesExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Solomon.WebUI.ViewModels
{
    public class ParticipantSolutionResult
    {
        public int ProblemID { get; set; }
        public int ID { get; set; }
        public bool Online { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public IEnumerable<UserProfile> Users { get; set; } // many if team
        public DateTime SendTime { get; set; }
        public TestResults Result { get; set; }
        public int Score { get; set; }
    }

    public class ParticipantProblemResult
    {
        public int ProblemID { get; set; }
        public bool Accept { get; set; }
        public int Score { get; set; }
        public TimeSpan AcceptTime { get; set; }
        public int Penalties { get; set; }
    }

    public class ParticipantTournamentResult
    {
        public string Place { get; set; }
        public int ID { get; set; }
        public bool Online { get; set; }
        public string Name { get; set; }                    // user name or team name
        public string FullName { get; set; }                // participiant name(s)
        public IEnumerable<UserProfile> Users { get; set; } // many if team
        public IEnumerable<ParticipantProblemResult> ProblemsResults { get; set; }
        public int TotalAccepted { get; set; }
        public int TotalPenalties { get; set; }
        public int TotalScore { get; set; }
    }

    public class TournamentResultsViewModel
    {
        public int TournamentID { get; set; }
        public string TournamentName { get; set; }
        public DateTime TournamentStartDate { get; set; }
        public DateTime TournamentEndDate { get; set; }
        public DateTime CurrentTime { get; set; }
        public TournamentFormats TF { get; set; }

        public bool ShowSolutionSendingTime { get; set; }
        public bool ShowTimer { get; set; }

        public IEnumerable<Problem> Problems { get; set; }
        public IEnumerable<ParticipantTournamentResult> TournamentResults { get; set; }

        public bool CanExportResults { get; set; }
    }

    #region Statistic
    public class TournamentStatisticViewModel
    {
        public int TournamentID { get; set; }
        public string TournamentName { get; set; }
    }

    // Structures for hightchart
    public class SolutionsData
    {
        public string name { get; set; }
        public int y { get; set; }
        public string drilldown { get; set; }
    }
    public class Drilldown
    {
        public string name { get; set; }
        public string id { get; set; }
        public IEnumerable<dynamic> data { get; set; }
    }
    public class JsonSolutions
    {
        public List<SolutionsData> SolutionsData { get; set; }
        public List<Drilldown> Drilldown { get; set; }

        public JsonSolutions()
        {
            SolutionsData = new List<SolutionsData>();
            Drilldown = new List<Drilldown>();
        }
    }
    #endregion
}