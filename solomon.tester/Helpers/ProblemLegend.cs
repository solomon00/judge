using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Xml;

namespace Solomon.Tester.Helpers
{
    public static class ProblemLegend
    {
        static ProblemLegend()
        {
            //CultureInfo culture = new CultureInfo(ConfigurationManager.AppSettings["DefaultCulture"]);
            //Thread.CurrentThread.CurrentCulture = culture;
            //Thread.CurrentThread.CurrentUICulture = culture;
        }

        public static void Read(string AbsolutePathToProblemDirectory,
            out string Name,
            out double TimeLimit,
            out int MemoryLimit,
            out DateTime LastModifiedTime)
        {
            Name = null;
            TimeLimit = MemoryLimit = -1;
            LastModifiedTime = DateTime.MinValue;

            if (!System.IO.File.Exists(Path.Combine(AbsolutePathToProblemDirectory, "problem.xml")))
            {
                throw new ArgumentException(Path.Combine(AbsolutePathToProblemDirectory, "problem.xml") + " not found");
            }

            CultureInfo culture = new CultureInfo(ConfigurationManager.AppSettings["DefaultCulture"]);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            using (XmlReader reader = XmlReader.Create(
                Path.Combine(AbsolutePathToProblemDirectory, "problem.xml")))
            {
                while (reader.Read())
                {
                    // Only detect start elements.
                    if (reader.IsStartElement())
                    {
                        // Get element name and switch on it.
                        switch (reader.Name)
                        {
                            case "Name":
                                if (reader.Read())
                                {
                                    Name = reader.Value.Trim();
                                }
                                break;
                            case "LastModifiedTime":
                                if (reader.Read())
                                {
                                    DateTime temp;
                                    DateTime.TryParse(reader.Value.Trim(), out temp);
                                    LastModifiedTime = temp;
                                }
                                break;
                            case "TimeLimit":
                                if (reader.Read())
                                {
                                    double temp;
                                    Double.TryParse(reader.Value.Trim(), out temp);
                                    TimeLimit = temp;
                                }
                                break;
                            case "MemoryLimit":
                                if (reader.Read())
                                {
                                    int temp;
                                    Int32.TryParse(reader.Value.Trim(), out temp);
                                    MemoryLimit = temp;
                                }
                                break;
                        }
                    }
                }
            }
        }
    }
}