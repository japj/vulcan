using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using DTS = Microsoft.SqlServer.Dts.Runtime;
using DTSTasks = Microsoft.SqlServer.Dts.Tasks;

using VulcanEngine.Common;
using Ssis2008Emitter.Properties;
using Ssis2008Emitter.Utility;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.Framework;
using Ssis2008Emitter.IR.Task;
using Ssis2008Emitter.Emitters.Common;
using Ssis2008Emitter.Emitters.Framework;

namespace Ssis2008Emitter.Emitters.Task
{
    [PhysicalIRMapping(typeof(SqlTask))]
    public class SsisSqlTask : SsisTaskEmitter, ISSISEmitter
    {
        private DTS.TaskHost _sqlTask;
        private SqlTask _logicalExecuteSQL;

        public SsisSqlTask(SqlTask obj, SSISEmitterContext context) : base(obj, context)
        {
            _logicalExecuteSQL = obj;
            _sqlTask = (DTS.TaskHost)Context.SSISSequence.AppendExecutable("STOCK:SQLTask");

            if (!string.IsNullOrEmpty(_logicalExecuteSQL.Name))
            {
                _name = _logicalExecuteSQL.Name;
            }

            _sqlTask.Name = Name + Guid.NewGuid().ToString();
            _sqlTask.Description = Description;

            SetProperty("Connection", _logicalExecuteSQL.Connection);
            SetProperty("IsolationLevel", _logicalExecuteSQL.IsolationLevel);

            switch (_logicalExecuteSQL.Type.ToUpper(System.Globalization.CultureInfo.InvariantCulture))
            {
                case "EXPRESSION": SSISTask.SqlStatementSourceType = DTSTasks.ExecuteSQLTask.SqlStatementSourceType.DirectInput; break;
                default: //case "FILE":
                    SSISTask.SqlStatementSourceType = DTSTasks.ExecuteSQLTask.SqlStatementSourceType.FileConnection; break;
            }
        }

        public SSISEmitterContext Emit()
        {
            SetResultSetType(_logicalExecuteSQL.ResultSet);

            switch (_logicalExecuteSQL.Type.ToUpper(System.Globalization.CultureInfo.InvariantCulture))
            {
                case "EXPRESSION":
                    SSISTask.SqlStatementSource = Resources.Placeholder;
                    SetExpression("SqlStatementSource", _logicalExecuteSQL.Body);
                    break;
                default: // case "FILE":
                    //TODO: solve the name conflicts if necessary.
                    string sqlFilePath;
                    string sqlFileRelativePath;
                    ConnectionConfiguration config = new ConnectionConfiguration();
                    config.Name = Name;
                    config.Type = "FILE";
                    WriteSQLToFile(_logicalExecuteSQL.Body, out sqlFilePath, out sqlFileRelativePath);
                    config.ConnectionString = SSISExpressionPathBuilder.BuildExpressionPath(sqlFileRelativePath);
                    
                    SsisConnection connection = new SsisConnection(config);
                    SSISTask.SqlStatementSource = connection.ConnectionManager.Name;
                    Context.Package.ProjectManager.MiscFiles.Add(Name + Resources.ExtensionSQLFile);
                    break;

            }

            int index = 0;
            foreach (ExecuteSQLParameter param in _logicalExecuteSQL.ParameterList)
            {
                BindParameter(new SsisVariable(param.Variable, Context).DTSVariable, param.Direction, index.ToString(), (int)param.OleDbType, 255);
                index++;
            }
            
            foreach (ExecuteSQLResult executeSQLResult in _logicalExecuteSQL.ResultList)
            {
                BindResult(executeSQLResult.Name, executeSQLResult.VariableName);
            }
            
            ExecuteSQLTask(_logicalExecuteSQL.ExecuteDuringDesignTime);

            return _context;
        }
        
        public void SetConnection(string sConnectionName)
        {
            if (!string.IsNullOrEmpty(sConnectionName))
            {
                SetProperty("Connection", sConnectionName);
            }
        }

