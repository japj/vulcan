using System;
using System.ComponentModel;
using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Transformation
{
    public partial class AstDataflowOutputPathNode
    {
        // TODO: Should this be in AstDesigner to enable Cloning?
        [BrowsableAttribute(false)]
        public string SsisName { get; set; }

        public AstDataflowOutputPathNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
            SingletonPropertyChanged += AstDataflowOutputPathNode_SingletonPropertyChanged;
        }

        private void AstDataflowOutputPathNode_SingletonPropertyChanged(object sender, Vulcan.Utility.ComponentModel.VulcanPropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name" && String.IsNullOrEmpty(SsisName))
            {
                SsisName = Name;
            }
        }

        [BrowsableAttribute(false)]
        public AstTransformationNode Transformation
        {
            get { return ParentItem as AstTransformationNode; }
        }
    }
}
