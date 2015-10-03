using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Solomon.WebUI.Helpers
{
    public static class StdCheckers
    {
        public class Checker
        {
            public string CheckerID { get; set; }
            public string FileName { get; set; }
            public string Description { get; set; }
            public bool Available { get; set; }
        }

        private static List<Checker> checkers = new List<Checker>();
        private static ILog logger = LogManager.GetLogger(typeof(StdCheckers));

        static StdCheckers()
        {
            XmlConfigurator.Configure();
            UpdateCheckersList();
        }

        public static List<Checker> Checkers
        {
            get { return checkers; }
        }

        public static void UpdateCheckersList()
        {
            try
            {
                using (StreamReader sr = new StreamReader(LocalPath.AbsoluteCheckersDirectory + "stdCheckers.inf"))
                {
                    Checker temp;
                    int id = 1;
                    while (!sr.EndOfStream)
                    {
                        temp = new Checker()
                            {
                                CheckerID = id.ToString(),
                                FileName = sr.ReadLine(),
                                Description = sr.ReadLine()
                            };
                        temp.Available = File.Exists(LocalPath.AbsoluteCheckersDirectory + temp.FileName);

                        checkers.Add(temp);
                        id++;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred on updating checker list:", ex);
            }
        }
    }
}