using NLog;
//using Solomon.Domain.Abstract;
//using Solomon.Domain.Concrete;
//using Solomon.Domain.Entities;
using Solomon.Tester.Helpers;
using Solomon.TypesExtensions;
using System;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Globalization;
using System.Configuration;
using Microsoft.Win32;
using System.Collections.Generic;

namespace Solomon.Tester
{
    class Program
    {
        private static PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        //private static IRepository repository = new EFRepository();
        private static SocketConnection socket;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static CultureInfo culture = new CultureInfo(ConfigurationManager.AppSettings["DefaultCulture"]);

        private static void SetupRegistry()
        {
            logger.Info("Begin reading registry values");
            if (logger.IsInfoEnabled)
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Begin reading registry values");
            Int32 DontShowUI = (Int32)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\Windows Error Reporting", "DontShowUI", -1);

            if (DontShowUI == -1)
            {
                logger.Info("Regestry key not exist: HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\Windows Error Reporting\\DontShowUI");
                logger.Info("Creating...");
                if (logger.IsInfoEnabled)
                {
                    Console.WriteLine(DateTime.Now.ToString(culture) +
                        " - Regestry key not exist: HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\Windows Error Reporting\\DontShowUI");
                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Creating...");
                }

                RegistryKey key = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\Windows Error Reporting");
                key.SetValue("DontShowUI", 1, RegistryValueKind.DWord);
                key.Close();

                logger.Info("Registry key created");
                if (logger.IsInfoEnabled)
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Registry key created");
            }
            else if (DontShowUI == 0)
            {
                logger.Info("Regestry key set to 0: HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\Windows Error Reporting\\DontShowUI");
                logger.Info("Setting value to 1 ...");
                if (logger.IsInfoEnabled)
                {
                    Console.WriteLine(DateTime.Now.ToString(culture) +
                        " - Regestry key set to 0: HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\Windows Error Reporting\\DontShowUI");
                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Setting value to 1 ...");
                }

                Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\Windows Error Reporting", "DontShowUI", 1, RegistryValueKind.DWord);

                logger.Info("Registry key set to 1");
                if (logger.IsInfoEnabled)
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Registry key set to 1");
            }
            else
            {
                logger.Info("Registry already set up");
                if (logger.IsInfoEnabled)
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Registry already set up");
            }
        }

        private static void ShowCompilers()
        {
            Console.WriteLine(DateTime.Now.ToString(culture) + " - Available compilers:");
            List<ProgrammingLanguages> availablePL = new List<ProgrammingLanguages>();
            availablePL.AddRange(CompilerSingleton.Instance.AvailablePL);
            foreach (var item in availablePL)
            {
                Console.WriteLine(item);
            }
        }

