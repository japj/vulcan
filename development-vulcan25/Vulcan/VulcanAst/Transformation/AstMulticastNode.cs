using System;
using System.Globalization;
using AstFramework.Model;
using Vulcan.Utility.Collections;

namespace VulcanEngine.IR.Ast.Transformation
{
    public partial class AstMulticastNode
    {
        public AstMulticastNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
            CollectionPropertyChanged += AstMulticastNode_CollectionPropertyChanged;
        }

        private void AstMulticastNode_CollectionPropertyChanged(object sender, VulcanCollectionPropertyChangedEventArgs e)
        {
            if (e.PropertyName == "OutputPaths")
            {
                for (int outputIndex = 0; outputIndex < OutputPaths.Count; outputIndex++)
                {
                    OutputPaths[outputIndex].SsisName = String.Format(CultureInfo.InvariantCulture, "Multicast Output {0}", outputIndex + 1);
                }
            }
        }
    }
}
