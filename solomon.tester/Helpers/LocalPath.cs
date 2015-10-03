using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solomon.Tester.Helpers
{
    /// <summary>
    /// Server path to directories.
    /// </summary>
    public static class LocalPath
    {
        private static string solutionsDirectory;
        private static string relativeSolutionsDirectory = "Solutions\\";
        private static string problemsDirectory;
        private static string relativeProblemsDirectory = "Problems\\";
        private static string compilerOptionsDirectory;
        private static string relativeCompilerOptionsDirectory = "CompilerOptions\\";
        private static string[] relativeTestlibCPP = new string[] { "Testlib\\testlib.h" };
        private static string[] testlibCPP = new string[relativeTestlibCPP.Length];
        private static string[] relativeTestlibPAS = new string[] { "Testlib\\testlib.pas", "Testlib\\symbols.pas" };
        private static string[] testlibPAS = new string[relativeTestlibPAS.Length];

        static LocalPath()
        {
            problemsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativeProblemsDirectory);
            solutionsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativeSolutionsDirectory);
            compilerOptionsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativeCompilerOptionsDirectory);

            for (int i = 0; i < testlibCPP.Length; i++)
            {
                testlibCPP[i] = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativeTestlibCPP[i]);
            }
            for (int i = 0; i < testlibPAS.Length; i++)
            {
                testlibPAS[i] = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativeTestlibPAS[i]);
            }

            if (!Directory.Exists(ProblemsDirectory))
                Directory.CreateDirectory(ProblemsDirectory);

            if (!Directory.Exists(SolutionsDirectory))
                Directory.CreateDirectory(SolutionsDirectory);

            if (!Directory.Exists(CompilerOptionsDirectory))
                Directory.CreateDirectory(CompilerOptionsDirectory);
        }

        public static string ProblemsDirectory
        {
            get
            {
                return problemsDirectory;
            }
        }
        public static string RelativeProblemsDirectory
        {
            get
            {
                return relativeProblemsDirectory;
            }
        }
        public static string SolutionsDirectory
        {
            get
            {
                return solutionsDirectory;
            }
        }
        public static string RelativeSolutionsDirectory
        {
            get
            {
                return relativeSolutionsDirectory;
            }
        }
        public static string CompilerOptionsDirectory
        {
            get
            {
                return compilerOptionsDirectory;
            }
        }
        public static string RelativeCompilerOptionsDirectory
        {
            get
            {
                return relativeCompilerOptionsDirectory;
            }
        }
        public static string[] TestlibCPP
        {
            get
            {
                return testlibCPP;
            }
        }
        public static string[] RelativeTestlibCPP
        {
            get
            {
                return relativeTestlibCPP;
            }
        }

        public static string[] TestlibPAS
        {
            get
            {
                return testlibPAS;
            }
        }
        public static string[] RelativeTestlibPAS
        {
            get
            {
                return relativeTestlibPAS;
            }
        }
    }
}
