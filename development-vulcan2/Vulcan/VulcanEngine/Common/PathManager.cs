using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Globalization;

namespace VulcanEngine.Common
{
    public static class PathManager
    {
        public static string TargetPath { get; set; }
        /// TODO: vsabella: This is a really 
        /// cheap hack / placeholder until we can do true settings override 
        /// as phase parameters.
        /// Using the .config files is broken right now since it's a pain for users to edit
        /// blah.config and merge them, given the fact that we have VulcanEngine.dll and Vulcan.exe
        public static string PackageConfigurationPath
        {
            get
            {
                return System.Environment.GetEnvironmentVariable("VULCAN_PACKAGECONFIGROOT");
            }
        }

        public static string GetToolPath()
        {
            return Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
        }

        public static string GetTargetPath()
        {
            return TargetPath;
        }

        public static string GetToolSubpath(string relativeSubpath)
        {
            return String.Format(CultureInfo.CurrentCulture, "{0}{1}{2}", Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), Path.DirectorySeparatorChar, relativeSubpath);
        }

        public static string GetTargetSubpath(string relativeSubpath)
        {
            return String.Format(CultureInfo.CurrentCulture, "{0}{1}{2}", TargetPath, Path.DirectorySeparatorChar, relativeSubpath);
        }

        public static string AddSubpath(string path, string relativeSubpath)
        {
            return String.Format(CultureInfo.CurrentCulture, "{0}{1}{2}", path, Path.DirectorySeparatorChar, relativeSubpath);
        }
    }
}
