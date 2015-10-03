using Solomon.TypesExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

namespace Solomon.WebUI.Helpers
{
    public static class Compilers
    {
        static Compilers()
        {
            LoadOptions(LocalPath.AbsoluteCompilerOptionsDirectory);
        }

        private class ProgrammingLanguage
        {
            public ProgrammingLanguages PL { get; set; }
            public int Count { get; set; }

            public ProgrammingLanguage(ProgrammingLanguages PL, int Count)
            {
                this.PL = PL;
                this.Count = Count;
            }
        }
        private static object lockObj = new object();
        private static object lockFile = new object();
        private static List<ProgrammingLanguage> availableLanguages = new List<ProgrammingLanguage>();
        private static Dictionary<ProgrammingLanguages, string> command = new Dictionary<ProgrammingLanguages, string>();
        private static Dictionary<ProgrammingLanguages, string> options = new Dictionary<ProgrammingLanguages, string>();

        public static IEnumerable<ProgrammingLanguages> AvailableLanguages 
        { 
            get 
            {
                return availableLanguages.Select(l => l.PL);
            }
        }

        public static void AddLanguage(ProgrammingLanguages PL)
        {
            lock (lockObj)
            {
                if (!AvailableLanguages.Contains(PL))
                {
                    availableLanguages.Add(new ProgrammingLanguage(PL, 1));
                }
                else
                {
                    availableLanguages.First(l => l.PL == PL).Count++;
                }
            }
        }

        public static void RemoveLanguage(ProgrammingLanguages PL)
        {
            lock (lockObj)
            {
                if (AvailableLanguages.Contains(PL))
                {
                    ProgrammingLanguage pl = availableLanguages.First(l => l.PL == PL);
                    pl.Count--;
                    if (pl.Count == 0)
                    {
                        availableLanguages.Remove(pl);
                    }
                }
            }
        }


        public static void LoadOptions(string AbsolutePathToCompilerOptionsDirectory)
        {
            if (!System.IO.File.Exists(Path.Combine(AbsolutePathToCompilerOptionsDirectory, "options.xml")))
            {
                throw new ArgumentException(Path.Combine(AbsolutePathToCompilerOptionsDirectory, "options.xml") + " not found");
            }

            command.Clear();
            options.Clear();

            lock (lockFile)
            {
                using (XmlReader reader = XmlReader.Create(
                    Path.Combine(AbsolutePathToCompilerOptionsDirectory, "options.xml")))
                {
                    while (reader.Read())
                    {
                        // Only detect start elements.
                        if (reader.IsStartElement())
                        {
                            foreach (ProgrammingLanguages pl in (ProgrammingLanguages[])Enum.GetValues(typeof(ProgrammingLanguages)))
                            {
                                if (reader.Name == pl.ToString() && !reader.IsEmptyElement && reader.Read())
                                {
                                    if (reader.Name == "Command" && !reader.IsEmptyElement && reader.Read())
                                    {
                                        lock (lockObj)
                                        {
                                            if (command.ContainsKey(pl))
                                                command.Remove(pl);
                                            command.Add(pl, reader.Value.Trim());
                                            reader.Read();  // read end element
                                            reader.Read();  // read next element
                                        }
                                    }
                                    if (reader.Name == "Options" && !reader.IsEmptyElement && reader.Read())
                                    {
                                        lock (lockObj)
                                        {
                                            if (options.ContainsKey(pl))
                                                options.Remove(pl);
                                            options.Add(pl, reader.Value.Trim());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void SaveOptions(string AbsolutePathToCompilerOptionsDirectory,
            Dictionary<ProgrammingLanguages, string> Command,
            Dictionary<ProgrammingLanguages, string> Options)
        {
            if (!Directory.Exists(AbsolutePathToCompilerOptionsDirectory))
            {
                Directory.CreateDirectory(AbsolutePathToCompilerOptionsDirectory);
            }

            if (File.Exists(Path.Combine(AbsolutePathToCompilerOptionsDirectory, "options.xml")))
                File.Delete(Path.Combine(AbsolutePathToCompilerOptionsDirectory, "options.xml"));

            using (XmlWriter writer = XmlWriter.Create(
                Path.Combine(AbsolutePathToCompilerOptionsDirectory, "options.xml")))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Options");

                foreach (ProgrammingLanguages pl in (ProgrammingLanguages[])Enum.GetValues(typeof(ProgrammingLanguages)))
                {
                    if (pl != ProgrammingLanguages.Open)
                    {
                        writer.WriteStartElement(pl.ToString());

                        writer.WriteElementString("Command", Command[pl]);
                        writer.WriteElementString("Options", Options[pl]);

                        writer.WriteEndElement();

                        command[pl] = Command[pl];
                        options[pl] = Options[pl];
                    }
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        public static string GetOptions(ProgrammingLanguages PL)
        {
            lock (lockObj)
            {
                return options.ContainsKey(PL) ? options[PL] : String.Empty;
            }
        }

        public static string GetCommand(ProgrammingLanguages PL)
        {
            lock (lockObj)
            {
                return command.ContainsKey(PL) ? command[PL] : String.Empty;
            }
        }
    }
}