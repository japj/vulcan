using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Connection
{
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("ConnectionElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstConnectionNode : AstNamedNode
    {
        public static string DefaultXMLNamespace
        {
            get { return "http://tempuri.org/vulcan2.xsd"; }
        }

        #region Private Storage
        private string _connectionString;
        private ConnectionType _type;
        private bool _retainSameConnection;
        #endregion // Private Storage

        #region Public Accessor Properties

        public string ConnectionString
        {
            get { return this._connectionString; }
            set { this._connectionString = value; }
        }

        public ConnectionType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public bool RetainSameConnection
        {
            get { return _retainSameConnection; }
            set { _retainSameConnection = value; }
        }
        #endregion // Public Accessor Properties

        #region Default Constructor
        public AstConnectionNode() { }
        #endregion // Default Constructor

        #region Validation
        public override IList<ValidationItem> Validate()
        {
            // TODO: Add ConnectionString Validator
            // TODO: Add Name Validator
            return new List<ValidationItem>();
        }
        #endregion  // Validation
    }

    public enum ConnectionType
    {
      FILE,
      OLEDB,
    }
}
