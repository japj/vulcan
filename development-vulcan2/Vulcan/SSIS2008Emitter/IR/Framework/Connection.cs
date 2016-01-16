using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.IR.Framework
{
    public class ConnectionConfiguration : LogicalObject
    {
        public override bool Equals(object obj)
        {
            ConnectionConfiguration connectionNode = obj as ConnectionConfiguration;
            if (connectionNode == null)
            {
                return false;
            }
            return this.Equals(connectionNode);
        }

        public bool Equals(ConnectionConfiguration connectionNode)
        {
            return this.Name.Equals(connectionNode.Name);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        #region Private Storage
        private string _type;
        private string _connectionString;
        private bool _retainSameConnection;
        #endregion  // Private Storage

        #region Public Accessor Properties
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        public bool RetainSameConnection
        {
            get { return _retainSameConnection; }
            set { _retainSameConnection = value; }
        }
        #endregion  // Public Accessor Properties
    }

    public class Connection : LogicalReference
    {
        public override string ToString()
        {
            return Name;
        }

        public static implicit operator string(Connection connection)
        {
            return connection.ToString();
        }
    }
}
