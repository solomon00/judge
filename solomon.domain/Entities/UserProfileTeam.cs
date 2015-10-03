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
    public class UserProfileTeam
    {
        public int UserProfileTeamID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }
        public virtual UserProfile User { get; set; }

        [ForeignKey("Team")]
        public int TeamID { get; set; }
        public virtual Team Team { get; set; }

        public int Confirm { get; set; }

        // participant
        [InverseProperty("Teams")]
        public virtual ICollection<Tournament> Tournaments { get; set; }
    }
}