        public void SetResultSetType(string sResultType)
        {
            switch (sResultType.ToUpper(System.Globalization.CultureInfo.InvariantCulture))
            {
                case "NONE": SSISTask.ResultSetType = DTSTasks.ExecuteSQLTask.ResultSetType.ResultSetType_None; break;
                case "SINGLEROW": SSISTask.ResultSetType = DTSTasks.ExecuteSQLTask.ResultSetType.ResultSetType_SingleRow; break;
                case "FULL": SSISTask.ResultSetType = DTSTasks.ExecuteSQLTask.ResultSetType.ResultSetType_Rowset; break;
                case "XML": SSISTask.ResultSetType = DTSTasks.ExecuteSQLTask.ResultSetType.ResultSetType_XML; break;
                default: SSISTask.ResultSetType = DTSTasks.ExecuteSQLTask.ResultSetType.ResultSetType_None; break;
            }
        }

        private void WriteSQLToFile(string sSQL, out string sqlFilePath, out string sqlFileRelativePath)
        {
            sqlFileRelativePath = Context.Package.PackageRelativeDirectory + Path.DirectorySeparatorChar + Name + Resources.ExtensionSQLFile;
            sqlFilePath = Context.Package.OutputBaseDirectory + sqlFileRelativePath;

            TextWriter sqlFile = new StreamWriter(sqlFilePath,false,Encoding.Unicode);
            sqlFile.Write(sSQL);
            sqlFile.Close();
        }

        private void BindResult(string resultName, string variableName)
        {
            DTSTasks.ExecuteSQLTask.IDTSResultBindings resultBindings = SSISTask.ResultSetBindings;
            DTSTasks.ExecuteSQLTask.IDTSResultBinding result = resultBindings.Add();
            result.ResultName = resultName;

            if (_sqlTask.Variables.Contains(variableName))
            {
                result.DtsVariableName = _sqlTask.Variables[variableName].QualifiedName;
            }
            else
            {
                MessageEngine.Global.Trace(Severity.Error, "Task {0}: Could not Bind ResultSet: Variable {1} does not exist", this.Name, variableName);
            }
        }
                
        private void BindParameter(DTS.Variable variable, DTSTasks.ExecuteSQLTask.ParameterDirections direction, string parameterName, int dataType, int size)
        {
            DTSTasks.ExecuteSQLTask.IDTSParameterBinding binding = SSISTask.ParameterBindings.Add();
            binding.DtsVariableName = variable.QualifiedName;
            binding.ParameterDirection = direction;
            binding.ParameterName = parameterName;
            binding.ParameterSize = size;
            binding.DataType = dataType;
        }
        
        public virtual void BindLogVariableAsInputParameter(SsisVariable variable, int parameterIndex, System.Data.OleDb.OleDbType dataType)
        {
            BindParameter(variable.DTSVariable, DTSTasks.ExecuteSQLTask.ParameterDirections.Input, parameterIndex.ToString(), (int)dataType, 255);
        }

        public virtual void BindLogVariableAsOutputParameter(SsisVariable variable, int parameterIndex, System.Data.OleDb.OleDbType dataType)
        {
            BindParameter(variable.DTSVariable, DTSTasks.ExecuteSQLTask.ParameterDirections.Output, parameterIndex.ToString(), (int)dataType, 255);
        }
        
        public override Microsoft.SqlServer.Dts.Runtime.IDTSPropertiesProvider PropertyProvider
        {
            get { return _sqlTask; }
        }

        public DTSTasks.ExecuteSQLTask.ExecuteSQLTask SSISTask
        {
            get { return (DTSTasks.ExecuteSQLTask.ExecuteSQLTask)_sqlTask.InnerObject; }
        }

        public override SsisExecutable SSISExecutable
        {
            get { return new SsisExecutable((DTS.Executable)_sqlTask); }
        }

        protected void ExecuteSQLTask(bool bExecute)
        {
            if (bExecute)
            {
                Execute();
            }
        }
    }
}
