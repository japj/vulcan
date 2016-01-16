/*
 * Microsoft Public License (Ms-PL)
 * 
 * This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.
 * 
 * 1. Definitions
 * The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
 * A "contribution" is the original software, or any additions or changes to the software.
 * A "contributor" is any person that distributes its contribution under this license.
 * "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 * 
 * 2. Grant of Rights
 * (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 * (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 * 
 * 3. Conditions and Limitations
 * (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
 * (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
 * (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
 * (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
 * (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement. 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Vulcan.Common;
using Vulcan.Tasks;
using Vulcan.Common.Templates;
using Vulcan.Emitters;
using Vulcan.Properties;

using DTS = Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;

using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace Vulcan.Patterns
{
    public static class PatternFactory
    {
        public static Pattern ProcessPattern(Packages.VulcanPackage vulcanPackage, DTS.IDTSSequence parentContainer, XPathNavigator patternNavigator, Pattern previousPattern)
        {
            DTS.Executable lastExecutableInPattern = null;
            if (previousPattern != null)
            {
                lastExecutableInPattern = previousPattern.FirstExecutableGeneratedByPattern;
            }
            Pattern p = null;
            Message.Trace(Severity.Debug,"Emitting Pattern {0}", patternNavigator.Name);

            try
            {
                switch (patternNavigator.Name)
                {
                    case "HelperTables":
                        p = new HelperTablePattern(vulcanPackage, parentContainer);
                        p.Emit(patternNavigator);
                        break;
                    case "ETL":
                        p = new ETLPattern(vulcanPackage, parentContainer);
                        p.Emit(patternNavigator);
                        break;
                    case "StoredProc":
                        p = new StoredProcPattern(vulcanPackage, parentContainer);
                        p.Emit(patternNavigator);
                        break;
                    case "Dimensions":
                        p = new DimensionsPattern(vulcanPackage, parentContainer);
                        p.Emit(patternNavigator);
                        break;
                    case "Logtainer":
                        p = new LogtainerPattern(vulcanPackage, parentContainer);
                        p.Emit(patternNavigator);
                        break;
                    case "LogUpdate":
                        p = new LogUpdatePattern(vulcanPackage, parentContainer);
                        p.Emit(patternNavigator);
                        break;
                    case "Container":
                        p = new ContainerPattern(vulcanPackage, parentContainer);
                        p.Emit(patternNavigator);
                        break;
                    case "FileSystemTask":
                        p = new FileSystemTaskPattern(vulcanPackage, parentContainer);
                        p.Emit(patternNavigator);
                        break;
                    case "ExecuteSQL":
                        p = new ExecuteSQLPattern(vulcanPackage, parentContainer);
                        p.Emit(patternNavigator);
                        break;
                    case "FactTable":
                        p = new FactTablePattern(vulcanPackage, parentContainer);
                        p.Emit(patternNavigator);
                        break;
                    case "ExecutePackage":
                        p = new ExecutePackagePattern(vulcanPackage, parentContainer);
                        p.Emit(patternNavigator);
                        break;
                    case "FTPTask":
                        p = new FTPTaskPattern(vulcanPackage, parentContainer);
                        p.Emit(patternNavigator);
                        break;
                    default:
                        break;
                }
            }
            catch (System.Runtime.InteropServices.COMException ce)
            {
                Message.Trace(Severity.Error,ce,"Exception in Pattern {0}\n {1}\n", patternNavigator.Name, ce.Message);
            }
            catch (Exception e)
            {
                Message.Trace(Severity.Error,e,"Exception in Pattern {0}\n {1}\n", patternNavigator.Name, e.Message);
            }

            return p;
        }
    }
}
