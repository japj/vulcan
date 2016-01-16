using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Table
{
    public class AstTableLookupBaseNode : AstNamedNode
    {
        #region Private Storage
        #endregion   // Private Storage

        #region Public Accessor Properties
        #endregion   // Public Accessor Properties

        #region Default Constructor
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

    [AstSchemaTypeBindingAttribute("TableCustomLookupElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstTableCustomLookupNode : AstTableLookupBaseNode 
    {
        #region Private Storage
        private VulcanCollection<Transformation.AstTransformationNode> _transformations;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [AstXNameBindingAttribute("DerivedColumns", "http://tempuri.org/vulcan2.xsd")]
        [AstXNameBindingAttribute("Lookup", "http://tempuri.org/vulcan2.xsd")]
        [AstXNameBindingAttribute("OLEDBCommand", "http://tempuri.org/vulcan2.xsd")]
        [AstXNameBindingAttribute("IsNullPatcher", "http://tempuri.org/vulcan2.xsd")]
        [AstXNameBindingAttribute("AutoNullPatcher", "http://tempuri.org/vulcan2.xsd")]
        [AstXNameBindingAttribute("TermLookup", "http://tempuri.org/vulcan2.xsd")]
        [AstXNameBindingAttribute("ConditionalSplit", "http://tempuri.org/vulcan2.xsd")]
        [AstXNameBindingAttribute("UnionAll", "http://tempuri.org/vulcan2.xsd")]
        [AstXNameBindingAttribute("Sort", "http://tempuri.org/vulcan2.xsd")]
        public VulcanCollection<Transformation.AstTransformationNode> Transformations
        {
            get { return _transformations; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstTableCustomLookupNode()
            : base()
        {
            this._transformations = new VulcanCollection<VulcanEngine.IR.Ast.Transformation.AstTransformationNode>();
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

    [AstSchemaTypeBindingAttribute("TableKeyLookupElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstTableKeyLookupNode : AstTableLookupBaseNode
    {

        #region Private Storage
        private AstTableKeyBaseNode _key;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [AstXNameBindingAttribute("KeyName", "http://tempuri.org/vulcan2.xsd")]
        public AstTableKeyBaseNode Key
        {
            get { return this._key; }
            set { this._key = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstTableKeyLookupNode() : base() { }
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
