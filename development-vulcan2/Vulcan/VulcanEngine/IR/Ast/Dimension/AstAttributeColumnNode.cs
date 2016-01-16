using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;
using VulcanEngine.IR.Ast.Ssas;
using VulcanEngine.IR.Ast.Table;

namespace VulcanEngine.IR.Ast.Dimension
{
    [AstSchemaTypeBindingAttribute("AttributeColumnElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstAttributeColumnNode : AstNamedNode
    {
        #region Private Storage
        private AstTableColumnBaseNode _column;
        private SsasDataMimeType _dataMimeType;
        private SsasDataFormat _dataFormat;
        private SsasCollation _collation;
        private SsasTrimming _trimming;
        private SsasInvalidXMLCharacterProcessing _invalidXMLCharacterProcessing;
        private SsasNullProcessing _nullProcessing;
        #endregion   // Private Storage

        #region Public Accessor Properties
        public AstTableColumnBaseNode Column
        {
            get { return _column; }
            set { _column = value; }
        }

        public SsasDataMimeType DataMimeType
        {
            get { return _dataMimeType; }
            set { _dataMimeType = value; }
        }

        public SsasDataFormat DataFormat
        {
            get { return _dataFormat; }
            set { _dataFormat = value; }
        }

        public SsasCollation Collation
        {
            get { return _collation; }
            set { _collation = value; }
        }

        public SsasTrimming Trimming
        {
            get { return _trimming; }
            set { _trimming = value; }
        }

        public SsasInvalidXMLCharacterProcessing InvalidXMLCharacterProcessing
        {
            get { return _invalidXMLCharacterProcessing; }
            set { _invalidXMLCharacterProcessing = value; }
        }

        public SsasNullProcessing NullProcessing
        {
            get { return _nullProcessing; }
            set { _nullProcessing = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstAttributeColumnNode() : base() { }
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

    public class AstAttributeKeyColumnNode : AstAttributeColumnNode
    {
        public AstAttributeKeyColumnNode()
            : base()
        {
        }
    }

    public class AstAttributeNameColumnNode : AstAttributeColumnNode
    {
        public AstAttributeNameColumnNode()
            : base()
        {
        }
    }

    public class AstAttributeValueColumnNode : AstAttributeColumnNode
    {
        public AstAttributeValueColumnNode()
            : base()
        {
        }
    }
}
