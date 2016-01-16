using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Transformation
{
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("TransformationDerivedColumnListElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstDerivedColumnListNode : AstTransformationNode
    {
        #region Private Storage
        private VulcanCollection<AstDerivedColumnNode> _columns;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Column", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<AstDerivedColumnNode> Columns
        {
            get { return _columns; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstDerivedColumnListNode()
        {
            this._columns = new VulcanCollection<AstDerivedColumnNode>();
        }
        #endregion   // Default Constructor

        #region Validation
        private List<AstNode> Children
        {
            get
            {
                List<AstNode> children = new List<AstNode>();
                children.AddRange(this.Columns.Cast<AstNode>());
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

    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("TransformationDerivedColumnElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstDerivedColumnNode : AstNamedNode
    {
        #region Private Storage

        private DerivedColumnType _type;
        private int _length;
        private int _precision;
        private int _scale;
        private string _codepage;
        private bool _replaceExisting;
        private string _expression;  // Text Child
        #endregion   // Private Storage

        #region Public Accessor Properties
        public DerivedColumnType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public int Length
        {
            get { return _length; }
            set { _length = value; }
        }

        public int Precision
        {
            get { return _precision; }
            set { _precision = value; }
        }

        public int Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        public string Codepage
        {
            get { return _codepage; }
            set { _codepage = value; }
        }

        public bool ReplaceExisting
        {
            get { return _replaceExisting; }
            set { _replaceExisting = value; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("__self", "http://tempuri.org/vulcan2.xsd", "text()")]
        public string Expression
        {
            get { return _expression; }
            set { _expression = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstDerivedColumnNode() { }
        #endregion   // Default Constructor

        #region Validation
        public override IList<ValidationItem> Validate()
        {
            List<ValidationItem> validationItems = new List<ValidationItem>();
            validationItems.AddRange(base.Validate());

            return validationItems;
        }
        #endregion  // Validation
    }

    public enum DerivedColumnType
    {
        BOOL,
        WSTR,
        STR,
        FLOAT,
        DOUBLE,
        INT32,
        INT64,
        UINT32,
        UINT64,
        DATE,
        TIME,
        TIMESTAMP,
        TIMESTAMP2,
    }
}
