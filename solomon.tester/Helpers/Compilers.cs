using Solomon.TypesExtensions;
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
    public static class Compilers
    {
        static bool ExistsOnPath(string FileName)
        {
            if (GetFullPath(FileName) != null)
                return true;
            return false;
        }
        static string GetFullPath(string FileName)
        {
            if (File.Exists(FileName))
                return Path.GetFullPath(FileName);

            var values = Environment.GetEnvironmentVariable("PATH");
            foreach (var path in values.Split(';'))
            {
                var fullPath = Path.Combine(path, FileName);
                if (File.Exists(fullPath) &&
                    (FileName != "gcc.exe" || (FileName == "gcc.exe" && !File.Exists(Path.Combine(path, "fpc.exe")))))
                    return fullPath;
            }
            return null;
        }


        static Compilers()
        {
            //CultureInfo culture = new CultureInfo(ConfigurationManager.AppSettings["DefaultCulture"]);
            //Thread.CurrentThread.CurrentCulture = culture;
            //Thread.CurrentThread.CurrentUICulture = culture;

            Load(LocalPath.CompilerOptionsDirectory);
        }

        static object lockOptions = new object();
        static object lockCommand = new object();
        static object lockPath = new object();
        static object lockAvailPL = new object();
        static Dictionary<ProgrammingLanguages, string> options = new Dictionary<ProgrammingLanguages, string>();
        static Dictionary<ProgrammingLanguages, string> command = new Dictionary<ProgrammingLanguages, string>();
        static Dictionary<ProgrammingLanguages, string> path = new Dictionary<ProgrammingLanguages, string>();

        static List<ProgrammingLanguages> availablePL = new List<ProgrammingLanguages>();

        public static void Load(string AbsolutePathToCompilerOptionsDirectory)
        {
            if (!System.IO.File.Exists(Path.Combine(AbsolutePathToCompilerOptionsDirectory, "options.xml")))
            {
                throw new ArgumentException(Path.Combine(AbsolutePathToCompilerOptionsDirectory, "options.xml") + " not found");
            }

            CultureInfo culture = new CultureInfo(ConfigurationManager.AppSettings["DefaultCulture"]);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            command.Clear();
            options.Clear();
            path.Clear();

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
                                    lock (lockCommand)
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
                                    lock (lockOptions)
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

            availablePL.Clear();
            lock (lockAvailPL)
            {
                lock (lockPath)
                {
                    availablePL.Add(ProgrammingLanguages.CS);
                    availablePL.Add(ProgrammingLanguages.VB);
                    availablePL.Add(ProgrammingLanguages.Open);

                    if (command.ContainsKey(ProgrammingLanguages.C) && ExistsOnPath(command[ProgrammingLanguages.C]))
                    {
                        availablePL.Add(ProgrammingLanguages.C);
                        if (path.ContainsKey(ProgrammingLanguages.C))
                            path.Remove(ProgrammingLanguages.C);
                        path.Add(ProgrammingLanguages.C, GetFullPath(command[ProgrammingLanguages.C]));
                    }

                    if (command.ContainsKey(ProgrammingLanguages.CPP) && ExistsOnPath(command[ProgrammingLanguages.CPP]))
                    {
                        availablePL.Add(ProgrammingLanguages.CPP);
                        if (path.ContainsKey(ProgrammingLanguages.CPP))
                            path.Remove(ProgrammingLanguages.CPP);
                        path.Add(ProgrammingLanguages.CPP, GetFullPath(command[ProgrammingLanguages.CPP]));
                    }
                    if (command.ContainsKey(ProgrammingLanguages.VCPP) && ExistsOnPath(command[ProgrammingLanguages.VCPP]))
                    {
                        availablePL.Add(ProgrammingLanguages.VCPP);
                        if (path.ContainsKey(ProgrammingLanguages.VCPP))
                            path.Remove(ProgrammingLanguages.VCPP);
                        path.Add(ProgrammingLanguages.VCPP, GetFullPath(command[ProgrammingLanguages.VCPP]));
                    }

                    if (command.ContainsKey(ProgrammingLanguages.Pascal) && ExistsOnPath(command[ProgrammingLanguages.Pascal]))
                    {
                        availablePL.Add(ProgrammingLanguages.Pascal);
                        if (path.ContainsKey(ProgrammingLanguages.Pascal))
                            path.Remove(ProgrammingLanguages.Pascal);
                        path.Add(ProgrammingLanguages.Pascal, GetFullPath(command[ProgrammingLanguages.Pascal]));
                    }
                    if (command.ContainsKey(ProgrammingLanguages.Delphi) && ExistsOnPath(command[ProgrammingLanguages.Delphi]))
                    {
                        availablePL.Add(ProgrammingLanguages.Delphi);
                        if (path.ContainsKey(ProgrammingLanguages.Delphi))
                            path.Remove(ProgrammingLanguages.Delphi);
                        path.Add(ProgrammingLanguages.Delphi, GetFullPath(command[ProgrammingLanguages.Delphi]));
                    }
                    if (command.ContainsKey(ProgrammingLanguages.ObjPas) && ExistsOnPath(command[ProgrammingLanguages.ObjPas]))
                    {
                        availablePL.Add(ProgrammingLanguages.ObjPas);
                        if (path.ContainsKey(ProgrammingLanguages.ObjPas))
                            path.Remove(ProgrammingLanguages.ObjPas);
                        path.Add(ProgrammingLanguages.ObjPas, GetFullPath(command[ProgrammingLanguages.ObjPas]));
                    }
                    if (command.ContainsKey(ProgrammingLanguages.TurboPas) && ExistsOnPath(command[ProgrammingLanguages.TurboPas]))
                    {
                        availablePL.Add(ProgrammingLanguages.TurboPas);
                        if (path.ContainsKey(ProgrammingLanguages.TurboPas))
                            path.Remove(ProgrammingLanguages.TurboPas);
                        path.Add(ProgrammingLanguages.TurboPas, GetFullPath(command[ProgrammingLanguages.TurboPas]));
                    }


                    if (ExistsOnPath("python.exe"))
                    {
                        availablePL.Add(ProgrammingLanguages.Python);
                        if (path.ContainsKey(ProgrammingLanguages.Python))
                            path.Remove(ProgrammingLanguages.Python);
                        path.Add(ProgrammingLanguages.Python, GetFullPath("python.exe"));
                    }

                    if (ExistsOnPath("javac.exe"))
                    {
                        availablePL.Add(ProgrammingLanguages.Java);
                        if (path.ContainsKey(ProgrammingLanguages.Java))
                            path.Remove(ProgrammingLanguages.Java);
                        path.Add(ProgrammingLanguages.Java, GetFullPath("javac.exe"));
                    }
                }
            }

        }

        public static string GetOptions(ProgrammingLanguages PL)
        {
            lock (lockOptions)
            {
                return options.ContainsKey(PL) ? options[PL] : String.Empty;
            }
        }

        public static string GetCommand(ProgrammingLanguages PL)
        {
            lock (lockCommand)
            {
                return command.ContainsKey(PL) ? command[PL] : String.Empty;
            }
        }

        public static string GetPath(ProgrammingLanguages PL)
        {
            lock (lockPath)
            {
                return path.ContainsKey(PL) ? path[PL] : String.Empty;
            }
        }

        public static List<ProgrammingLanguages> GetAvailablePL()
        {
            lock (lockAvailPL)
            {
                return availablePL;
            }
        }
    }
}