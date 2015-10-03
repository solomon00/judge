using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Solomon.WebUI.Helpers
{
    /// <summary>
    /// Server path to directories.
    /// </summary>
    public static class LocalPath
    {
        private static string relativeProblemsDirectory;
        private static string absoluteProblemsDirectory;
        private static string relativeProblemsAttachDirectory;
        private static string relativeSolutionsDirectory;
        private static string absoluteSolutionsDirectory;
        private static string relativeCheckersDirectory;
        private static string absoluteCheckersDirectory;
        private static string relativeTestersDirectory;
        private static string absoluteTestersDirectory;
        private static string relativeGeneratedAccountsDirectory;
        private static string absoluteGeneratedAccountsDirectory;
        private static string relativeUploadsDirectory;
        private static string absoluteUploadsDirectory;
        private static string relativeCompilerOptionsDirectory;
        private static string absoluteCompilerOptionsDirectory;
        private static string[] absoluteLogFileDirectory;
        private static string relativeTempDirectory;
        private static string absoluteTempDirectory;
        private static string rootDirectory;

        static LocalPath()
        {
            rootDirectory = Path.Combine(HttpRuntime.AppDomainAppPath, "App_Data");

            relativeProblemsDirectory = "Problems\\";
            absoluteProblemsDirectory = Path.Combine(rootDirectory, relativeProblemsDirectory);

            relativeProblemsAttachDirectory = "Attach\\";

            relativeSolutionsDirectory = "Solutions\\";
            absoluteSolutionsDirectory = Path.Combine(rootDirectory, relativeSolutionsDirectory);

            relativeCheckersDirectory = "Checkers\\";
            absoluteCheckersDirectory = Path.Combine(rootDirectory, relativeCheckersDirectory);

            relativeTestersDirectory = "Testers\\";
            absoluteTestersDirectory = Path.Combine(rootDirectory, relativeTestersDirectory);

            relativeGeneratedAccountsDirectory = "GeneratedAccounts\\";
            absoluteGeneratedAccountsDirectory = Path.Combine(rootDirectory, relativeGeneratedAccountsDirectory);

            relativeUploadsDirectory = "Uploads\\";
            absoluteUploadsDirectory = Path.Combine(HttpRuntime.AppDomainAppPath, relativeUploadsDirectory);

            relativeCompilerOptionsDirectory = "CompilerOptions\\";
            absoluteCompilerOptionsDirectory = Path.Combine(rootDirectory, relativeCompilerOptionsDirectory);

            var rootAppender = ((Hierarchy)LogManager.GetRepository())
                .Root.Appenders.OfType<FileAppender>();
            absoluteLogFileDirectory = rootAppender.Select(a => Path.GetDirectoryName(a.File)).ToArray();

            relativeTempDirectory = "Temp\\";
            absoluteTempDirectory = Path.Combine(rootDirectory, relativeTempDirectory);

            if (!Directory.Exists(absoluteProblemsDirectory))
                Directory.CreateDirectory(absoluteProblemsDirectory);
            if (!Directory.Exists(absoluteSolutionsDirectory))
                Directory.CreateDirectory(absoluteSolutionsDirectory);
            if (!Directory.Exists(absoluteCheckersDirectory))
                Directory.CreateDirectory(absoluteCheckersDirectory);
            if (!Directory.Exists(absoluteTestersDirectory))
                Directory.CreateDirectory(absoluteTestersDirectory);
            if (!Directory.Exists(absoluteGeneratedAccountsDirectory))
                Directory.CreateDirectory(absoluteGeneratedAccountsDirectory);
            if (!Directory.Exists(absoluteUploadsDirectory))
                Directory.CreateDirectory(absoluteUploadsDirectory);
            if (!Directory.Exists(absoluteCompilerOptionsDirectory))
                Directory.CreateDirectory(absoluteCompilerOptionsDirectory);
            if (!Directory.Exists(absoluteTempDirectory))
                Directory.CreateDirectory(absoluteTempDirectory);
        }

        public static string RootDirectory
        {
            get
            {
                return rootDirectory;
            }
        }

        public static string RelativeProblemsDirectory
        {
            get
            {
                return relativeProblemsDirectory;
            }
        }

        public static string AbsoluteProblemsDirectory
        {
            get
            {
                return absoluteProblemsDirectory;
            }
        }

        public static string RelativeProblemsAttachDirectory
        {
            get
            {
                return relativeProblemsAttachDirectory;
            }
        }
        
        public static string RelativeSolutionsDirectory
        {
            get
            {
                return relativeSolutionsDirectory;
            }
        }

        public static string AbsoluteSolutionsDirectory
        {
            get
            {
                return absoluteSolutionsDirectory;
            }
        }

        public static string RelativeCheckersDirectory
        {
            get
            {
                return relativeCheckersDirectory;
            }
        }

        public static string AbsoluteCheckersDirectory
        {
            get
            {
                return absoluteCheckersDirectory;
            }
        }

        public static string RelativeTestersDirectory
        {
            get
            {
                return relativeTestersDirectory;
            }
        }

        public static string AbsoluteTestersDirectory
        {
            get
            {
                return absoluteTestersDirectory;
            }
        }

        public static string RelativeGeneratedAccountsDirectory
        {
            get
            {
                return relativeGeneratedAccountsDirectory;
            }
        }

        public static string AbsoluteGeneratedAccountsDirectory
        {
            get
            {
                return absoluteGeneratedAccountsDirectory;
            }
        }

        public static string RelativeUploadsDirectory
        {
            get
            {
                return relativeUploadsDirectory;
            }
        }

        public static string AbsoluteUploadsDirectory
        {
            get
            {
                return absoluteUploadsDirectory;
            }
        }

        public static string RelativeCompilerOptionsDirectory
        {
            get
            {
                return relativeCompilerOptionsDirectory;
            }
        }

        public static string AbsoluteCompilerOptionsDirectory
        {
            get
            {
                return absoluteCompilerOptionsDirectory;
            }
        }

        public static string RelativeTempDirectory
        {
            get
            {
                return relativeTempDirectory;
            }
        }

        public static string AbsoluteTempDirectory
        {
            get
            {
                return absoluteTempDirectory;
            }
        }

        public static string[] AbsoluteLogFileDirectory
        {
            get
            {
                return absoluteLogFileDirectory; 
            }
        }

    }
}