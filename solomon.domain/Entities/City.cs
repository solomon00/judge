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
    public class City
    {
        public int CityID { get; set; }

        [ForeignKey("Country")]
        public int CountryID { get; set; }
        public virtual Country Country { get; set; }

        public string Region { get; set; }
        public string Area { get; set; }
        public string Name { get; set; }

        [DefaultValue(0)]
        public int Important { get; set; }
    }
}
