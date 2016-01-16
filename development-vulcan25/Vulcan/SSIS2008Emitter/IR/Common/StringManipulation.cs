using System;
using System.Globalization;
using System.IO;
using Vulcan.Utility.Files;
using VulcanEngine.Common;

namespace Ssis2008Emitter.IR.Common
{
    public static class StringManipulation
    {
        private static string EscapeBackslashes(string path)
        {
            return path.Replace(Path.DirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar + Path.DirectorySeparatorChar.ToString());
        }

        public static string NameCleaner(string name)
        {
            return name.Replace(".", "_").Replace(@"\", "|").Replace("[", String.Empty).Replace("]", String.Empty);
        }

        public static string NameCleanerAndUniqifier(string name)
        {
             return NameCleaner(name) + "_" + Guid.NewGuid().ToString("N");
        }

        public static string CleanPath(string path)
        {
            path = path.Trim();
            path = Path.GetFullPath(path);
            if (path.StartsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                path = path.Remove(0, 1);
            }

            if (path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                path = path.Remove(path.Length - 1, 1);
            }

            return path;
        }

        public static string BuildExpressionPath(string path)
        {
            path = PathManager.GetTargetSubpath(path);
            var subPath = EscapeBackslashes(path.Replace(PathManager.TargetPath, String.Empty));
            path = String.Format(CultureInfo.InvariantCulture, "@[User::_ssisRootDir] + \"{0}\"", subPath);

            return path;
        }
    }
}