using Solomon.TypesExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solomon.Domain.Entities
{
    public class Problem
    {
        public int ProblemID { get; set; }
        public string Name { get; set; }

        [ForeignKey("ProblemType")]
        public ProblemTypes Type { get; set; }
        public virtual ProblemType ProblemType { get; set; }

        public virtual ICollection<ProblemTag> Tags { get; set; }

        public double TimeLimit { get; set; }
        public int MemoryLimit { get; set; }
        public string Path { get; set; }
        public DateTime LastModifiedTime { get; set; }

        [DefaultValue(false)]
        public bool CheckPending { get; set; }

        public virtual ICollection<Tournament> Tournaments { get; set; }

        [InverseProperty("CanModifyProblems")]
        public virtual ICollection<UserProfile> UsersCanModify { get; set; }

        [InverseProperty("SolvedProblems")]
        public virtual ICollection<UserProfile> SolvedByUsers { get; set; }
        [InverseProperty("NotSolvedProblems")]
        public virtual ICollection<UserProfile> NotSolvedByUsers { get; set; }
    }
}
