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
    public class Institution
    {
        public int InstitutionID { get; set; }
        public string Name { get; set; }

        [ForeignKey("City")]
        public int CityID { get; set; }
        public virtual City City { get; set; }

        [InverseProperty("CanModifyInstitutions")]
        public virtual ICollection<UserProfile> UsersCanModify { get; set; }
    }
}
