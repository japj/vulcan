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
    public class LogtainerLog
    {
        private Vulcan.Packages.VulcanPackage _vulcanPackage;

        private string _logName;
        private DTS.Variable _logVariable = null;
        private DTS.Variable _lastRecordLogVariable = null;
        private DTS.Variable _isAnotherInstanceCurrentlyRunningLogVariable = null;

        private Dictionary<string, DTS.Variable> _columnVariableDictionary;

        private string _sourceColumn;
        private string _destinationColumn;
        private string _table;
        private string _tableConnectionName;

        public LogtainerLog(Packages.VulcanPackage vulcanPackage, string logName, string sourceColumn, string destinationColumn, string table, string tableConnectionName)
        {
            _sourceColumn = sourceColumn;
            _destinationColumn = destinationColumn;
            _table = table;
            _tableConnectionName = tableConnectionName;

            _vulcanPackage = vulcanPackage;
            _logName = logName;
            _columnVariableDictionary = new Dictionary<string, Microsoft.SqlServer.Dts.Runtime.Variable>();
            _logVariable = vulcanPackage.AddVariable(logName, (System.Int32)(-1));
            _lastRecordLogVariable = vulcanPackage.AddVariable(logName + Resources.LastSuccessfulRunLogID, (System.Int32)(-1));
            _isAnotherInstanceCurrentlyRunningLogVariable = vulcanPackage.AddVariable(logName + Resources.IsAnotherInstanceCurrentlyRunningLogID, (System.Int32)0);
        }

        public string SourceColumn
        {
            get
            {
                return _sourceColumn;
            }
        }

        public string DestinationColumn
        {
            get
            {
                return _destinationColumn;
            }
        }

        public string Table
        {
            get
            {
                return _table;
            }
        }

        public string TableConnectionName
        {
            get
            {
                return _tableConnectionName;
            }
        }

        public string LogName
        {
            get
            {
                return _logName;
            }
        }

        public DTS.Variable this[string columnName]
        {
            get
            {
                if (_columnVariableDictionary.ContainsKey(columnName.ToUpperInvariant()))
                {
                    return _columnVariableDictionary[columnName.ToUpperInvariant()];
                }
                else
                {
                    _columnVariableDictionary[columnName.ToUpperInvariant()] = _vulcanPackage.AddVariable(_logName + columnName, String.Empty);
                    return _columnVariableDictionary[columnName.ToUpperInvariant()];
                }
            }
        }

        public Dictionary<string, DTS.Variable> LogColumnDictionary
        {
            get
            {
                return _columnVariableDictionary;
            }
        }

        public DTS.Variable LogVariable
        {
            get
            {
                return _logVariable;
            }
        }

        public DTS.Variable LastRecordLogVariable
        {
            get
            {
                return _lastRecordLogVariable;
            }
        }

        public DTS.Variable IsAnotherInstanceCurrentlyRunningLogVariable
        {
            get
            {
                return _isAnotherInstanceCurrentlyRunningLogVariable;
            }
        }
    }

    public class LogtainerPattern : Pattern
    {
        private static Stack<LogtainerLog> _logStack = new Stack<LogtainerLog>();

        public static LogtainerLog CurrentLog
        {
            get
            {
                return _logStack.Peek();
            }
        }

        public LogtainerPattern(Vulcan.Packages.VulcanPackage vulcanPackage, DTS.IDTSSequence parentContainer)
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
                string taskName = patternNavigator.SelectSingleNode("@Name").Value;
                string connectionName = patternNavigator.SelectSingleNode("@Connection").Value;
                string varPrefix = patternNavigator.SelectSingleNode("@PreviousLogEntryVariablePrefix").Value;

                string sourceColumn = patternNavigator.SelectSingleNode("@SourceColumn") == null ? null : patternNavigator.SelectSingleNode("@SourceColumn").Value;
                string destinationColumn = patternNavigator.SelectSingleNode("@DestinationColumn") == null ? null : patternNavigator.SelectSingleNode("@DestinationColumn").Value;
                string table = patternNavigator.SelectSingleNode("@Table")== null ? null : patternNavigator.SelectSingleNode("@Table").Value;
                string tableConnectionName = patternNavigator.SelectSingleNode("@TableConnection") == null ? null : patternNavigator.SelectSingleNode("@TableConnection").Value;

                if (varPrefix.ToUpperInvariant() == "varLog")
                {
                    Message.Trace(Severity.Error, "Name: {0}: Error in PreviousLogEntryVariablePrefix: varLog is a reserved variable name.", taskName);
                    return;
                }

                Connection connection =
                    Connection.GetExistingConnection(VulcanPackage, connectionName);

                LogtainerLog ll = new LogtainerLog(
                                                    VulcanPackage, 
                                                    Resources.LogVariablePrefix + VulcanPackage.Name + taskName,
                                                    sourceColumn,
                                                    destinationColumn,
                                                    table,
                                                    tableConnectionName
                                                    );

                string execSqlTaskName = Resources.Log + Resources.Seperator + VulcanPackage.Name + Resources.Seperator + taskName + Resources.Seperator;
                SQLTask logStartTask = new SQLTask(VulcanPackage, execSqlTaskName + Resources.Start, execSqlTaskName + Resources.Start, ParentContainer, connection);


                TemplateEmitter te = new TemplateEmitter(VulcanPackage.TemplateManager["LogStart"]);
                te.SetNamedParameter("ETLName", VulcanPackage.Name);
                te.SetNamedParameter("TaskName", taskName);

                if (_logStack.Count > 0)
                {
                    te.SetNamedParameter("ParentLogID", "?");
                    // Yes you have to hard code the data type as there is no lookup or cross-reference source, and the intger is dependent on the connection you create... bugbug!
                    logStartTask.BindParameter(CurrentLog.LogVariable, Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Input, 3);
                }
                else
                {
                    te.SetNamedParameter("ParentLogID", "NULL");
                }
                logStartTask.BindParameter(ll.LogVariable, Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Output, 3);
                logStartTask.BindParameter(ll.LastRecordLogVariable, Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Output, 3);
                logStartTask.BindParameter(ll.IsAnotherInstanceCurrentlyRunningLogVariable, Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Output, 3);

                
                string sqlExpression;
                te.Emit(out sqlExpression);
                logStartTask.TransmuteToExpressionTask(sqlExpression);

                te = new TemplateEmitter(VulcanPackage.TemplateManager["LogGetValue"]);
                te.SetNamedParameter("LogID", ll.LastRecordLogVariable.QualifiedName);
                this.FirstExecutableGeneratedByPattern = logStartTask.SQLTaskHost;



                // Push the current log variable onto the stack!
                _logStack.Push(ll);

                //Add a new SQL Task to read the  previous log
                te = new TemplateEmitter("LogGetValue", VulcanPackage, ll.LastRecordLogVariable.QualifiedName);
                string readPreviousLogValuesQuery;
                te.Emit(out readPreviousLogValuesQuery);

                SQLTask readPreviousLogValues = new SQLTask(VulcanPackage, Resources.Log + Resources.LoadInto + varPrefix, Resources.Log + Resources.LoadInto + varPrefix, ParentContainer,connection);
                readPreviousLogValues.TransmuteToExpressionTask(readPreviousLogValuesQuery);
                VulcanPackage.AddPrecedenceConstraint(this.FirstExecutableGeneratedByPattern, readPreviousLogValues.SQLTaskHost, ParentContainer);

                //Bind and create variables for the SQL Task that reads the previous log. This is kinda hacky
                readPreviousLogValues.BindParameter(
                    VulcanPackage.AddVariable(varPrefix + "StartTime", new DateTime(1980,1,1)),
                    Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Output,
                    (int)System.Data.OleDb.OleDbType.DBTimeStamp,
                    255);
                readPreviousLogValues.BindParameter(
                    VulcanPackage.AddVariable(varPrefix + "EndTime", new DateTime(1980,1,1)),
                    Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Output,
                    (int)System.Data.OleDb.OleDbType.DBTimeStamp,
                    255);

                readPreviousLogValues.BindParameter(
                    VulcanPackage.AddVariable(varPrefix + "FirstSourceRecord", String.Empty),
                    Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Output,
                    (int)System.Data.OleDb.OleDbType.WChar,
                    255);
                readPreviousLogValues.BindParameter(
                    VulcanPackage.AddVariable(varPrefix + "FirstDestinationRecord", String.Empty),
                    Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Output,
                    (int)System.Data.OleDb.OleDbType.WChar,
                    255);
                readPreviousLogValues.BindParameter(
                    VulcanPackage.AddVariable(varPrefix + "LastSourceRecord", String.Empty),
                    Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Output,
                    (int)System.Data.OleDb.OleDbType.WChar,
                    255);
                readPreviousLogValues.BindParameter(
                    VulcanPackage.AddVariable(varPrefix + "LastDestinationRecord", String.Empty),
                    Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Output,
                    (int)System.Data.OleDb.OleDbType.WChar,
                    255);

                readPreviousLogValues.BindParameter(
                    VulcanPackage.AddVariable(varPrefix + "Status", String.Empty),
                    Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Output,
                    (int)System.Data.OleDb.OleDbType.WChar,
                    255);

                readPreviousLogValues.BindParameter(
                    VulcanPackage.AddVariable(varPrefix + "Notes", String.Empty),
                    Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Output,
                    (int)System.Data.OleDb.OleDbType.WChar,
                    255);

                // Handle the sub-tasks just like the Container pattern
                DTS.Executable previousExec = readPreviousLogValues.SQLTaskHost;
                Pattern p = null;
                foreach (XPathNavigator nav in patternNavigator.SelectChildren(XPathNodeType.Element))
                {
                    p = PatternFactory.ProcessPattern(VulcanPackage, ParentContainer, nav, p);
                    VulcanPackage.AddPrecedenceConstraint(previousExec, p.FirstExecutableGeneratedByPattern, ParentContainer);
                    previousExec = p.LastExecutableGeneratedByPattern;
                }

                this.LastExecutableGeneratedByPattern = previousExec;

                // Pop the current log variable off of the stack and end the log.
                ll = _logStack.Pop();

                te = new TemplateEmitter(VulcanPackage.TemplateManager["LogSetValue"]);
                te.SetNamedParameter("LogID", ll.LogVariable.QualifiedName);
                StringBuilder writeValuesToLogQuery = new StringBuilder();
                foreach (string column in ll.LogColumnDictionary.Keys)
                {
                    string temp;
                    te.SetNamedParameter("Column", column);
                    te.SetNamedParameter("Value", ll.LogColumnDictionary[column].QualifiedName);
                    te.Emit(out temp);
                    writeValuesToLogQuery.Append(temp);
                    writeValuesToLogQuery.AppendFormat(" + \n");
                }

                te = new TemplateEmitter(VulcanPackage.TemplateManager["LogEnd"]);
                te.Emit(out sqlExpression);

                writeValuesToLogQuery.Append(sqlExpression);

                SQLTask logEndTask = new SQLTask(VulcanPackage, execSqlTaskName + Resources.Stop, execSqlTaskName + Resources.Stop, ParentContainer, connection);
                logEndTask.TransmuteToExpressionTask(writeValuesToLogQuery.ToString());
                logEndTask.BindParameter(ll.LogVariable, Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Input, 3);
                if (previousExec != null)
                {
                    VulcanPackage.AddPrecedenceConstraint(previousExec, logEndTask.SQLTaskHost, ParentContainer);
                }
                this.LastExecutableGeneratedByPattern = logEndTask.SQLTaskHost;
            }
        }
    }
}