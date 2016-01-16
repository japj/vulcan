using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;
using VulcanEngine.IR.Ast.Ssas;

namespace VulcanEngine.IR.Ast.MeasureGroup
{
    [AstSchemaTypeBindingAttribute("MeasureElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstMeasureNode : AstNamedNode
    {
        #region Private Storage
        private bool _visible;
        private string _expression;
        private AstMeasureAggregateColumnNode _aggregateColumn;
        private string _foreColorExpression;
        private string _backColorExpression;
        private string _fontSizeExpression;
        private string _fontFlagsExpression;
        private SsasDataMimeType _dataMimeType;
        private SsasDataFormat _dataFormat;
        private SsasCollation _collation;
        private SsasTrimming _trimming;
        private SsasInvalidXMLCharacterProcessing _invalidXMLCharacterProcessing;
        private SsasNullProcessing _nullProcessing;
        private SsasMeasureFormat _measureFormat;
        private string _displayFolder;
        #endregion   // Private Storage

        #region Public Accessor Properties
        public bool HasExpression { get { return this._expression != null; } }
        public bool HasAggregateColumn { get { return this._aggregateColumn != null; } }

        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        public string Expression
        {
            get { return _expression; }
            set { _expression = value; }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Aggregate", "http://tempuri.org/vulcan2.xsd")]
        public AstMeasureAggregateColumnNode AggregateColumn
        {
            get { return _aggregateColumn; }
            set { _aggregateColumn = value; }
        }

        public string ForeColorExpression
        {
            get { return _foreColorExpression; }
            set { _foreColorExpression = value; }
        }

        public string BackColorExpression
        {
            get { return _backColorExpression; }
            set { _backColorExpression = value; }
        }

        public string FontSizeExpression
        {
            get { return _fontSizeExpression; }
            set { _fontSizeExpression = value; }
        }

        public string FontFlagsExpression
        {
            get { return _fontFlagsExpression; }
            set { _fontFlagsExpression = value; }
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

        public SsasMeasureFormat MeasureFormat
        {
            get { return _measureFormat; }
            set { _measureFormat = value; }
        }

        public string DisplayFolder
        {
            get { return _displayFolder; }
            set { _displayFolder = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstMeasureNode() { }
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

    [AstSchemaTypeBindingAttribute("AggregateElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstMeasureAggregateColumnNode : AstNamedNode
    {
        #region Private Storage
        private Table.AstTableColumnBaseNode _column;
        private SsasAggregationFunction _function;
        #endregion   // Private Storage

        #region Public Accessor Properties

        public Table.AstTableColumnBaseNode Column
        {
            get { return _column; }
            set { _column = value; }
        }

        public SsasAggregationFunction Function
        {
            get { return _function; }
            set { _function = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstMeasureAggregateColumnNode() { }
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
