using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Table
{
    public class AstTableSourceBaseNode : AstNamedNode { }

    [AstSchemaTypeBindingAttribute("TableStaticSourceElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstTableStaticSourceNode : AstTableSourceBaseNode
    {
        #region Private Storage
        private List<string> _rows;
        private List<Dictionary<AstTableColumnBaseNode, string>> _columnValuePairRows;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [AstXNameBindingAttribute("Row", "http://tempuri.org/vulcan2.xsd")]
        public List<string> Rows
        {
            get { return _rows; }
        }

        public List<Dictionary<AstTableColumnBaseNode, string>> ColumnValuePairRows
        {
            get { return _columnValuePairRows; }
            set
            {
                _columnValuePairRows = value;

                // Update rows to reflect columnValuePairRows.
                _rows.Clear();

                for (int i = 0; i < _columnValuePairRows.Count; i++)
                {
                    // Get a row of key value pairs.
                    Dictionary<AstTableColumnBaseNode, string> columnValuePairs = (Dictionary<AstTableColumnBaseNode, string>)_columnValuePairRows[i];
                    int pairsCount = columnValuePairs.Count;
                    StringBuilder rowString = new StringBuilder(pairsCount);

                    int pairsAppended = 0;
                    foreach (KeyValuePair<AstTableColumnBaseNode, string> pair in columnValuePairs)
                    {
                        rowString.Append(pair.Value);
                        if (pairsAppended < (pairsCount - 1))
                        {
                            // The string is a comma delimited list.
                            rowString.Append(',');
                            pairsAppended++;
                        }
                    }

                    _rows.Add(rowString.ToString());
                }
            }
        }

        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstTableStaticSourceNode()
        {
            this._rows = new List<string>();
            this._columnValuePairRows = new List<Dictionary<AstTableColumnBaseNode, string>>();
        }
        #endregion   // Default Constructor

        #region Validation
        private List<AstNode> Children
        {
            get
            {
                List<AstNode> children = new List<AstNode>();
                children.AddRange(this.Rows.Cast<AstNode>());
                return children;
            }
        }

        public override IList<ValidationItem> Validate()
        {
            List<ValidationItem> validationItems = new List<ValidationItem>();
            validationItems.AddRange(base.Validate());

            foreach (AstNode child in this.Children)
            {
                validationItems.AddRange(child.Validate());
            }

            return validationItems;
        }
        #endregion  // Validation
    }

    [AstSchemaTypeBindingAttribute("TableQuerySourceElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstTableQuerySourceNode : AstTableSourceBaseNode
    {
        #region Private Storage
        private Connection.AstConnectionNode _connection;
        private string _query;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [AstXNameBindingAttribute("ConnectionName", "http://tempuri.org/vulcan2.xsd")]
        public Connection.AstConnectionNode Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("__self", "http://tempuri.org/vulcan2.xsd", "text()")]
        public string Query
        {
            get { return _query; }
            set { _query = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstTableQuerySourceNode()
        {
        }
        #endregion   // Default Constructor

        #region Validation
        private List<AstNode> Children
        {
            get
            {
                List<AstNode> children = new List<AstNode>();
                return children;
            }
        }

        public override IList<ValidationItem> Validate()
        {
            List<ValidationItem> validationItems = new List<ValidationItem>();
            validationItems.AddRange(base.Validate());

            foreach (AstNode child in this.Children)
            {
                validationItems.AddRange(child.Validate());
            }

            return validationItems;
        }
        #endregion  // Validation
    }

    [AstSchemaTypeBindingAttribute("TableDynamicSourceElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstTableDynamicSourceNode : AstTableSourceBaseNode
    {
        #region Private Storage
        private Task.AstPackageNode _package;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [AstXNameBindingAttribute("__self", "http://tempuri.org/vulcan2.xsd")]
        public Task.AstPackageNode Package
        {
            get { return _package; }
            set { _package = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstTableDynamicSourceNode()
        {
        }
        #endregion   // Default Constructor

        #region Validation
        private List<AstNode> Children
        {
            get
            {
                List<AstNode> children = new List<AstNode>();
                return children;
            }
        }

        public override IList<ValidationItem> Validate()
        {
            List<ValidationItem> validationItems = new List<ValidationItem>();
            validationItems.AddRange(base.Validate());

            foreach (AstNode child in this.Children)
            {
                validationItems.AddRange(child.Validate());
            }

            return validationItems;
        }
        #endregion  // Validation
    }
}