        static void Main(string[] args)
        {
            //CultureInfo culture = new CultureInfo(ConfigurationManager.AppSettings["DefaultCulture"]);
            //Thread.CurrentThread.CurrentCulture = culture;
            //Thread.CurrentThread.CurrentUICulture = culture;

            List<Tuple<string, string>> commands = new List<Tuple<string, string>>()
                {
                    new Tuple<string, string>("compilers", "show available compilers"),
                    new Tuple<string, string>("exit", "exit from tester")
                };

            SetupRegistry();

            socket = new SocketConnection();
            socket.ReceivedData += socket_ReceivedData;

            string command = "";
            while (true)
            {
                try
                {
                    command = Console.ReadLine();

                    switch (command)
                    {
                        case "compilers":
                            ShowCompilers();
                            break;
                        case "exit":
                            return;
                        case "help":
                            commands.Each(c => { Console.Write(c.Item1); Console.CursorLeft = 20; Console.WriteLine(" - " + c.Item2); });
                            break;
                        case "":
                            Console.CursorTop--;
                            Console.WriteLine(DateTime.Now);
                            break;
                        default:
                            Console.WriteLine(DateTime.Now.ToString(culture) + " - Unknown command: {0}", command);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Translate received data. Get the key value and the message.
        /// </summary>
        /// <remarks>
        /// First 4 bytes   - length of data.
        /// Second 4 bytes  - key value.
        /// 
        /// Key:
        /// 1   Get cpu usage.
        /// 
        /// 100 Solution file:
        ///     [4b - solution id][4b - file name length][file name][file data]
        /// 
        /// 101 Problem file:
        ///     [4b - file name length][file name][file data]
        /// 
        /// </remarks>
        /// <param name="data"></param>
        static void socket_ReceivedData(object sender, SocketConnection.ReceivedDataEventArgs e)
        {
            Int32 code = BitConverter.ToInt32(e.ReceivedData, 4);
            byte[] data = null;
            int solutionID = -1, problemID = -1;
            string fileName;
            ProgrammingLanguages pl;
            TournamentFormats tf;
            ProblemTypes pt;

            try
            {
                logger.Trace("Received code: {0}", (RequestCodes)code);
                if (logger.IsTraceEnabled)
                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Received code: {0}", (RequestCodes)code);

                switch ((RequestCodes)code)
                {
                    case RequestCodes.MainInfo:
                        SendMainInfo((SocketConnection)sender, (Int32)ResponseCodes.MainInfo);
                        break;
                    case RequestCodes.CPUUsage:
                        SendCPUUsage((SocketConnection)sender, (Int32)ResponseCodes.CPUUsage);
                        break;
                    case RequestCodes.SolutionFile:
                        data = new byte[e.ReceivedData.Length];
                        Array.Copy(e.ReceivedData, data, e.ReceivedData.Length);

                        ThreadPool.QueueUserWorkItem(state =>
                            {
                                TestResults result;
                                int score;

                                SaveSolution(data, out problemID, out solutionID, out fileName, out pl, out tf, out pt);

                                List<Tuple<Int64, Int64, TestResults>> testResults = null;// = new List<Tuple<long, long, TestResult>>();

                                TesterSingleton.Instance.Test(problemID, solutionID, fileName, pl, tf, pt, out result, out score, out testResults);

                                logger.Info("Complete testing solution {0} for problem {1} on PL {2}, result: {3}", 
                                    solutionID, problemID, pl, result);
                                Console.WriteLine(DateTime.Now.ToString(culture) + " - Complete testing solution {0} for problem {1} on PL {2}, result: {3}", 
                                    solutionID, problemID, pl, result);

                                DeleteSolution(solutionID);

                                SendSolutionChecked((SocketConnection)sender, (Int32)ResponseCodes.SolutionFileChecked,
                                    solutionID, (Int32)pt, (Int32)result, score, testResults);
                            });

                        break;
                    case RequestCodes.ReadyForReceivingProblem:
                        //socket.CheckConnectionTimer.Stop();
                        TesterSingleton.Instance.CanTest.Reset();
                        TesterSingleton.Instance.AllDone.Wait();
                        SendRequestForRecivingProblem((SocketConnection)sender, (Int32)ResponseCodes.ReadyForReceivingProblem);
                        break;
                    case RequestCodes.EndOfReceivingProblem:
                        TesterSingleton.Instance.CanTest.Set();

                        //socket.CheckConnectionTimer.Start();
                        //SendProblemsInfo((SocketConnection)sender, (Int32)ResponseCodes.ProblemsInfo);
                        break;
                    case RequestCodes.ProblemFile:
                        data = new byte[e.ReceivedData.Length];
                        Array.Copy(e.ReceivedData, data, e.ReceivedData.Length);

                        TestResults res;
                        bool isCorrect = false;
                        string info = "";
                        DateTime lastModifiedTime = DateTime.MinValue;
                        try
                        {
                            SaveProblem(data, out problemID, out lastModifiedTime);
                            CompileChecker(problemID, out res);
                            CheckProblem(problemID, out isCorrect, out info);
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Problem receiving/saving failed: {0}", ex.Message);
                            Console.WriteLine(DateTime.Now.ToString(culture) + " - Problem receiving/saving failed: {0}", ex.Message);
                        }
                        finally
                        {
                            ProblemsInfo.Instance.Add(new ProblemInfo()
                                {
                                    ProblemID = problemID,
                                    IsCorrect = isCorrect,
                                    Info = info,
                                    LastModifiedTime = lastModifiedTime
                                }
                            );

                            SendProblemReceived((SocketConnection)sender, (Int32)ResponseCodes.ProblemFileReceived);
                        }
                        break;
                    case RequestCodes.DeleteProblem:
                        TesterSingleton.Instance.CanTest.Reset();
                        TesterSingleton.Instance.AllDone.Wait();

                        problemID = BitConverter.ToInt32(e.ReceivedData, 8);
                        DeleteProblem(problemID);
                        ProblemsInfo.Instance.Delete(problemID);

                        SendProblemDeleted((SocketConnection)sender, (Int32)ResponseCodes.ProblemDeleted);
                        //SendProblemsInfo((SocketConnection)sender, (Int32)ResponseCodes.ProblemsInfo);
                        break;
                    case RequestCodes.ProblemsInfo:
                        SendProblemsInfo((SocketConnection)sender, (Int32)ResponseCodes.ProblemsInfo);
                        break;
                    case RequestCodes.ReadyForReceivingCompilerOptions:
                        //socket.CheckConnectionTimer.Stop();
                        TesterSingleton.Instance.CanTest.Reset();
                        TesterSingleton.Instance.AllDone.Wait();
                        SendRequestForRecivingCompilerOptions((SocketConnection)sender, (Int32)ResponseCodes.ReadyForReceivingCompilerOptions);
                        break;
                    case RequestCodes.CompilerOptionsFile:
                        data = new byte[e.ReceivedData.Length];
                        Array.Copy(e.ReceivedData, data, e.ReceivedData.Length);

                        try
                        {
                            SaveCompilerOptions(data);
                            Compilers.Load(LocalPath.CompilerOptionsDirectory);
                        }
                        catch (Exception ex)
                        {
                            logger.Error("CompilerOptions receiving/saving failed: {0}", ex.Message);
                            Console.WriteLine(DateTime.Now.ToString(culture) + " - CompilerOptions receiving/saving failed: {0}", ex.Message);
                        }
                        finally
                        {
                            SendCompilerOptionsReceived((SocketConnection)sender, (Int32)ResponseCodes.CompilerOptionsFileReceived);
                            SendMainInfo((SocketConnection)sender, (Int32)ResponseCodes.MainInfo);
                            TesterSingleton.Instance.CanTest.Set();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Error occurred on request \"{0}\" processing: {1}", (RequestCodes)code, ex.Message);
                logger.Error("Error occurred on request \"{0}\" processing: {1}", (RequestCodes)code, ex.Message);
            }
        }

        private static void SendMainInfo(SocketConnection Socket, Int32 RequestCode)
        {
            Int32 virtProcessorsCount = Environment.ProcessorCount;

            Int32 compilersCount = CompilerSingleton.Instance.AvailablePL.Count;

            byte[] data = new byte[4 + 4 + 4 + 4 + 4 * compilersCount];
            BitConverter.GetBytes(data.Length).CopyTo(data, 0);
            BitConverter.GetBytes(RequestCode).CopyTo(data, 4);
            BitConverter.GetBytes(virtProcessorsCount).CopyTo(data, 8);
            BitConverter.GetBytes(compilersCount).CopyTo(data, 12);

            int i = 1;
            foreach (ProgrammingLanguages pl in CompilerSingleton.Instance.AvailablePL)
            {
                BitConverter.GetBytes((Int32)pl).CopyTo(data, 12 + 4 * i);
                i++;
            }

            Socket.Send(data, data.Length);

            logger.Trace("Send main info");
            if (logger.IsTraceEnabled)
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Send main info");
        }

        private static void SendCPUUsage(SocketConnection Socket, Int32 RequestCode)
        {
            Int32 cpuUsage = (Int32)cpuCounter.NextValue();
            byte[] data = new byte[4 + 4 + 4];
            BitConverter.GetBytes(4 + 4 + 4).CopyTo(data, 0);
            BitConverter.GetBytes(RequestCode).CopyTo(data, 4);
            BitConverter.GetBytes(cpuUsage).CopyTo(data, 8);

            Socket.Send(data, data.Length);

            logger.Trace("Send cpu usage");
            if (logger.IsTraceEnabled)
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Send cpu usage");
        }

        private static void SendRequestForRecivingProblem(SocketConnection Socket, Int32 RequestCode)
        {
            byte[] data = new byte[4 + 4];
            BitConverter.GetBytes(4 + 4).CopyTo(data, 0);
            BitConverter.GetBytes(RequestCode).CopyTo(data, 4);

            Socket.Send(data, data.Length);

            logger.Trace("Send request for receiving problem");
            if (logger.IsTraceEnabled)
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Send request for receiving problem");
        }

        private static void SendRequestForRecivingCompilerOptions(SocketConnection Socket, Int32 RequestCode)
        {
            byte[] data = new byte[4 + 4];
            BitConverter.GetBytes(4 + 4).CopyTo(data, 0);
            BitConverter.GetBytes(RequestCode).CopyTo(data, 4);

            Socket.Send(data, data.Length);

            logger.Trace("Send request for receiving CompilerOptions");
            if (logger.IsTraceEnabled)
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Send request for receiving CompilerOptions");
        }

        private static void SendSolutionChecked(SocketConnection Socket, Int32 RequestCode,
            Int32 SolutionID, Int32 PT, Int32 Result, Int32 Score, List<Tuple<Int64, Int64, TestResults>> TestResults)
        {
            int testsCount = TestResults == null ? 0 : TestResults.Count;

            int n = 4 + 4;
            n += 4;     // solution id
            n += 4;     // problem type
            n += 4;     // result
            n += 4;     // score
            n += 4;     // tests count
            n += (8 + 8 + 4) * testsCount;

            byte[] data = new byte[n];
            BitConverter.GetBytes(n).CopyTo(data, 0);
            BitConverter.GetBytes(RequestCode).CopyTo(data, 4);
            BitConverter.GetBytes(SolutionID).CopyTo(data, 8);
            BitConverter.GetBytes(PT).CopyTo(data, 12);
            BitConverter.GetBytes(Result).CopyTo(data, 16);
            BitConverter.GetBytes(Score).CopyTo(data, 20);
            BitConverter.GetBytes(testsCount).CopyTo(data, 24);

            if (TestResults != null)
            {
                int i = 0;
                foreach (var item in TestResults)
                {
                    BitConverter.GetBytes(item.Item1).CopyTo(data, 28 + i * 20);
                    BitConverter.GetBytes(item.Item2).CopyTo(data, 36 + i * 20);
                    BitConverter.GetBytes((Int32)item.Item3).CopyTo(data, 44 + i * 20);
                    i++;
                }
            }

            Socket.Send(data, data.Length);

            logger.Trace("Send solution checked");
            if (logger.IsTraceEnabled)
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Send solution checked");
        }

        private static void SendProblemReceived(SocketConnection Socket, Int32 RequestCode)
        {
            byte[] data = new byte[4 + 4];
            BitConverter.GetBytes(4 + 4).CopyTo(data, 0);
            BitConverter.GetBytes(RequestCode).CopyTo(data, 4);

            Socket.Send(data, data.Length);

            logger.Trace("Send problem received");
            if (logger.IsTraceEnabled)
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Send problem received");
        }

        private static void SendCompilerOptionsReceived(SocketConnection Socket, Int32 RequestCode)
        {
            byte[] data = new byte[4 + 4];
            BitConverter.GetBytes(4 + 4).CopyTo(data, 0);
            BitConverter.GetBytes(RequestCode).CopyTo(data, 4);

            Socket.Send(data, data.Length);

            logger.Trace("Send CompilerOptions received");
            if (logger.IsTraceEnabled)
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Send CompilerOptions received");
        }

        private static void SendProblemDeleted(SocketConnection Socket, Int32 RequestCode)
        {
            byte[] data = new byte[4 + 4];
            BitConverter.GetBytes(4 + 4).CopyTo(data, 0);
            BitConverter.GetBytes(RequestCode).CopyTo(data, 4);

            Socket.Send(data, data.Length);

            logger.Trace("Send problem deleted");
            if (logger.IsTraceEnabled)
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Send problem deleted");
        }

        private static void SendProblemsInfo(SocketConnection Socket, Int32 RequestCode)
        {
            ProblemsInfo.Instance.Serialize();

            if (!File.Exists(LocalPath.ProblemsDirectory + "problems.inf"))
            {
                throw new FileNotFoundException(LocalPath.ProblemsDirectory + "problems.inf");
            }

            byte[] fileName = Encoding.ASCII.GetBytes("problems.inf");
            byte[] fileData = File.ReadAllBytes(LocalPath.ProblemsDirectory + "problems.inf");
            byte[] data = new byte[4 + 4 + 4 + fileName.Length + fileData.Length];
            byte[] fileNameLen = BitConverter.GetBytes(fileName.Length);
            BitConverter.GetBytes(data.Length).CopyTo(data, 0);
            BitConverter.GetBytes(RequestCode).CopyTo(data, 4);
            fileNameLen.CopyTo(data, 8);
            fileName.CopyTo(data, 12);
            fileData.CopyTo(data, 12 + fileName.Length);

            Socket.Send(data, data.Length);

            logger.Trace("Send problems info");
            if (logger.IsTraceEnabled)
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Send problems info");
        }


        private static void SaveSolution(byte[] Data, 
            out int ProblemID, 
            out int SolutionID, 
            out string FileName, 
            out ProgrammingLanguages PL,
            out TournamentFormats TF,
            out ProblemTypes PT)
        {
            ProblemID = BitConverter.ToInt32(Data, 8);
            SolutionID = BitConverter.ToInt32(Data, 12);
            PL = (ProgrammingLanguages)BitConverter.ToInt32(Data, 16);
            TF = (TournamentFormats)BitConverter.ToInt32(Data, 20);
            PT = (ProblemTypes)BitConverter.ToInt32(Data, 24);
            Int32 FileNameLength = BitConverter.ToInt32(Data, 28);
            FileName = Encoding.ASCII.GetString(Data, 32, FileNameLength);

            try
            {
                if (!Directory.Exists(LocalPath.SolutionsDirectory))
                {
                    Directory.CreateDirectory(LocalPath.SolutionsDirectory);
                }

                if (Directory.Exists(LocalPath.SolutionsDirectory + SolutionID.ToString()))
                {
                    Directory.Delete(LocalPath.SolutionsDirectory + SolutionID.ToString(), true);
                }
                Directory.CreateDirectory(LocalPath.SolutionsDirectory + SolutionID.ToString());

                using (BinaryWriter bw = new BinaryWriter(
                    File.Open(LocalPath.SolutionsDirectory + SolutionID.ToString() + "\\" + FileName, FileMode.Create)))
                {
                    bw.Write(Data, FileNameLength + 32, Data.Length - FileNameLength - 32);
                }

                logger.Trace("Solution {0} saved", SolutionID);
                if (logger.IsTraceEnabled)
                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Solution {0} saved", SolutionID);
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred on solution {0} saving: {1}", SolutionID, ex.Message);
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Error occurred on solution {0} saving: {1}", SolutionID, ex.Message);
            }
        }

        private static void DeleteSolution(int SolutionID)
        {
            if (Directory.Exists(LocalPath.SolutionsDirectory + SolutionID.ToString()))
            {
                try
                {
                    if (!logger.IsDebugEnabled && !logger.IsTraceEnabled)
                        DirectoryExtensions.Delete(LocalPath.SolutionsDirectory + SolutionID.ToString());
                }
                catch (Exception ex)
                {
                    logger.Warn("Error occured on deleting solution {0} directory: {1}", SolutionID, ex.Message);
                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Error occured on deleting solution directory: {0}", ex.Message);
                }
            }
        }

        private static void SaveProblem(byte[] Data, out int ProblemID, out DateTime LastModifiedTime)
        {
            Int32 FileNameLength = BitConverter.ToInt32(Data, 8);
            string FileName = Encoding.ASCII.GetString(Data, 12, FileNameLength);

            int id;
            Int32.TryParse(Path.GetFileNameWithoutExtension(FileName), out id);
            ProblemID = id;
            LastModifiedTime = DateTime.MinValue;

            try
            {
                if (!Directory.Exists(LocalPath.ProblemsDirectory))
                {
                    Directory.CreateDirectory(LocalPath.ProblemsDirectory);
                }

                if (File.Exists(LocalPath.ProblemsDirectory + FileName + ".zip"))
                {
                    File.Delete(LocalPath.ProblemsDirectory + FileName + ".zip");
                }
                if (Directory.Exists(LocalPath.ProblemsDirectory + ProblemID.ToString()))
                {
                    DirectoryExtensions.Delete(LocalPath.ProblemsDirectory + ProblemID.ToString());
                }

                using (BinaryWriter bw = new BinaryWriter(
                    File.Open(LocalPath.ProblemsDirectory + FileName + ".zip", FileMode.Create)))
                {
                    bw.Write(Data, FileNameLength + 12, Data.Length - FileNameLength - 12);
                }

                ZipFile.ExtractToDirectory(
                    LocalPath.ProblemsDirectory + FileName + ".zip",
                    LocalPath.ProblemsDirectory + ProblemID.ToString());
                File.Delete(LocalPath.ProblemsDirectory + FileName + ".zip");

                double timeLimit;
                int memoryLimit;
                string name;
                DateTime lastModifiedTime;
                ProblemLegend.Read(LocalPath.ProblemsDirectory + ProblemID.ToString(),
                    out name, out timeLimit, out memoryLimit, out lastModifiedTime);
                LastModifiedTime = lastModifiedTime;

                logger.Debug("Problem {0} saved", ProblemID);
                if (logger.IsDebugEnabled)
                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Problem {0} saved", ProblemID);
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred on problem {0} saving: {1}", ProblemID, ex.Message);
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Error occurred on problem {0} saving: {1}", ProblemID, ex.Message);
                throw;
            }
        }

        private static void SaveCompilerOptions(byte[] Data)
        {
            Int32 FileNameLength = BitConverter.ToInt32(Data, 8);
            string FileName = Encoding.ASCII.GetString(Data, 12, FileNameLength);

            try
            {
                if (!Directory.Exists(LocalPath.CompilerOptionsDirectory))
                {
                    Directory.CreateDirectory(LocalPath.CompilerOptionsDirectory);
                }

                if (File.Exists(Path.Combine(LocalPath.CompilerOptionsDirectory, "options.xml")))
                {
                    File.Delete(Path.Combine(LocalPath.CompilerOptionsDirectory, "options.xml"));
                }

                using (BinaryWriter bw = new BinaryWriter(
                    File.Open(Path.Combine(LocalPath.CompilerOptionsDirectory, "options.xml"), FileMode.Create)))
                {
                    bw.Write(Data, FileNameLength + 12, Data.Length - FileNameLength - 12);
                }
                logger.Debug("CompilerOptions");
                if (logger.IsDebugEnabled)
                    Console.WriteLine(DateTime.Now.ToString(culture) + " - CompilerOptions saved");
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred on CompilerOptions saving: {0}", ex.Message);
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Error occurred on CompilerOptions saving: {0}", ex.Message);
                throw;
            }
        }

        private static void CompileChecker(int ProblemID, out TestResults Result)
        {
            Result = TestResults.FL;

            string[] testlib;
            string checkerName;
            ProgrammingLanguages checkerPL;
            if (!Directory.Exists(LocalPath.ProblemsDirectory + ProblemID.ToString()))
            {
                throw new DirectoryNotFoundException(LocalPath.ProblemsDirectory + ProblemID.ToString());
            }

            // Chose testlib
            if (File.Exists(LocalPath.ProblemsDirectory + ProblemID.ToString() + "\\checker.cpp"))
            {
                testlib = LocalPath.TestlibCPP;
                checkerName = "checker.cpp";
                checkerPL = ProgrammingLanguages.CPP;
            }
            else if (File.Exists(LocalPath.ProblemsDirectory + ProblemID.ToString() + "\\checker.pas"))
            {
                testlib = LocalPath.TestlibPAS;
                checkerName = "checker.pas";
                checkerPL = ProgrammingLanguages.Pascal;
            }
            else if (File.Exists(LocalPath.ProblemsDirectory + ProblemID.ToString() + "\\checker.dpr"))
            {
                testlib = LocalPath.TestlibPAS;
                checkerName = "checker.dpr";
                checkerPL = ProgrammingLanguages.Delphi;
            }
            else
            {
                Result = TestResults.FL;
                return;
            }

            // Copy testlib
            foreach (var item in testlib)
            {
                File.Copy(item, LocalPath.ProblemsDirectory + ProblemID.ToString() + "\\" + Path.GetFileName(item));
            }

            try
            {
                CompilerSingleton.Instance.Compile(
                    LocalPath.ProblemsDirectory + ProblemID.ToString() + "\\" + checkerName,
                    LocalPath.ProblemsDirectory + ProblemID.ToString() + "\\checker.exe",
                    checkerPL, out Result);

                if (Result == TestResults.OK)
                {
                    logger.Debug("Checker for problem {0} compiled", ProblemID);
                    if (logger.IsDebugEnabled)
                        Console.WriteLine(DateTime.Now.ToString(culture) + " - Checker for problem {0} compiled", ProblemID);
                }
                else
                {
                    logger.Error("Checker compilation for problem {0} failed with result: {1}", ProblemID, Result);
                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Checker compilation for problem {0} failed with result: {1}", ProblemID, Result);
                    if (File.Exists(LocalPath.ProblemsDirectory + ProblemID.ToString() + "\\checker.exe"))
                    {
                        File.Delete(LocalPath.ProblemsDirectory + ProblemID.ToString() + "\\checker.exe");
                    }
                }
            }
            catch (Exception ex)
            {
                if (File.Exists(LocalPath.ProblemsDirectory + ProblemID.ToString() + "\\checker.exe"))
                {
                    File.Delete(LocalPath.ProblemsDirectory + ProblemID.ToString() + "\\checker.exe");
                }

                logger.Error("Checker compilation for problem {0} failed: {1}", ProblemID, ex.Message);
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Checker compilation for problem {0} failed: {1}", ProblemID, ex.Message);
                throw;
            }
            finally
            {
                foreach (var item in testlib)
                {
                    if (File.Exists(LocalPath.ProblemsDirectory + ProblemID.ToString() + "\\" + Path.GetFileName(item)))
                    {
                        File.Delete(LocalPath.ProblemsDirectory + ProblemID.ToString() + "\\" + Path.GetFileName(item));
                    }
                }
            }
        }

        private static void CheckProblem(int ProblemID, out bool IsCorrect, out string Info)
        {
            IsCorrect = false;
            Info = "";

            if (!Directory.Exists(LocalPath.ProblemsDirectory + ProblemID.ToString()))
            {
                Info = "Problem not exist";
                return;
            }

            if (!Directory.Exists(LocalPath.ProblemsDirectory + ProblemID.ToString() + "\\Tests"))
            {
                Info = "Tests not exist";
                return;
            }
            else
            {
                // TODO: Check tests.
            }

            if (!File.Exists(LocalPath.ProblemsDirectory + ProblemID.ToString() + "\\checker.exe"))
            {
                Info = "Checker not exist";
                return;
            }

            if (!File.Exists(LocalPath.ProblemsDirectory + ProblemID.ToString() + "\\problem.xml"))
            {
                Info = "Problem.xml not exist";
                return;
            }

            IsCorrect = true;
            Info = "Ok";
        }

        private static void DeleteProblem(int ProblemID)
        {
            try
            {
                if (Directory.Exists(LocalPath.ProblemsDirectory + ProblemID))
                {
                    DirectoryExtensions.Delete(LocalPath.ProblemsDirectory + ProblemID);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Error occured on deleting problem {0} directory: {1}", ProblemID, ex.Message);
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Error occured on deleting problem {0} directory: {1}", ProblemID, ex.Message);
            }
        }
    }
}
