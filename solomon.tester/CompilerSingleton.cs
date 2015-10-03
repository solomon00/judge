using NLog;
using Solomon.Tester.Helpers;
using Solomon.TypesExtensions;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Solomon.Tester
{
    class CompilerSingleton
    {
        #region Helpers
        
        protected string RemoveComments(string Source)
        {
            var blockComments = @"/\*(.*?)\*/";
            var lineComments = @"//(.*?)\r?\n";

            string noComments = Regex.Replace(Source,
                blockComments + "|" + lineComments,
                me =>
                {
                    if (me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
                        return me.Value.StartsWith("//") ? Environment.NewLine : "";
                    // Keep the literal strings
                    return me.Value;
                },
                RegexOptions.Singleline);

            return noComments;
        }
        #endregion

        private static CompilerSingleton instance = new CompilerSingleton();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static CultureInfo culture = new CultureInfo(ConfigurationManager.AppSettings["DefaultCulture"]);

        public List<ProgrammingLanguages> AvailablePL
        {
            get { return Compilers.GetAvailablePL(); }
        }

        private CompilerSingleton()
        {
            //CultureInfo culture = new CultureInfo(ConfigurationManager.AppSettings["DefaultCulture"]);
            //Thread.CurrentThread.CurrentCulture = culture;
            //Thread.CurrentThread.CurrentUICulture = culture;

        }
        public static CompilerSingleton Instance
        {
            get { return instance; }
        }

        #region Untrusted code helpers
        private bool ContainsUntrustedCodeC(string SourceFile, out string UntrustedCode)
        {
            bool result = false;
            UntrustedCode = String.Empty;

            List<string> untrustedCode = new List<string>()
                {
                    "\"(.*)shutdown(.*)\"",
                    "<(.*)windows(.*)>"
                };

            string source;
            using (StreamReader sr = new StreamReader(SourceFile))
            {
                source = RemoveComments(sr.ReadToEnd());
            }

            foreach (var codeStr in untrustedCode)
            {
                if (Regex.IsMatch(source, codeStr, RegexOptions.IgnoreCase))
                {
                    result = true;
                    UntrustedCode = codeStr;
                    break;
                }
            }

            return result;
        }
        private bool ContainsUntrustedCodeCPP(string SourceFile, out string UntrustedCode)
        {
            bool result = false;
            UntrustedCode = String.Empty;

            List<string> untrustedCode = new List<string>()
                {
                    "\"(.*)shutdown(.*)\"",
                    "<(.*)windows(.*)>"
                };

            string source;
            using (StreamReader sr = new StreamReader(SourceFile))
            {
                source = RemoveComments(sr.ReadToEnd());
            }

            foreach (var codeStr in untrustedCode)
            {
                if (Regex.IsMatch(source, codeStr, RegexOptions.IgnoreCase))
                {
                    result = true;
                    UntrustedCode = codeStr;
                    break;
                }
            }

            return result;
        }
        private bool ContainsUntrustedCodePascal(string SourceFile, out string UntrustedCode)
        {
            bool result = false;
            UntrustedCode = String.Empty;

            List<string> untrustedCode = new List<string>()
                {
                    "\'(.*)shutdown(.*)\'",
                    "uses(.*?)windows"
                };

            string source;
            using (StreamReader sr = new StreamReader(SourceFile))
            {
                source = RemoveComments(sr.ReadToEnd());
            }

            foreach (var codeStr in untrustedCode)
            {
                if (Regex.IsMatch(source, codeStr, RegexOptions.IgnoreCase | RegexOptions.Singleline))
                {
                    result = true;
                    UntrustedCode = codeStr;
                    break;
                }
            }

            return result;
        }
        private bool ContainsUntrustedCodeCS(string SourceFile, out string UntrustedCode)
        {
            bool result = false;
            UntrustedCode = String.Empty;

            List<string> untrustedCode = new List<string>()
                {
                    "\\[(.*)dllimport(.*)\\]"
                };

            string source;
            using (StreamReader sr = new StreamReader(SourceFile))
            {
                source = RemoveComments(sr.ReadToEnd());
            }

            foreach (var codeStr in untrustedCode)
            {
                if (Regex.IsMatch(source, codeStr, RegexOptions.IgnoreCase))
                {
                    result = true;
                    UntrustedCode = codeStr;
                    break;
                }
            }

            return result;
        }
        private bool ContainsUntrustedCodeVB(string SourceFile, out string UntrustedCode)
        {
            bool result = false;
            UntrustedCode = String.Empty;

            List<string> untrustedCode = new List<string>()
                {
                    " lib "
                };

            string source;
            using (StreamReader sr = new StreamReader(SourceFile))
            {
                source = RemoveComments(sr.ReadToEnd());
            }

            foreach (var codeStr in untrustedCode)
            {
                if (Regex.IsMatch(source, codeStr, RegexOptions.IgnoreCase))
                {
                    result = true;
                    UntrustedCode = codeStr;
                    break;
                }
            }

            return result;
        }
        private bool ContainsUntrustedCodeJava(string SourceFile, out string UntrustedCode)
        {
            bool result = false;
            UntrustedCode = String.Empty;

            List<string> untrustedCode = new List<string>()
                {
                    "\"(.*)shutdown(.*)\""
                };

            string source;
            using (StreamReader sr = new StreamReader(SourceFile))
            {
                source = RemoveComments(sr.ReadToEnd());
            }

            foreach (var codeStr in untrustedCode)
            {
                if (Regex.IsMatch(source, codeStr, RegexOptions.IgnoreCase))
                {
                    result = true;
                    UntrustedCode = codeStr;
                    break;
                }
            }

            return result;
        }
        #endregion

        private bool ContainsUntrustedCode(string SourceFile, ProgrammingLanguages PL, out string UntrustedCode)
        {
            // TODO: Add lexical analyzer

            bool result = false;
            UntrustedCode = String.Empty;
            //return result;

            switch (PL)
            {
                case ProgrammingLanguages.C:
                    result = ContainsUntrustedCodeC(SourceFile, out UntrustedCode);
                    break;
                case ProgrammingLanguages.CPP:
                    result = ContainsUntrustedCodeCPP(SourceFile, out UntrustedCode);
                    break;
                case ProgrammingLanguages.Pascal:
                    result = ContainsUntrustedCodePascal(SourceFile, out UntrustedCode);
                    break;
                case ProgrammingLanguages.CS:
                    result = ContainsUntrustedCodeCS(SourceFile, out UntrustedCode);
                    break;
                case ProgrammingLanguages.VB:
                    result = ContainsUntrustedCodeVB(SourceFile, out UntrustedCode);
                    break;
                case ProgrammingLanguages.Java:
                    result = ContainsUntrustedCodeJava(SourceFile, out UntrustedCode);
                    break;
            }

            return result;
        }

        private void CompileC(string SourceFile, string OutputFile, string WorkingDirectory = null)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.FileName = Compilers.GetPath(ProgrammingLanguages.C);
            startInfo.Arguments = Compilers.GetOptions(ProgrammingLanguages.C) + " \"" + SourceFile + "\" -o \"" + OutputFile + "\"";
            startInfo.CreateNoWindow = true;
            if (WorkingDirectory != null)
                startInfo.WorkingDirectory = WorkingDirectory;

            string error = "";
            using (Process proc = Process.Start(startInfo))
            {
                error += proc.StandardError.ReadToEnd();
                error += proc.StandardOutput.ReadToEnd();

                proc.WaitForExit();

                logger.Debug("Compilation of file \"{0}\" end", Path.GetFileName(SourceFile));
                if (logger.IsDebugEnabled)
                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Compilation of file \"{0}\" end", Path.GetFileName(SourceFile));

                if (proc.ExitCode != 0)
                {
                    throw new InvalidDataException(error);
                }
            }
        }

        private void CompileCPP(string SourceFile, string OutputFile, string WorkingDirectory = null)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.FileName = Compilers.GetPath(ProgrammingLanguages.CPP);
            startInfo.Arguments = Compilers.GetOptions(ProgrammingLanguages.CPP) + " \"" + SourceFile + "\" -o \"" + OutputFile + "\"";
            startInfo.CreateNoWindow = true;
            if (WorkingDirectory != null)
                startInfo.WorkingDirectory = WorkingDirectory;

            string error = "";
            using (Process proc = Process.Start(startInfo))
            {
                error += proc.StandardError.ReadToEnd();
                error += proc.StandardOutput.ReadToEnd();

                proc.WaitForExit();

                logger.Debug("Compilation of file \"{0}\" end", Path.GetFileName(SourceFile));
                if (logger.IsDebugEnabled)
                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Compilation of file \"{0}\" end", Path.GetFileName(SourceFile));

                if (proc.ExitCode != 0)
                {
                    throw new InvalidDataException(error);
                }
            }
        }
        private void CompileVCPP(string SourceFile, string OutputFile, string WorkingDirectory = null)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.FileName = Compilers.GetPath(ProgrammingLanguages.VCPP);
            startInfo.Arguments = Compilers.GetOptions(ProgrammingLanguages.VCPP) + " \"" + SourceFile + "\" /Fe\"" + OutputFile + "\"";
            startInfo.CreateNoWindow = true;
            if (WorkingDirectory != null)
                startInfo.WorkingDirectory = WorkingDirectory;

            string error = "";
            using (Process proc = Process.Start(startInfo))
            {
                error += proc.StandardError.ReadToEnd();
                error += proc.StandardOutput.ReadToEnd();

                proc.WaitForExit();

                logger.Debug("Compilation of file \"{0}\" end", Path.GetFileName(SourceFile));
                if (logger.IsDebugEnabled)
                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Compilation of file \"{0}\" end", Path.GetFileName(SourceFile));

                if (proc.ExitCode != 0)
                {
                    throw new InvalidDataException(error);
                }
            }
        }

        private void CompileCS(string SourceFile, string OutputFile, string WorkingDirectory = null)
        {
            CodeDomProvider codeProvider =
                CodeDomProvider.CreateProvider("CSharp");
            
            CompilerParameters parameters = new CompilerParameters();
            //Make sure we generate an EXE, not a DLL
            parameters.GenerateExecutable = true;
            parameters.GenerateInMemory = false;
            parameters.OutputAssembly = OutputFile;
            CompilerResults results =
                codeProvider.CompileAssemblyFromFile(parameters, SourceFile);

            logger.Debug("Compilation of file \"{0}\" end", Path.GetFileName(SourceFile));
            if (logger.IsDebugEnabled)
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Compilation of file \"{0}\" end", Path.GetFileName(SourceFile));

            if (results.Errors.Count > 0)
            {
                string error = "";
                results.Errors.Each(e => error += e.ToString() + "\n");

                throw new InvalidDataException(error);
            }
        }

        private void CompileVB(string SourceFile, string OutputFile, string WorkingDirectory = null)
        {
            CodeDomProvider codeProvider = 
                CodeDomProvider.CreateProvider("VB");

            CompilerParameters parameters = new CompilerParameters();
            //Make sure we generate an EXE, not a DLL
            parameters.GenerateExecutable = true;
            parameters.GenerateInMemory = false;
            parameters.OutputAssembly = OutputFile;
            CompilerResults results =
                codeProvider.CompileAssemblyFromFile(parameters, SourceFile);

            logger.Debug("Compilation of file \"{0}\" end", Path.GetFileName(SourceFile));
            if (logger.IsDebugEnabled)
                Console.WriteLine(DateTime.Now.ToString(culture) + " - Compilation of file \"{0}\" end", Path.GetFileName(SourceFile));

            if (results.Errors.Count > 0)
            {
                string error = "";
                results.Errors.Each(e => error += e.ToString() + "\n");

                throw new InvalidDataException(error);
            }
        }

        private void CompileJava(string SourceFile, string OutputFile, string WorkingDirectory = null)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.FileName = Compilers.GetPath(ProgrammingLanguages.Java);
            startInfo.Arguments = "\"" + SourceFile + "\" -d \"" + Path.GetDirectoryName(OutputFile) + "\"";
            startInfo.CreateNoWindow = true;
            if (WorkingDirectory != null)
                startInfo.WorkingDirectory = WorkingDirectory;

            string error = "";
            using (Process proc = Process.Start(startInfo))
            {
                error += proc.StandardError.ReadToEnd();
                error += proc.StandardOutput.ReadToEnd();

                proc.WaitForExit();

                logger.Debug("Compilation of file \"{0}\" end", Path.GetFileName(SourceFile));
                if (logger.IsDebugEnabled)
                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Compilation of file \"{0}\" end", Path.GetFileName(SourceFile));

                if (proc.ExitCode != 0)
                {
                    throw new InvalidDataException(error);
                }
            }
        }

        private void CompilePascal(string SourceFile, string OutputFile, ProgrammingLanguages PL, string WorkingDirectory = null)
        {
            if (PL != ProgrammingLanguages.Pascal &&
                PL != ProgrammingLanguages.Delphi &&
                PL != ProgrammingLanguages.ObjPas &&
                PL != ProgrammingLanguages.TurboPas)
                throw new NotSupportedException();

            ProcessStartInfo startInfo = new ProcessStartInfo();

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.FileName = Compilers.GetPath(PL);
            startInfo.Arguments = Compilers.GetOptions(PL) + " \"" + SourceFile + "\" -o\"" + OutputFile + "\"";
            startInfo.CreateNoWindow = true;
            if (WorkingDirectory != null)
                startInfo.WorkingDirectory = WorkingDirectory;

            string error = "";
            using (Process proc = Process.Start(startInfo))
            {
                error += proc.StandardError.ReadToEnd();
                error += proc.StandardOutput.ReadToEnd();

                proc.WaitForExit();

                logger.Debug("Compilation of file \"{0}\" end", Path.GetFileName(SourceFile));
                if (logger.IsDebugEnabled)
                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Compilation of file \"{0}\" end", Path.GetFileName(SourceFile));

                if (proc.ExitCode != 0)
                {
                    throw new InvalidDataException(error);
                }
            }
        }

        private void CompilePython(string SourceFile, string OutputFile, string WorkingDirectory = null)
        {
            File.Copy(SourceFile,
                Path.Combine(Path.GetDirectoryName(OutputFile), Path.GetFileName(SourceFile)), true);
        }

        public void Compile(string SourceFile, string OutputFile, ProgrammingLanguages PL, out TestResults Result, int SolutionID = -1, string WorkingDirectory = null)
        {
            if (!AvailablePL.Contains(PL))
            {
                throw new NotSupportedException("Compiler " + PL.ToString() + " not supported");
            }

            if (PL != ProgrammingLanguages.Python)
            {
                logger.Debug("Begin compiling solution {0} \"{1}\"", SolutionID, Path.GetFileName(SourceFile));
                if (logger.IsDebugEnabled)
                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Begin compiling solution {0} \"{1}\"", SolutionID, Path.GetFileName(SourceFile));
            }

            if (File.Exists(OutputFile))
            {
                File.Delete(OutputFile);
            }

            string untrustedCode;
            if (ContainsUntrustedCode(SourceFile, PL, out untrustedCode))
            {
                logger.Debug("Solution {0} \"{1}\" contains untrusted code: {2}", SolutionID, Path.GetFileName(SourceFile), untrustedCode);
                if (logger.IsDebugEnabled)
                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Solution {0} \"{1}\" contains untrusted code: {2}", SolutionID, Path.GetFileName(SourceFile), untrustedCode);

                Result = TestResults.CE;
                return;
            }

            // TODO: FL
            Result = TestResults.RTE;

            try
            {
                switch (PL)
                {
                    case ProgrammingLanguages.C:
                        CompileC(SourceFile, OutputFile, WorkingDirectory);
                        break;
                    case ProgrammingLanguages.CPP:
                        CompileCPP(SourceFile, OutputFile, WorkingDirectory);
                        break;
                    case ProgrammingLanguages.VCPP:
                        CompileCPP(SourceFile, OutputFile, WorkingDirectory);
                        break;
                    case ProgrammingLanguages.CS:
                        CompileCS(SourceFile, OutputFile, WorkingDirectory);
                        break;
                    case ProgrammingLanguages.VB:
                        CompileVB(SourceFile, OutputFile, WorkingDirectory);
                        break;
                    case ProgrammingLanguages.Java:
                        CompileJava(SourceFile, OutputFile, WorkingDirectory);
                        break;
                    case ProgrammingLanguages.Pascal:
                    case ProgrammingLanguages.Delphi:
                    case ProgrammingLanguages.ObjPas:
                    case ProgrammingLanguages.TurboPas:
                        CompilePascal(SourceFile, OutputFile, PL, WorkingDirectory);
                        break;
                    case ProgrammingLanguages.Python:
                        CompilePython(SourceFile, OutputFile, WorkingDirectory);
                        break;
                    default:
                        throw new NotSupportedException("Compiler not found in case");
                }
            }
            catch (InvalidDataException ex)
            {
                logger.Debug("Compiling solution {0} \"{1}\" fail: {2}", SolutionID, Path.GetFileName(SourceFile), ex.Message);
                if (logger.IsDebugEnabled)
                    Console.WriteLine(DateTime.Now.ToString(culture) + " - Compiling solution {0} \"{1}\" fail: {2}", SolutionID, Path.GetFileName(SourceFile), ex.Message);

                Result = TestResults.CE;
                return;
            }

            Result = TestResults.OK;
        }
    }
}
