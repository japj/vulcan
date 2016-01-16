using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using DTS = Microsoft.SqlServer.Dts.Runtime;

using VulcanEngine.Common;
using Ssis2008Emitter.Properties;
using Ssis2008Emitter.Emitters.Framework;

namespace Ssis2008Emitter.Emitters.Common
{
    public class SSISExpressionPathBuilder
    {
        private static string escapeBackslashes(string path)
        {
            return path.Replace(Path.DirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString());
        }

        public static string BuildAbsoluteExpressionPath(string path)
        {
            path = path.Trim();
            if (path.StartsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                path = path.Remove(0, 1);
            }
            if (path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                path = path.Remove(path.Length - 1, 1);
            }

            path = escapeBackslashes(path);

            return path;
        }
        public static string BuildExpressionPath(string relativePath)
        {
            relativePath = relativePath.Trim();

            if (relativePath.StartsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                relativePath = relativePath.Remove(0, 1);
            }
            if (relativePath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                relativePath = relativePath.Remove(relativePath.Length - 1, 1);
            }

            relativePath = escapeBackslashes(relativePath);
            relativePath = String.Format("@[User::_vulcanPackageRepositoryDirectory] + \"{0}\"", relativePath);

            return relativePath;
        }
    }
}