using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast
{
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("IncludeFileElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstIncludedFileNode : AstNode
    {
        #region Private Storage
        private string _includedFile;
        private string _asNameSpace;
        #endregion // Private Storage

        #region Public Accessor Properties
        
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("__self", "http://tempuri.org/vulcan2.xsd", "text()")]
        public string IncludedFile
        {
            get { return _includedFile; }
            set { _includedFile = value.Trim().ToUpperInvariant(); }
        }

        public string As
        {
            get { return _asNameSpace; }
            set { _asNameSpace = value; }
        }

        #endregion // Public Accessor Properties

        #region Default Constructor
        public AstIncludedFileNode()
        {
        }
        #endregion // Default Constructor

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
