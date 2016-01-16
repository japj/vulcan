using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Transformation
{
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("DataFlowElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstDataFlowNode : AstTransformationNode
    {
        #region Private Storage
        private VulcanCollection<Transformation.AstTransformationNode> _transformations;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("DerivedColumns", "http://tempuri.org/vulcan2.xsd")]
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Lookup", "http://tempuri.org/vulcan2.xsd")]
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("OLEDBCommand", "http://tempuri.org/vulcan2.xsd")]
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("IsNullPatcher", "http://tempuri.org/vulcan2.xsd")]
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("AutoNullPatcher", "http://tempuri.org/vulcan2.xsd")]
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("TermLookup", "http://tempuri.org/vulcan2.xsd")]
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("ConditionalSplit", "http://tempuri.org/vulcan2.xsd")]
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("UnionAll", "http://tempuri.org/vulcan2.xsd")]
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("Sort", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<Transformation.AstTransformationNode> Transformations
        {
            get { return _transformations; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstDataFlowNode()
        {
            _transformations = new VulcanCollection<AstTransformationNode>();
        }
        #endregion   // Default Constuctor

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
