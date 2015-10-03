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
    public class Tournament
    {
        public int TournamentID { get; set; }
        public string Name { get; set; }

        [ForeignKey("TournamentFormat")]
        public TournamentFormats Format { get; set; }
        public virtual TournamentFormat TournamentFormat { get; set; }

        [ForeignKey("TournamentType")]
        public TournamentTypes Type { get; set; }
        public virtual TournamentType TournamentType { get; set; }

        public bool ShowResultsToAll { get; set; }

        public bool ShowSolutionSendingTime { get; set; }
        public bool ShowTimer { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public virtual ICollection<Problem> Problems { get; set; }

        // Available programming languages
        public virtual ICollection<ProgrammingLanguage> AvailablePL { get; set; }

        // participant user
        [InverseProperty("Tournaments")]
        public virtual ICollection<UserProfile> Users { get; set; }

        // participant in team
        [InverseProperty("Tournaments")]
        public virtual ICollection<UserProfileTeam> Teams { get; set; }
        
        [InverseProperty("CanModifyTournaments")]
        public virtual ICollection<UserProfile> UsersCanModify { get; set; }
    }
}
