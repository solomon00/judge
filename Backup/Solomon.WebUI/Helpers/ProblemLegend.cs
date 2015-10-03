using Solomon.TypesExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

namespace Solomon.WebUI.Helpers
{
    public class ProblemLegend
    {
        public static void Write(string AbsolutePathToProblemDirectory,
            string Name,
            double TimeLimit,
            int MemoryLimit,
            DateTime LastModifiedTime,
            ProblemTypes PT,
            string Description,
            string InputFormat,
            string OutputFormat)
        {
            if (!Directory.Exists(AbsolutePathToProblemDirectory))
            {
                Directory.CreateDirectory(AbsolutePathToProblemDirectory);
            }

            if (File.Exists(AbsolutePathToProblemDirectory + "/problem.xml"))
                File.Delete(AbsolutePathToProblemDirectory + "/problem.xml");

            using (XmlWriter writer = XmlWriter.Create(
                AbsolutePathToProblemDirectory + "/problem.xml"))
            {
                writer.WriteStartDocument();

                writer.WriteStartElement("Problem");
                writer.WriteElementString("Name", Name);
                writer.WriteElementString("TimeLimit", TimeLimit.ToString());
                writer.WriteElementString("MemoryLimit", MemoryLimit.ToString());
                writer.WriteElementString("PT", ((int)PT).ToString());
                writer.WriteElementString("LastModifiedTime", LastModifiedTime.ToString());
                writer.WriteEndElement();

                writer.WriteEndDocument();
            }

            using (StreamWriter sw = new StreamWriter(
                AbsolutePathToProblemDirectory + "/Description", false))
            {
                if (Description != null)
                {
                    sw.WriteLine(Description.Trim());
                }
            }
            using (StreamWriter sw = new StreamWriter(
                AbsolutePathToProblemDirectory + "/InputFormat", false))
            {
                if (InputFormat != null)
                {
                    sw.WriteLine(InputFormat.Trim());
                }
            }
            using (StreamWriter sw = new StreamWriter(
                AbsolutePathToProblemDirectory + "/OutputFormat", false))
            {
                if (OutputFormat != null)
                {
                    sw.WriteLine(OutputFormat.Trim());
                }
            }
        }

        public static void Read(string AbsolutePathToProblemDirectory,
            out string Name,
            out double TimeLimit,
            out int MemoryLimit,
            out ProblemTypes PT,
            out string Description,
            out string InputFormat,
            out string OutputFormat)
        {
            Name = Description = InputFormat = OutputFormat = null;
            TimeLimit = MemoryLimit = -1;
            PT = ProblemTypes.Standart;
            DateTime LastModifiedTime = DateTime.MinValue; // not used

            if (!System.IO.File.Exists(AbsolutePathToProblemDirectory + "/problem.xml"))
            {
                throw new ArgumentException(AbsolutePathToProblemDirectory + "/problem.xml not found");
            }
            if (!System.IO.File.Exists(AbsolutePathToProblemDirectory + "/Description"))
            {
                throw new ArgumentException(AbsolutePathToProblemDirectory + "/Description not found");
            }
            if (!System.IO.File.Exists(AbsolutePathToProblemDirectory + "/InputFormat"))
            {
                throw new ArgumentException(AbsolutePathToProblemDirectory + "/InputFormat not found");
            }
            if (!System.IO.File.Exists(AbsolutePathToProblemDirectory + "/OutputFormat"))
            {
                throw new ArgumentException(AbsolutePathToProblemDirectory + "/OutputFormat not found");
            }

            using (XmlReader reader = XmlReader.Create(
                AbsolutePathToProblemDirectory + "/problem.xml"))
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
                            case "PT":
                                if (reader.Read())
                                {
                                    int temp;
                                    Int32.TryParse(reader.Value.Trim(), out temp);
                                    PT = (ProblemTypes)temp;
                                }
                                break;
                        }
                    }
                }
            }

            using (StreamReader sr = new StreamReader(AbsolutePathToProblemDirectory + "/Description"))
            {
                Description = sr.ReadToEnd();
            }
            using (StreamReader sr = new StreamReader(AbsolutePathToProblemDirectory + "/InputFormat"))
            {
                InputFormat = sr.ReadToEnd();
            }
            using (StreamReader sr = new StreamReader(AbsolutePathToProblemDirectory + "/OutputFormat"))
            {
                OutputFormat = sr.ReadToEnd();
            }
        }
    }
}