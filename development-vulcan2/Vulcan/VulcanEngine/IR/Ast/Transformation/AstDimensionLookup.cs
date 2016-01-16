using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VulcanEngine.IR.Ast.Transformation
{
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("TransformationDimensionLookupElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstDimensionLookup : AstTransformationNode
    {
        #region Private Storage
        private VulcanCollection<AstTransformationNode> _transformations;
        private VulcanCollection<AstDimensionLookupParameter> _parameters;
        #endregion   // Private Storage

        #region Public Accessor Properties
        public VulcanCollection<AstTransformationNode> Transformations
        {
            get
            {
                return _transformations;
            }
        }

        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Parameter", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<AstDimensionLookupParameter> Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstDimensionLookup()
        {
            this._transformations = new VulcanCollection<AstTransformationNode>();
        }
        #endregion   // Default Constructor
    }

    public class AstDimensionLookupParameter : AstNode
    {
        #region Private Storage
        private VulcanCollection<AstTransformationNode> _sourceColumn;
        private VulcanCollection<AstTransformationNode> _targetColumn;
        private AstDimensionLookupParameterDirection _direction;
        #endregion   // Private Storage

        #region Public Accessor Properties
        public VulcanCollection<AstTransformationNode> SourceColumn
        {
            get { return _sourceColumn; }
            set { _sourceColumn = value; }
        }

        public VulcanCollection<AstTransformationNode> TargetColumn
        {
            get { return _targetColumn; }
            set { _targetColumn = value; }
        }

        public AstDimensionLookupParameterDirection Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }
        #endregion   // Public Accessor Properties
    }

    public enum AstDimensionLookupParameterDirection
    {
        Input,
        Output,
    }
}
