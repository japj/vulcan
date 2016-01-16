using System.ComponentModel;
using AstFramework.Model;
using Vulcan.Utility.Collections;
using Vulcan.Utility.ComponentModel;

namespace VulcanEngine.IR.Ast.Dimension
{
    public partial class AstAttributeRelationshipNode
    {
        private VulcanCollection<AstNamedNode> _attributes;

        public AstAttributeRelationshipNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            _attributes = new VulcanCollection<AstNamedNode>();
            InitializeAstNode();

            SingletonPropertyChanged += AstAttributeRelationshipNode_SingletonPropertyChanged;
        }

        private void AstAttributeRelationshipNode_SingletonPropertyChanged(object sender, VulcanPropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Parent" || e.PropertyName == "Child")
            {
                VulcanCompositeCollectionChanged(_attributes, e.OldValue as AstNamedNode, e.NewValue as AstNamedNode);
            }
        }

        [Browsable(false)]
        public VulcanCollection<AstNamedNode> Attributes
        {
            get { return _attributes; }
        }

        public static bool StructureEquals(AstAttributeRelationshipNode relationship1, AstAttributeRelationshipNode relationship2)
        {
            if (relationship1 == null || relationship2 == null)
            {
                return relationship1 == null && relationship2 == null;
            }

            bool match = true;
            match &= relationship1.Cardinality == relationship2.Cardinality;
            match &= relationship1.ChildAttribute == relationship2.ChildAttribute;
            match &= relationship1.Optionality == relationship2.Optionality;
            match &= relationship1.ParentAttribute == relationship2.ParentAttribute;
            match &= relationship1.RelationshipType == relationship2.RelationshipType;
            match &= relationship1.Visible == relationship2.Visible;
            return match;
        }

        public bool RequiredFieldsSet
        {
            get { return ParentAttribute != null && ChildAttribute != null; }
        }
    }
}
