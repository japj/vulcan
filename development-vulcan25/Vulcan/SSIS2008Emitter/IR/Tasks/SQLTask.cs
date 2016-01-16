using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using AstFramework;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.Framework.Connections;
using VulcanEngine.Common;
using VulcanEngine.IR.Ast.Task;
using DTSTasks = Microsoft.SqlServer.Dts.Tasks;

namespace Ssis2008Emitter.IR.Tasks
{
    public enum SqlTaskStatementType
    {
        File,
        Expression
    }

    public enum SqlTaskResultSetType
    {
        None,
        SingleRow,
        Full,
        Xml
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public class SqlTask : Task
    {
        #region Private Variables
        private string _body;

        private SqlTaskResultSetType _resultSetType = SqlTaskResultSetType.None;
        private DTSTasks.ExecuteSQLTask.ResultSetType _resultSetTypeDTS = DTSTasks.ExecuteSQLTask.ResultSetType.ResultSetType_None;

        private SqlTaskStatementType _sourceStatementType = SqlTaskStatementType.File;
        private DTSTasks.ExecuteSQLTask.SqlStatementSourceType _sourceStatementTypeDTS = DTSTasks.ExecuteSQLTask.SqlStatementSourceType.FileConnection;
        
        private readonly Dictionary<string, ExecuteSqlParameter> _parameters = new Dictionary<string, ExecuteSqlParameter>();
        private readonly Dictionary<string, ExecuteSqlResult> _executeSQLResultList = new Dictionary<string, ExecuteSqlResult>();

        private Connection _connection;
        #endregion

        public SqlTask(AstExecuteSqlTaskNode astNode) : base(astNode)
        {
            ExecuteDuringDesignTime = astNode.ExecuteDuringDesignTime;
            _body = astNode.Query.Body;
            _connection = new OleDBConnection(astNode.Connection);

            StatementType = astNode.Query.QueryType == QueryType.Expression ? SqlTaskStatementType.Expression : SqlTaskStatementType.File;
            ResultSetType = (SqlTaskResultSetType)Enum.Parse(typeof(SqlTaskResultSetType), astNode.ResultSet.ToString(), true);

            LoadResultMappings(astNode);

            LoadParameterMappings(astNode);
        }

        private void LoadResultMappings(AstExecuteSqlTaskNode astNode)
        {
            foreach (AstExecuteSqlParameterMappingTypeNode resultNode in astNode.Results)
            {
                _executeSQLResultList.Add(resultNode.Name, new ExecuteSqlResult(resultNode.Name, resultNode.Variable.Name));
            }
        }

        private void LoadParameterMappings(AstExecuteSqlTaskNode astNode)
        {
            foreach (AstExecuteSqlParameterMappingTypeNode paramNode in astNode.Query.Parameters)
            {
                var paramType = PlatformEmitters.OleDBTypeTranslator.Translate(paramNode.Variable.TypeCode);
                DTSTasks.ExecuteSQLTask.ParameterDirections direction;

                switch (paramNode.Direction)
                {
                    case Direction.Input:
                        direction = Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Input;
                        break;
                    case Direction.Output:
                        direction = Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.Output;
                        break;
                    case Direction.ReturnValue:
                        direction = Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections.ReturnValue;
                        break;
                    default: throw new NotImplementedException(String.Format(CultureInfo.InvariantCulture, "ExecuteSQL Parameters: Direction {0} not supported", paramNode.Direction));
                }
                
                _parameters.Add(paramNode.Name, new ExecuteSqlParameter(paramNode.Name, paramNode.Variable.Name, direction, paramType, paramNode.Length));
            }
        }

        #region Public Properties

        public IDictionary<string, ExecuteSqlParameter> Parameters
        {
            get { return _parameters; }
        }

        public IDictionary<string, ExecuteSqlResult> Results
        {
            get { return _executeSQLResultList; }
        }

        public Connection Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        public string Body
        {
            get { return _body; }
            set { _body = value; }
        }

        public SqlTaskResultSetType ResultSetType
        {
            get { return _resultSetType; }
            set
            {
                switch (value)
                {
                    case SqlTaskResultSetType.None:
                        _resultSetTypeDTS = Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ResultSetType.ResultSetType_None;
                        break;
                    case SqlTaskResultSetType.SingleRow:
                        _resultSetTypeDTS = Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ResultSetType.ResultSetType_SingleRow;
                        break;
                    case SqlTaskResultSetType.Full:
                        _resultSetTypeDTS = Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ResultSetType.ResultSetType_Rowset;
                        break;
                    case SqlTaskResultSetType.Xml:
                        _resultSetTypeDTS = Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ResultSetType.ResultSetType_XML;
                        break;
                    default:
                        throw new NotSupportedException(String.Format(CultureInfo.InvariantCulture, Properties.Resources.ErrorEnumerationTypeNotSupported, value));
                }

                _resultSetType = value;
            }
        }

        public SqlTaskStatementType StatementType
        {
            get { return _sourceStatementType; }
            set
            {
                switch (value)
                {
                    case SqlTaskStatementType.File:
                        _sourceStatementTypeDTS = Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.SqlStatementSourceType.FileConnection;
                        break;
                    case SqlTaskStatementType.Expression:
                        _sourceStatementTypeDTS = Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.SqlStatementSourceType.DirectInput;
                        break;
                    default:
                        throw new NotSupportedException(String.Format(CultureInfo.InvariantCulture, Properties.Resources.ErrorEnumerationTypeNotSupported, value));
                }

                _sourceStatementType = value;
            }
        }

        protected DTSTasks.ExecuteSQLTask.ExecuteSQLTask ExecuteSqlTask
        {
            get { return (DTSTasks.ExecuteSQLTask.ExecuteSQLTask)DtsTaskHost.InnerObject; }
        }

        public override string Moniker
        {
            get { return "STOCK:SQLTask"; }
        }

        public override Microsoft.SqlServer.Dts.Runtime.Executable DtsExecutable
        {
            get { return DtsTaskHost; }
        }

        #endregion

        #region Methods
        public override void Initialize(SsisEmitterContext context)
        {
            base.Initialize(context);
            _connection.Initialize(context);
        }

        public override void Emit(SsisEmitterContext context)
        {
            base.Emit(context);
            _connection.Emit(context);
            SetProperty("Connection", _connection.Name);

            ExecuteSqlTask.ResultSetType = _resultSetTypeDTS;
            ExecuteSqlTask.SqlStatementSourceType = _sourceStatementTypeDTS;

            switch (_sourceStatementType)
            {
                case SqlTaskStatementType.File:
                    string filePath;
                    string fileName;
                    _writeSqlToFile(context, out fileName, out filePath);

                    var fileConnection = new FileConnection(fileName, filePath);
                    fileConnection.Initialize(context);
                    fileConnection.Emit(context);
                    ExecuteSqlTask.SqlStatementSource = fileConnection.Name;

                    break;
                case SqlTaskStatementType.Expression:

                    ExecuteSqlTask.SqlStatementSource = Properties.Resources.Placeholder;
                    SetExpression("SqlStatementSource", _body);
                    break;
                default:
                    throw new NotSupportedException(String.Format(CultureInfo.InvariantCulture, Properties.Resources.ErrorEnumerationTypeNotSupported, _sourceStatementType));
            }

            // Parameter Binding
            int index = 0;
            foreach (ExecuteSqlParameter param in Parameters.Values)
            {
                BindParameter(param.VariableName, param.Direction, param.Name, (int)param.OleDBType, param.Length);
                index++;
            }

            foreach (ExecuteSqlResult executeSqlResult in Results.Values)
            {
                BindResult(executeSqlResult.Name, executeSqlResult.VariableName);
            }

            TryExecuteDuringDesignTime();
        }

        private void _writeSqlToFile(SsisEmitterContext context, out string fileName, out string filePath)
        {
            fileName = Name + Properties.Resources.ExtensionSQLFile;
            filePath = Path.Combine(context.Package.PackageFolder, fileName);

            try
            {
                TextWriter sqlFileWriter = new StreamWriter(filePath, false, Encoding.Unicode);
                sqlFileWriter.Write(Body);
                sqlFileWriter.Close();
            }
            catch (PathTooLongException e)
            {
                throw new PathTooLongException(String.Format(CultureInfo.InvariantCulture, "{0}: {1}", filePath, e.Message), e);
            }

            context.Package.SsisProject.MiscFiles.Add(fileName);
        }

        private void BindResult(string resultName, string variableName)
        {
            DTSTasks.ExecuteSQLTask.IDTSResultBindings resultBindings = ExecuteSqlTask.ResultSetBindings;
            DTSTasks.ExecuteSQLTask.IDTSResultBinding result = resultBindings.Add();
            result.ResultName = resultName;

            if (DtsTaskHost.Variables.Contains(variableName))
            {
                result.DtsVariableName = DtsTaskHost.Variables[variableName].QualifiedName;
            }
            else
            {
                MessageEngine.Trace(AstNamedNode, Severity.Error, "V0109", "Task {0}: Could not Bind ResultSet: Parameter {1}, Variable {2} does not exist", Name, resultName, variableName);
            }
        }

        private void BindParameter(string variableName, DTSTasks.ExecuteSQLTask.ParameterDirections direction, string parameterName, int dataType, int size)
        {
            DTSTasks.ExecuteSQLTask.IDTSParameterBinding binding = ExecuteSqlTask.ParameterBindings.Add();
            binding.ParameterDirection = direction;
            binding.ParameterName = parameterName;
            binding.ParameterSize = size;
            binding.DataType = dataType;

            if (DtsTaskHost.Variables.Contains(variableName))
            {
                binding.DtsVariableName = DtsTaskHost.Variables[variableName].QualifiedName;
            }
            else
            {
                MessageEngine.Trace(AstNamedNode, Severity.Error, "V0110", "Task {0}: Could not Bind Parameter {1}: Variable {2} does not exist", Name, parameterName, variableName);
            }
        }
        #endregion
    }
}
