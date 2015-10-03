using Solomon.TypesExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace Solomon.Domain.Entities
{
    public class Solution
    {
        public int SolutionID { get; set; }

        [ForeignKey("Tournament")]
        public int TournamentID { get; set; }
        [ForeignKey("Problem")]
        public int ProblemID { get; set; }
        [ForeignKey("User")]
        public int UserID { get; set; }

        public virtual Tournament Tournament { get; set; }
        public virtual Problem Problem { get; set; }
        public virtual UserProfile User { get; set; }

        [ForeignKey("PL")]
        public ProgrammingLanguages ProgrammingLanguage { get; set; }
        public virtual ProgrammingLanguage PL { get; set; }
        
        public DateTime SendTime { get; set; }
        public string FileName { get; set; }
        public string DataType { get; set; }

        [ForeignKey("TestResult")]
        [DefaultValue(Solomon.TypesExtensions.TestResults.Waiting)]
        public TestResults Result { get; set; }
        public virtual TestResult TestResult { get; set; }

        [DefaultValue(0)]
        public int Score { get; set; }

        [DefaultValue(0)]
        public int ErrorOnTest { get; set; }

        public virtual ICollection<SolutionTestResult> TestResults { get; set; }

        public string Path { get; set; }
    }
}
