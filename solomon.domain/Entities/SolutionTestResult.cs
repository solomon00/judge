using Solomon.TypesExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solomon.Domain.Entities
{
    public class SolutionTestResult
    {
        public int SolutionTestResultID { get; set; }

        [ForeignKey("Solution")]
        public int SolutionID { get; set; }
        public virtual Solution Solution { get; set; }

        public Int64 Time { get; set; }
        public Int64 Memory { get; set; }

        [ForeignKey("TestResult")]
        public TestResults Result { get; set; }
        public virtual TestResult TestResult { get; set; }
    }
}
