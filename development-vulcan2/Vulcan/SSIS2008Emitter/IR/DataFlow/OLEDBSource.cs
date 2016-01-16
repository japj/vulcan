using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.IR.DataFlow
{
    public class OLEDBSource : Transformation
    {
        #region Private Storage
        private string _body;
        private List<DataFlowSourceQueryParameter> _parameterMappings = new List<DataFlowSourceQueryParameter>();
        private string _connection;
        private string _sqlCommandVariableName;
        private DataFlowSourceQueryAccessMode _accessMode;

        #endregion  // Private Storage

        #region Public Accessor Properties
        public string Body
        {
            get { return _body; }
            set { _body = value; }
        }

        public string SqlCommandVariableName
        {
            get { return _sqlCommandVariableName; }
            set { _sqlCommandVariableName = value; }
        }

        public DataFlowSourceQueryAccessMode AccessMode
        {
            get { return _accessMode; }
            set { _accessMode = value; }
        }

        public IList<DataFlowSourceQueryParameter> ParameterMappings
        {
            get { return _parameterMappings; }
        }

        public string Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }
        #endregion  // Public Accessor Properties
    }

    public enum DataFlowSourceQueryAccessMode
    {
        SQLCOMMAND,
        SQLCOMMANDFROMVARIABLE
    }

    public class DataFlowSourceQueryParameter : LogicalObject
    {
        public string VariableName
        {
            get { return _variableName; }
            set { _variableName = value; }
        }

        private string _variableName;
    }
}
