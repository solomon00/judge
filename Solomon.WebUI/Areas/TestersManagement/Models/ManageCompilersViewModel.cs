using Solomon.Domain.Abstract;
using Solomon.Domain.Entities;
using Solomon.TypesExtensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Security;

namespace Solomon.WebUI.Areas.TestersManagement.ViewModels
{
    public class Compiler
    {
        public ProgrammingLanguages CompilerID { get; set; }
        public string Name { get; set; }
        public string Command { get; set; }
        public string Options { get; set; }
        public bool Available { get; set; }
        public bool Enable { get; set; }
    }

    public class ManageCompilersViewModel
    {
        public List<Compiler> Compilers { get; set; }
    }
}
