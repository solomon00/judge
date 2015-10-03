using NLog;
using ProcessPrivileges;
using Solomon.Tester.Helpers;
using Solomon.TypesExtensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Solomon.Tester
{
    class TesterSingleton
    {
        #region Helpers
        protected static string GetFullPath(string FileName)
        {
            if (File.Exists(FileName))
                return Path.GetFullPath(FileName);

            var values = Environment.GetEnvironmentVariable("PATH");
            foreach (var path in values.Split(';'))
            {
                var fullPath = Path.Combine(path, FileName);
                if (File.Exists(fullPath))
                    return fullPath;
            }
            return null;
        }

        protected List<Privilege> RemovePrivilege = new List<Privilege>()
            {
                Privilege.Shutdown,
                Privilege.SystemEnvironment,
                Privilege.SystemProfile,
                Privilege.Backup,
                Privilege.AssignPrimaryToken,
                Privilege.CreatePageFile,
                Privilege.EnableDelegation,
                Privilege.IncreaseBasePriority,
                Privilege.LoadDriver,
                Privilege.LockMemory,
                Privilege.ManageVolume,
                Privilege.Relabel,
                Privilege.RemoteShutdown,
                Privilege.Restore,
                Privilege.Security,
                Privilege.SystemTime,
                Privilege.TakeOwnership,
                Privilege.UnsolicitedInput
            };
        #endregion

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static CultureInfo culture = new CultureInfo(ConfigurationManager.AppSettings["DefaultCulture"]);

        private static TesterSingleton instance = new TesterSingleton();
        public static TesterSingleton Instance
        {
            get { return instance; }
        }

        private TesterSingleton()
        {
            //CultureInfo culture = new CultureInfo(ConfigurationManager.AppSettings["DefaultCulture"]);
            //Thread.CurrentThread.CurrentCulture = culture;
            //Thread.CurrentThread.CurrentUICulture = culture;
        }

        public ManualResetEventSlim CanTest = new ManualResetEventSlim(true);
        public ManualResetEventSlim AllDone = new ManualResetEventSlim(true);
        private int threadCount = 0;

        protected void RunSolution(Process proc, String Solution, String InputFile, String OutputFile,
            double TimeLimit, int MemoryLimit, out TestResults Result, out long SolutionTime, out long SolutionMemory)
        {
            Stopwatch stopWatch = new Stopwatch();
            Result = TestResults.RTE;
            SolutionTime = 0;
            SolutionMemory = 0;
            Int64 memorySize = 0;

            //Console.WriteLine(DateTime.Now.ToString(culture) + " - Start test " + Path.GetFileName(InputFile));

            string input = "";
            string output = "";
            string error = "";

            
                try
                {
                    using (StreamReader sr = new StreamReader(InputFile))
                    {
                        input = sr.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("--> " + ex.Message + "\n" + ex.StackTrace);
                }

            try
            {
                memorySize = proc.PrivateMemorySize64;
            }
            catch (Exception) { }

            TestResults tempResult = Result;
            AutoResetEvent startExec = new AutoResetEvent(false);
            AutoResetEvent endExec = new AutoResetEvent(false);

            Task<string> outputReader = null;
            ThreadPool.QueueUserWorkItem(state =>
                {
                    startExec.Set();

                    try
                    {
                        Task tsk = null;

                        try
                        {
                            using (StreamWriter sw = proc.StandardInput)
                            {
                                tsk = sw.WriteAsync(input);

                                try
                                {
                                    tsk.Wait(10 * 1000);
                                }
                                catch (AggregateException)
                                {
                                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Aggregate exception on tsk.Wait at " + Solution);
                                }

                                if (!tsk.IsCompleted)
                                {
                                    try
                                    {
                                        proc.Kill();
                                        proc.WaitForExit(2000);
                                    }
                                    catch (InvalidOperationException)
                                    {
                                        Console.WriteLine(DateTime.Now.ToString(culture) + " - Invalid operation exception on proc.Kill at " + Solution);
                                    }

                                    tempResult = TestResults.RTE;
                                    return;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(DateTime.Now.ToString(culture) + " - ----- Exception at " + Solution + " " + ex.Message);
                        }

                        stopWatch.Start();
                        while (!proc.HasExited &&
                                stopWatch.ElapsedMilliseconds < TimeLimit * 1000 &&
                                memorySize < MemoryLimit * 1024)
                        {
                            Thread.Sleep((int)(TimeLimit * 10));
                            proc.Refresh();
                            try
                            {
                                memorySize = proc.PrivateMemorySize64;
                                if (memorySize > MemoryLimit * 1024) break;

                                if (outputReader == null)
                                    outputReader = proc.StandardOutput.ReadToEndAsync();
                                else if (outputReader.IsCompleted)
                                {
                                    output += outputReader.Result;
                                    outputReader = proc.StandardOutput.ReadToEndAsync();
                                }
                            }
                            catch (Exception) { }
                        }
                        stopWatch.Stop();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(DateTime.Now.ToString(culture) + " - Exception at " + Solution + " " + ex.Message);
                    }
                    endExec.Set();
                });

            startExec.WaitOne();
            endExec.WaitOne((int)((10 + TimeLimit) * 1000));

            //Console.WriteLine(DateTime.Now.ToString(culture) + " - End of exec on test " + Path.GetFileName(InputFile));

            if (!proc.HasExited)
            {
                try
                {
                    proc.Kill();
                    proc.WaitForExit(2000);
                }
                catch (InvalidOperationException) 
                {
                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Invalid operation exception on proc.Kill at " + Solution);
                }
            }

            if (outputReader != null)
            {
                if (outputReader.IsCompleted)
                    output += outputReader.Result;
                else
                {
                    if (outputReader.Wait(1000))
                        output += outputReader.Result;
                }
            }

            SolutionTime = stopWatch.ElapsedMilliseconds;
            SolutionMemory = memorySize;

            if (stopWatch.ElapsedMilliseconds >= TimeLimit * 1000)
            {
                Result = TestResults.TLE;
                return;
            }
            if (memorySize > MemoryLimit * 1024)
            {
                Result = TestResults.MLE;
                return;
            }

            // Retrieve the app's exit code
            error = proc.StandardError.ReadToEnd();
            if (proc.ExitCode != 0)
            {
                Result = TestResults.RTE;

                logger.Info("Solution \"{0}\" exited with code \"{1}\"", Solution, proc.ExitCode);
                logger.Info("Solution \"{0}\" error out: {1}", Solution, error);
                Console.WriteLine("Solution \"{0}\" exited with code \"{1}\"", Solution, proc.ExitCode);
                Console.WriteLine("Solution \"{0}\" error out: {1}", Solution, error);
                return;
            }


            output += proc.StandardOutput.ReadToEnd();
            using (StreamWriter sw = new StreamWriter(OutputFile, false))
            {
                sw.Write(output);
            }

            if (logger.IsDebugEnabled || logger.IsTraceEnabled)
            {
                using (StreamWriter sw = new StreamWriter(
                    Path.Combine(Path.GetDirectoryName(OutputFile), Path.GetFileName(InputFile) + ".out"), false))
                {
                    sw.Write(output);
                }
            }

            Result = TestResults.OK;
        }

        /// <summary>
        /// For executable files
        /// </summary>
        /// <param name="SolutionExe"></param>
        /// <param name="InputFile"></param>
        /// <param name="OutputFile"></param>
        /// <param name="TimeLimit"></param>
        /// <param name="MemoryLimit"></param>
        /// <param name="Result"></param>
        protected void RunSolutionExe(String SolutionExe, String InputFile, String OutputFile,
            double TimeLimit, int MemoryLimit, out TestResults Result, out long SolutionTime, out long SolutionMemory)
        {
            Result = TestResults.RTE;
            SolutionTime = 0;
            SolutionMemory = 0;

            ProcessStartInfo startInfo = new ProcessStartInfo();

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.FileName = SolutionExe;
            startInfo.CreateNoWindow = true;

            // Run the external process & wait for it to finish
            using (Process proc = Process.Start(startInfo))
            {
                using (AccessTokenHandle accessTokenHandle =
                       proc.GetAccessTokenHandle(
                       TokenAccessRights.AdjustPrivileges | TokenAccessRights.Query))
                {
                    #region Remove privileges using the same access token handle.
                    accessTokenHandle.RemovePrivilege(Privilege.Shutdown);
                    RemovePrivilege.Each(p => { try { accessTokenHandle.RemovePrivilege(p); } catch(Exception) {}; });
                    #endregion

                    try
                    {
                        RunSolution(proc, SolutionExe, InputFile, OutputFile, TimeLimit, MemoryLimit, 
                            out Result, out SolutionTime, out SolutionMemory);
                    }
                    catch (InvalidOperationException ex)
                    {
                        if (logger.IsDebugEnabled)
                            Console.WriteLine(DateTime.Now.ToString(culture) + " - Invalid operation exception on " + SolutionExe, ex.Message);
                    }
                } // using AccessToken
            } // using Process
        }

        /// <summary>
        /// For Java class files
        /// </summary>
        /// <param name="SolutionExe"></param>
        /// <param name="InputFile"></param>
        /// <param name="OutputFile"></param>
        /// <param name="TimeLimit"></param>
        /// <param name="MemoryLimit"></param>
        /// <param name="Result"></param>
        protected void RunSolutionClass(String SolutionClass, String InputFile, String OutputFile,
            double TimeLimit, int MemoryLimit, out TestResults Result, out long SolutionTime, out long SolutionMemory)
        {
            if (!CompilerSingleton.Instance.AvailablePL.Contains(ProgrammingLanguages.Java))
            {
                throw new NotSupportedException("Compiler " + ProgrammingLanguages.Java.ToString() + " not supported");
            }

            Result = TestResults.RTE;
            SolutionTime = 0;
            SolutionMemory = 0;

            ProcessStartInfo startInfo = new ProcessStartInfo();

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.FileName = GetFullPath("java.exe");
            startInfo.Arguments = Path.GetFileNameWithoutExtension(SolutionClass);
            startInfo.CreateNoWindow = true;
            startInfo.WorkingDirectory = Path.GetDirectoryName(SolutionClass);

            // Run the external process & wait for it to finish
            using (Process proc = Process.Start(startInfo))
            {
                using (AccessTokenHandle accessTokenHandle =
                       proc.GetAccessTokenHandle(
                       TokenAccessRights.AdjustPrivileges | TokenAccessRights.Query))
                {
                    #region Remove privileges using the same access token handle.
                    accessTokenHandle.RemovePrivilege(Privilege.Shutdown);
                    RemovePrivilege.Each(p => { try { accessTokenHandle.RemovePrivilege(p); } catch (Exception) { }; });
                    #endregion

                    try
                    {
                        RunSolution(proc, SolutionClass, InputFile, OutputFile, TimeLimit, MemoryLimit, 
                            out Result, out SolutionTime, out SolutionMemory);
                    }
                    catch (InvalidOperationException ex)
                    {
                        if (logger.IsDebugEnabled)
                            Console.WriteLine(DateTime.Now.ToString(culture) + " - Invalid operation exception on " + SolutionClass, ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// For python files
        /// </summary>
        /// <param name="SolutionExe"></param>
        /// <param name="InputFile"></param>
        /// <param name="OutputFile"></param>
        /// <param name="TimeLimit"></param>
        /// <param name="MemoryLimit"></param>
        /// <param name="Result"></param>
        protected void RunSolutionPython(String SolutionPy, String InputFile, String OutputFile,
            double TimeLimit, int MemoryLimit, out TestResults Result, out long SolutionTime, out long SolutionMemory)
        {
            if (!CompilerSingleton.Instance.AvailablePL.Contains(ProgrammingLanguages.Python))
            {
                throw new NotSupportedException("Compiler " + ProgrammingLanguages.Python.ToString() + " not supported");
            }

            Result = TestResults.RTE;
            SolutionTime = 0;
            SolutionMemory = 0;

            ProcessStartInfo startInfo = new ProcessStartInfo();

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.FileName = GetFullPath("python.exe");
            startInfo.Arguments = Path.GetFileName(SolutionPy);
            startInfo.CreateNoWindow = true;
            startInfo.WorkingDirectory = Path.GetDirectoryName(SolutionPy);

            // Run the external process & wait for it to finish
            using (Process proc = Process.Start(startInfo))
            {
                using (AccessTokenHandle accessTokenHandle =
                       proc.GetAccessTokenHandle(
                       TokenAccessRights.AdjustPrivileges | TokenAccessRights.Query))
                {
                    #region Remove privileges using the same access token handle.
                    accessTokenHandle.RemovePrivilege(Privilege.Shutdown);
                    RemovePrivilege.Each(p => { try { accessTokenHandle.RemovePrivilege(p); } catch (Exception) { }; });
                    #endregion

                    try
                    {
                        RunSolution(proc, SolutionPy, InputFile, OutputFile, TimeLimit, MemoryLimit,
                            out Result, out SolutionTime, out SolutionMemory);
                    }
                    catch (InvalidOperationException ex)
                    {
                        if (logger.IsDebugEnabled)
                            Console.WriteLine(DateTime.Now.ToString(culture) + " - Invalid operation exception on " + SolutionPy, ex.Message);
                    }
                }
            }
        }

        protected void CheckSolutionOutput(string Checker, string Input, string Output, string Answer, out TestResults Result)
        {
            Result = TestResults.OK;

            ProcessStartInfo startInfo = new ProcessStartInfo();

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.FileName = Checker;
            startInfo.Arguments = "\"" + Input + "\" \"" + Output + "\" \"" + Answer + "\" C:\\1.txt";
            startInfo.CreateNoWindow = true;
            //if (WorkingDirectory != null)
            //    startInfo.WorkingDirectory = WorkingDirectory;

            string error = "";
            using (Process proc = Process.Start(startInfo))
            {
                error += proc.StandardError.ReadToEnd();
                error += proc.StandardOutput.ReadToEnd();

                proc.WaitForExit();

                Result = (TestResults)proc.ExitCode;
            }

            return;
        }

        public void TestStandart(Int32 ProblemID, Int32 SolutionID, string SolutionName,
            ProgrammingLanguages PL, TournamentFormats TF,
            out TestResults Result, out int Score, out List<Tuple<long, long, TestResults>> TestResults)
        {
            // TODO: FL
            Result = Solomon.TypesExtensions.TestResults.RTE;
            TestResults = null;
            Score = 0;

            File.Copy(LocalPath.ProblemsDirectory + ProblemID.ToString() + "\\checker.exe",
                LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\checker.exe", true);

            DirectoryExtensions.Copy(LocalPath.ProblemsDirectory + ProblemID.ToString() + "\\Tests",
                LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\Tests", false);

            if (!Directory.Exists(LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\exe"))
            {
                Directory.CreateDirectory(LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\exe");
            }
            if (!Directory.Exists(LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\output"))
            {
                Directory.CreateDirectory(LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\output");
            }

            CompilerSingleton.Instance.Compile(
                LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\" + SolutionName,
                LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\exe\\" + SolutionName + ".exe", PL, out Result, SolutionID);

            if (Result != Solomon.TypesExtensions.TestResults.OK)
            {
                //Interlocked.Decrement(ref threadCount);
                return;
            }

            // Testing
            // TODO: FL
            Result = Solomon.TypesExtensions.TestResults.RTE;
            TestResults = new List<Tuple<long, long, TestResults>>();
            Tuple<long, long, TestResults> tempTuple;

            // Get all "in" files
            String[] inFiles = null;

            inFiles = Directory.GetFiles(
                LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\Tests", "*.in");

            double timeLimit;
            int memoryLimit;
            string name;
            long solutionTime, solutionMemory;
            DateTime lastModifiedTime;
            ProblemLegend.Read(LocalPath.ProblemsDirectory + ProblemID.ToString(),
                out name, out timeLimit, out memoryLimit, out lastModifiedTime);

            int testOK = 0;
            foreach (String inFile in inFiles)
            {
                solutionTime = 0;
                solutionMemory = 0;

                try
                {
                    if (PL == ProgrammingLanguages.Java)
                    {
                        RunSolutionClass(
                            LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\exe\\" + SolutionName,
                            inFile,
                            LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\output\\output.txt",
                            timeLimit, memoryLimit, out Result, out solutionTime, out solutionMemory);
                    }
                    else if (PL == ProgrammingLanguages.Python)
                    {
                        RunSolutionPython(
                            LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\exe\\" + SolutionName,
                            inFile,
                            LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\output\\output.txt",
                            timeLimit, memoryLimit, out Result, out solutionTime, out solutionMemory);
                    }
                    else
                    {
                        RunSolutionExe(
                            LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\exe\\" + SolutionName + ".exe",
                            inFile,
                            LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\output\\output.txt",
                            timeLimit, memoryLimit, out Result, out solutionTime, out solutionMemory);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Warning on solution {0} running: {1}", SolutionID, ex.Message);
                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Warning on solution {0} running: {1}", SolutionID, ex.Message);
                }

                if (Result != Solomon.TypesExtensions.TestResults.OK)
                {
                    tempTuple = new Tuple<long, long, TestResults>(solutionTime, solutionMemory, Result);
                    TestResults.Add(tempTuple);

                    if (TF == TournamentFormats.ACM)
                        break;
                    else
                        continue;
                }

                Result = Solomon.TypesExtensions.TestResults.Executing;
                for (int i = 0; i < 5 && (int)Result >= 4; i++)
                {
                    CheckSolutionOutput(
                        LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\checker.exe",
                        inFile,
                        LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\output\\output.txt",
                        LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\Tests\\" + Path.GetFileNameWithoutExtension(inFile) + ".out",
                        out Result);
                }

                tempTuple = new Tuple<long, long, TestResults>(solutionTime, solutionMemory, Result);
                TestResults.Add(tempTuple);

                if (Result != Solomon.TypesExtensions.TestResults.OK)
                {
                    if (TF == TournamentFormats.ACM)
                        break;
                }
                else
                {
                    testOK++;
                }
            }

            if (TF == TournamentFormats.IOI)
            {
                Score = 100 * testOK / inFiles.Length;
                Result = testOK == inFiles.Length ? Solomon.TypesExtensions.TestResults.OK : Solomon.TypesExtensions.TestResults.PS;
            }

            if (File.Exists(LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\output\\output.txt"))
                File.Delete(LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\output\\output.txt");

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    Directory.Delete(LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\Tests", true);
                    break;
                }
                catch (Exception) { Thread.Sleep(100); }
            }
        }
        
        public void TestOpen(Int32 ProblemID, Int32 SolutionID, string SolutionName,
            ProgrammingLanguages PL, TournamentFormats TF,
            out TestResults Result, out int Score)
        {
            // TODO: FL
            Result = TestResults.RTE;
            Score = 0;

            File.Copy(LocalPath.ProblemsDirectory + ProblemID.ToString() + "\\checker.exe",
                LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\checker.exe", true);

            // Test

            CheckSolutionOutput(
                LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\checker.exe",
                LocalPath.ProblemsDirectory + ProblemID.ToString() + "\\OpenProblemResult",
                LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\" + SolutionName,
                LocalPath.ProblemsDirectory + ProblemID.ToString() + "\\OpenProblemResult",
                out Result);

            if (Result == TestResults.OK)
            {
                Score = 100;
            }
        }

        public void Test(Int32 ProblemID, Int32 SolutionID, string SolutionName, 
            ProgrammingLanguages PL, TournamentFormats TF, ProblemTypes PT,
            out TestResults Result, out int Score, out List<Tuple<long, long, TestResults>> testResults)
        {
            CanTest.Wait();
            AllDone.Reset();
            Interlocked.Increment(ref threadCount);

            logger.Debug("Start {0} thread", threadCount);
            if (logger.IsDebugEnabled)
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Start {0} thread", threadCount);

            // TODO: FL
            Result = TestResults.RTE;
            Score = 0;
            testResults = null;

            logger.Trace("Start testing solution {0}", SolutionID);
            if (logger.IsTraceEnabled)
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Start testing solution {0}", SolutionID);

            try
            {
                switch (PT)
                {
                    case ProblemTypes.Standart:
                        TestStandart(ProblemID, SolutionID, SolutionName, PL, TF, out Result, out Score, out testResults);
                        break;
                    case ProblemTypes.Open:
                        TestOpen(ProblemID, SolutionID, SolutionName, PL, TF, out Result, out Score);
                        break;

                }

            }
            catch (Exception ex)
            {
                logger.Error("Error occurred on solution {0} testing: {1}", SolutionID, ex.Message);
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Error occurred on solution {0} testing: {1}", SolutionID, ex.Message);
            }

            Interlocked.Decrement(ref threadCount);
            if (threadCount == 0)
                AllDone.Set();
        }

    }
}
