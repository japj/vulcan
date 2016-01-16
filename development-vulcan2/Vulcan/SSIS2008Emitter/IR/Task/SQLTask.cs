using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.Framework;

namespace Ssis2008Emitter.IR.Task
{
    public class SqlTask : Task
    {
        #region Private Storage
        private string _body;
        private string _resultSet;
        private string _type;
        private List<ExecuteSQLParameter> _executeSQLParameterList = new List<ExecuteSQLParameter>();
        private List<ExecuteSQLResult> _executeSQLResultList = new List<ExecuteSQLResult>();
        private bool _executeDuringDesignTime;
        private IsolationLevel _isolationLevel;
        private string _connection;
        #endregion  // Private Storage

        #region Public Accessor Properties
        public string Body
        {
            get { return _body; }
            set { _body = value; }
        }

        public IsolationLevel IsolationLevel
        {
            //TODO: By default, set IsolationLevel to ReadCommitted. This will be implemented in Vulcan 2 Schema, rather than hardcode here.
            get { return (_isolationLevel == 0 ? IsolationLevel.ReadCommitted : _isolationLevel); }
            set { _isolationLevel = value; }
        }

        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public string ResultSet
        {
            get { return _resultSet; }
            set { _resultSet = value; }
        }

        public IList<ExecuteSQLParameter> ParameterList
        {
            get { return _executeSQLParameterList; }
        }

        public IList<ExecuteSQLResult> ResultList
        {
            get { return _executeSQLResultList; }
        }

        public bool ExecuteDuringDesignTime
        {
            get { return _executeDuringDesignTime; }
            set { _executeDuringDesignTime = value; }
        }

        public string Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }
        #endregion  // Public Accessor Properties
    }


    public class ExecuteSQLParameter : LogicalObject
    {
        #region Private Storage
        private Variable _variable;
        private Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections _direction;
        private System.Data.OleDb.OleDbType _OleDbType;
        #endregion  // Private Storage

        #region Public Accessor Properties
        public Variable Variable
        {
            get { return _variable; }
            set { _variable = value; }
        }

        public Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        public System.Data.OleDb.OleDbType OleDbType
        {
            get { return _OleDbType; }
            set { _OleDbType = value; }
        }
        #endregion  // Public Accessor Properties
    }

    public class ExecuteSQLResult : LogicalObject
    {
        #region Private Storage
        private string _variableName;
        #endregion  // Private Storage

        #region Public Accessor Properties
        public string VariableName
        {
            get { return _variableName; }
            set { _variableName = value; }
        }
        #endregion  // Public Accessor Properties
    }
}
