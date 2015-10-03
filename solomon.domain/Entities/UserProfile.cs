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
    public class UserProfile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        public string UserName { get; set; }
        public string Email { get; set; }

        [DefaultValue(null)]
        public DateTime? LastAccessTime { get; set; }

        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string ThirdName { get; set; }

        [DefaultValue(null)]
        public DateTime? BirthDay { get; set; }

        public string PhoneNumber { get; set; }

        public int? GradeLevel { get; set; } // Year of study

        [ForeignKey("UserCategory")]
        public UserCategories? Category { get; set; }
        public virtual UserCategory UserCategory { get; set; }

        [ForeignKey("Country")]
        public int? CountryID { get; set; }
        public Country Country { get; set; }

        [ForeignKey("City")]
        public int? CityID { get; set; }
        public City City { get; set; }

        [ForeignKey("Institution")]
        public int? InstitutionID { get; set; }
        public Institution Institution { get; set; }

        [ForeignKey("CreatedByUser")]
        public int? CreatedByUserID { get; set; }
        public virtual UserProfile CreatedByUser { get; set; }

        public int? Generated { get; set; }
        public int? SendNotifications { get; set; }

        // member
        public virtual ICollection<UserProfileTeam> Teams { get; set; }

        // participant
        [InverseProperty("Users")]
        public virtual ICollection<Tournament> Tournaments { get; set; }

        [InverseProperty("UsersCanModify")]
        public virtual ICollection<Tournament> CanModifyTournaments { get; set; }

        [InverseProperty("UsersCanModify")]
        public virtual ICollection<Institution> CanModifyInstitutions { get; set; }

        [InverseProperty("UsersCanModify")]
        public virtual ICollection<Problem> CanModifyProblems { get; set; }

        [InverseProperty("SolvedByUsers")]
        public virtual ICollection<Problem> SolvedProblems { get; set; }
        [InverseProperty("NotSolvedByUsers")]
        public virtual ICollection<Problem> NotSolvedProblems { get; set; }
    }
}
