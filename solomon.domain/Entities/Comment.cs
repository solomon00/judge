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
    public class Comment
    {
        [Key]
        public int CommentID { get; set; }

        [ForeignKey("ParentComment")]
        public int? ParentCommentID { get; set; }
        public virtual Comment ParentComment { get; set; }
        public virtual ICollection<Comment> ChildComments { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }
        public virtual UserProfile User { get; set; }

        [ForeignKey("Problem")]
        public int ProblemID { get; set; }
        public virtual Problem Problem { get; set; }

        [ForeignKey("Tournament")]
        public int? TournamentID { get; set; }
        public virtual Tournament Tournament { get; set; }

        public string Value { get; set; }
        public DateTime Date { get; set; }
        public int Public { get; set; }
        public int IsRead { get; set; }
        public int Level { get; set; }

        [ForeignKey("EditedByUser")]
        public int? EditedByUserID { get; set; }
        public virtual UserProfile EditedByUser { get; set; }

        public string OldValue { get; set; }
        public string EditingReason { get; set; }
    }
}
