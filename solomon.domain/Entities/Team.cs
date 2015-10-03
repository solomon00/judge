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
    public class Team
    {
        public int TeamID { get; set; }
        public string Name { get; set; }

        public virtual ICollection<UserProfileTeam> Members { get; set; }
    }
}
