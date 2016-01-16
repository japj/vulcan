using System;
using System.IO;
using System.Reflection;

namespace Vulcan.Utility.Files
{
    public static class PathManager
    {
        private static string _targetPath;

        public static string TargetPath
        {
            get { return _targetPath; }
            set { _targetPath = Path.GetFullPath(value); }
        }

        public static string ToolPath
        {
            get { return Path.GetDirectoryName(Assembly.GetCallingAssembly().Location); }
        }

        public static string GetToolSubpath(string relativeSubpath)
        {
            return AddSubpath(ToolPath, relativeSubpath);
        }

        public static string GetTargetSubpath(string relativeSubpath)
        {
            return AddSubpath(TargetPath, relativeSubpath);
        }

        public static string AddSubpath(string path, string relativeSubpath)
        {
            if (relativeSubpath == null)
            {
                relativeSubpath = String.Empty;
            }

            return Path.Combine(path, relativeSubpath);
        }
    }
}
