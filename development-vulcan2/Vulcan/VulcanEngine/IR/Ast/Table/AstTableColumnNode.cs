using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Table
{
    public enum ColumnType
    {
      BOOL,
      WSTR,
      STR,
      FLOAT,
      DOUBLE,
      DECIMAL,
      INT32,
      INT64,
      UINT32,
      UINT64,
      DATE,
      TIME,
      DATETIME,
      BINARY,
      CUSTOM
    };

    public abstract class AstTableColumnBaseNode : AstNamedNode
    {
        #region Private Storage
        private bool _isAssignable = true;
        private bool _isAssignableSet = false;

        private ColumnType _dataType;

        private int _length;
        private int _precision;
        private int _scale;

        private string _customType;
        private string _computed = null;
        private string _default;
        #endregion  // Private Storage

        #region Public Accessor Properties
        public abstract bool IsNullable { get; set; }
        public abstract bool IsComputed { get; }

        public virtual bool IsAssignable
        {
            get
            {
                if (_isAssignableSet)
                {
                    return _isAssignable;
                }
                else
                {
                    AstTableNode tableNode = AstWalker.FirstParent<AstTableNode>(this);
                    // TODO: Shouldn't need a null check here.  Why would a parent table not be found?
					if (tableNode != null)
                    {
                        foreach (AstTableKeyBaseNode key in tableNode.Keys)
                        {
                            if (key is AstTableIdentityNode)
                            {
                                foreach (AstTableKeyColumnNode keyColumn in key.Columns)
                                {
                                    if (this.Equals(keyColumn.Column))
                                    {
                                        _isAssignableSet = true;
                                        _isAssignable = false;
                                    }
                                }
                            }
                        }
                    }
                    return _isAssignable;
                }
            }
        }

        public ColumnType Type
        {
            get { return _dataType; }
            set
            {
                _dataType = value;
                VulcanOnPropertyChanged("Type");
            }
        }

        [BrowsableAttribute(false)]
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

        public string CustomType
        {
            get { return _customType; }
            set { _customType = value; }
        }

        public string Computed
        {
            get { return _computed; }
            set { _computed = value; }
        }

        [BrowsableAttribute(false)]
        public string Default
        {
            get { return _default; }
            set
            {
                _default = value;
            }
        }
        #endregion  // Public Accessor Properties

        #region Validation
        public override IList<ValidationItem> Validate()
        {
            List<ValidationItem> validationItems = new List<ValidationItem>();
            validationItems.AddRange(base.Validate());

            return validationItems;
        }
        #endregion  // Validation
    }
        
    [AstSchemaTypeBindingAttribute("TableColumnElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstTableColumnNode : AstTableColumnBaseNode
    {
        #region Private Storage
        private bool _isNullable;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [BrowsableAttribute(false)]
        public override bool IsNullable
        {
            get { return _isNullable; }
            set { _isNullable = value; }
        }

        public override bool IsComputed
        {
            get
            {
                return !String.IsNullOrEmpty(Computed);
            }
        }

        public override bool IsAssignable
        {
            get
            {
                return base.IsAssignable && !IsComputed;
            }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstTableColumnNode() { }
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

    [AstSchemaTypeBindingAttribute("FactColumnElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstFactTableColumnNode : AstTableColumnNode
    {
        #region Private Storage
        #endregion   // Private Storage

        #region Public Accessor Properties
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstFactTableColumnNode() { }
        #endregion   // Default Constructor
    }

    [AstSchemaTypeBindingAttribute("HashedKeyColumnElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstTableHashedKeyColumnNode : AstTableColumnBaseNode
    {
        #region Private Storage
        private AstTableKeyBaseNode _constraint;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [BrowsableAttribute(false)]
        public AstTableKeyBaseNode Constraint
        {
            get { return _constraint; }
            set { _constraint = value; }
        }

        public override bool IsComputed
        {
            get { return true; }
        }

        public override bool IsAssignable
        {
            get
            {
                return false;
            }
        }

        public override bool IsNullable
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        #endregion  // Public Accessor Properties

        #region Default Constructor
        public AstTableHashedKeyColumnNode() : base() { }
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
    [AstSchemaTypeBindingAttribute("DimensionReferenceElemType", "http://tempuri.org/vulcan2.xsd")]
    [AstSchemaTypeBindingAttribute("TableReferenceElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstTableDimensionReferenceNode : AstTableColumnBaseNode
    {
        #region Private Storage
        private Dimension.AstDimensionNode _dimension;
        private string _outputName;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("DimensionName", "http://tempuri.org/vulcan2.xsd")]
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("TableName", "http://tempuri.org/vulcan2.xsd")]
        [BrowsableAttribute(false)]
        public Dimension.AstDimensionNode Dimension
        {
            get { return _dimension; }
            set { _dimension = value; }
        }

        public string OutputName
        {
            get { return _outputName; }
            set { _outputName = value; }
        }

        public override string Name
        {
            get { return _outputName; }
        }

        public override bool IsNullable
        {
            get { return false; }
            set { /* TODO: Message.Error() */ }
        }

        public override bool IsComputed
        {
            //DimensionReference types are never computed
            get
            {
                return false;
            }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstTableDimensionReferenceNode() { }
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
}
