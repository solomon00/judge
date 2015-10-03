using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Solomon.TypesExtensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// dateTime = dateTime.Truncate(TimeSpan.FromMilliseconds(1)); // Truncate to whole ms
        /// dateTime = dateTime.Truncate(TimeSpan.FromSeconds(1)); // Truncate to whole second
        /// dateTime = dateTime.Truncate(TimeSpan.FromMinutes(1)); // Truncate to whole minute
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
        {
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }
    }
}
