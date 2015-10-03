using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Solomon.WebUI.Areas.TestersManagement.ViewModels
{
    public class HomeViewModel
    {
        public Int32 TotalTestersCount { get; set; }
        public String HostName { get; set; }
        public IPAddress[] LocalAddress { get; set; }
        public int PortListen { get; set; }
    }
}
