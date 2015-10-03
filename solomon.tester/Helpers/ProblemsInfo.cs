using NLog;
using Solomon.TypesExtensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Solomon.Tester.Helpers
{
    class ProblemsInfo
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static CultureInfo culture = new CultureInfo(ConfigurationManager.AppSettings["DefaultCulture"]);

        private static ProblemsInfo instance = new ProblemsInfo();
        private ProblemsInfo()
        {
            Deserialize();
        }
        public static ProblemsInfo Instance { get { return instance; } }

        private List<ProblemInfo> problems = new List<ProblemInfo>();

        public void Add(ProblemInfo PI)
        {
            ProblemInfo pi = problems.FirstOrDefault(p => p.ProblemID == PI.ProblemID);
            if (pi == null)
            {
                problems.Add(PI);
            }
            else
            {
                pi.IsCorrect = PI.IsCorrect;
                pi.Info = PI.Info;
                pi.LastModifiedTime = PI.LastModifiedTime;
            }
        }

        public void Delete(int ProblemID)
        {
            ProblemInfo pi = problems.FirstOrDefault(p => p.ProblemID == ProblemID);
            if (pi != null)
            {
                problems.Remove(pi);
            }
        }

        public void Serialize()
        {
            try
            {
                using (Stream stream = File.Open(LocalPath.ProblemsDirectory + "problems.inf", FileMode.Create))
                {
                    BinaryFormatter bformatter = new BinaryFormatter();

                    logger.Info("Writing problems info");
                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Writing problems info");

                    bformatter.Serialize(stream, problems);

                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Writed " + problems.Count + " problems");
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Serialization failed: {0}", ex.Message);
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Serialization failed: {0}", ex.Message);
            }
        }

        public void Deserialize()
        {
            try
            {
                if (File.Exists(LocalPath.ProblemsDirectory + "problems.inf"))
                {
                    using (Stream stream = File.Open(LocalPath.ProblemsDirectory + "problems.inf", FileMode.Open))
                    {
                        BinaryFormatter bformatter = new BinaryFormatter();

                        logger.Info("Reading problems info");
                        Console.WriteLine(DateTime.Now.ToString(culture) + " - Reading problems info");

                        problems = (List<ProblemInfo>)bformatter.Deserialize(stream);

                        Console.WriteLine(DateTime.Now.ToString(culture) + " - Readed " + problems.Count + " problems");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Deserialization failed: {0}", ex.Message);
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Deserialization failed: {0}", ex.Message);
            }
        }
    }
}
