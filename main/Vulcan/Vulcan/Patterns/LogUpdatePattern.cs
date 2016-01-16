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
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;


using Vulcan.Common;
using Vulcan.Tasks;
using Vulcan.Common.Templates;

using Vulcan.Emitters;
using Vulcan.Properties;


using DTS = Microsoft.SqlServer.Dts.Runtime;
using DTSTasks = Microsoft.SqlServer.Dts.Tasks;


namespace Vulcan.Patterns
{

    public class LogUpdatePattern : Pattern
    {
        public LogUpdatePattern(Vulcan.Packages.VulcanPackage vulcanPackage, DTS.IDTSSequence parentContainer)
            :
            base(
            vulcanPackage,
            parentContainer
            )
        {
        }

        public override void Emit(XPathNavigator patternNavigator)
        {
            if (patternNavigator != null)
            {
                string firstOrLast = patternNavigator.SelectSingleNode("@FirstOrLast").Value;
                string status = patternNavigator.SelectSingleNode("@Status").Value;
                string notes = patternNavigator.SelectSingleNode("@Notes").Value;

                Connection tableConnection =
                    Connection.GetExistingConnection(VulcanPackage, LogtainerPattern.CurrentLog.TableConnectionName);

                string execSqlTaskName = LogtainerPattern.CurrentLog.LogName + Resources.Seperator + firstOrLast + Guid.NewGuid();
                TemplateEmitter te = new TemplateEmitter(VulcanPackage.TemplateManager["LogSelectQuery"]);
                if (
                    LogtainerPattern.CurrentLog.SourceColumn == null ||
                    LogtainerPattern.CurrentLog.DestinationColumn == null ||
                    LogtainerPattern.CurrentLog.TableConnectionName == null ||
                    LogtainerPattern.CurrentLog.Table == null)
                {
                    Message.Trace(Severity.Error,
                        "Could not perform LogUpdate (On Log: {0}), Parent Logtainer does not contain all of the necessary information.  Needs SourceColumn, DestinationColumn, TableConnectionName, and Table attributes.",LogtainerPattern.CurrentLog.LogName);
                    return;
                }

                te.SetNamedParameter("Source", LogtainerPattern.CurrentLog.SourceColumn);
                te.SetNamedParameter("Destination", LogtainerPattern.CurrentLog.DestinationColumn);
                te.SetNamedParameter("Table", LogtainerPattern.CurrentLog.Table);
                te.SetNamedParameter("Status", status);
                te.SetNamedParameter("Notes", notes);
                te.SetNamedParameter("SourceConvertStyle", "21");
                te.SetNamedParameter("DestinationConvertStyle", "21");

                string query;
                te.Emit(out query);

                SQLTask readForLogTask = new SQLTask(VulcanPackage, execSqlTaskName, execSqlTaskName, ParentContainer, tableConnection);
                readForLogTask.TransmuteToExpressionTask(String.Format("\"{0}\"", query));
                readForLogTask.ExecuteSQLTask.ResultSetType = Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ResultSetType.ResultSetType_SingleRow;

                DTS.Variable sourceVar = LogtainerPattern.CurrentLog[firstOrLast+"SourceRecord"];
                DTS.Variable destVar = LogtainerPattern.CurrentLog[firstOrLast+"DestinationRecord"];
                DTS.Variable statusVar = LogtainerPattern.CurrentLog["Status"];
                DTS.Variable notesVar = LogtainerPattern.CurrentLog["Notes"];

                readForLogTask.BindResult("0", sourceVar.QualifiedName);
                readForLogTask.BindResult("1", destVar.QualifiedName);
                readForLogTask.BindResult("2", statusVar.QualifiedName);
                readForLogTask.BindResult("3", notesVar.QualifiedName);

                this.FirstExecutableGeneratedByPattern = readForLogTask.SQLTaskHost;
                this.LastExecutableGeneratedByPattern = this.FirstExecutableGeneratedByPattern;
            }
        }
    }
}