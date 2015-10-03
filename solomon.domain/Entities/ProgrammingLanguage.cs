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
    public class ProgrammingLanguage
    {
        public ProgrammingLanguages ProgrammingLanguageID { get; set; }

        // Language enable for solution
        public bool Enable { get; set; }

        // Language available on tester
        public bool Available { get; set; }

        public string Title { get; set; }

        public virtual ICollection<Tournament> Tournaments { get; set; }
    }
}
