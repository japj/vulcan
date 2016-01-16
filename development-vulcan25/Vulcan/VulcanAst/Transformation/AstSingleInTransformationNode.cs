using AstFramework.Model;
using Vulcan.Utility.ComponentModel;

namespace VulcanEngine.IR.Ast.Transformation
{
    public partial class AstSingleInTransformationNode
    {
        protected AstSingleInTransformationNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();

            SingletonPropertyChanged += AstSingleInTransformationNode_SingletonPropertyChanged;
        }

        private void AstSingleInTransformationNode_SingletonPropertyChanged(object sender, VulcanPropertyChangedEventArgs e)
        {
            /*
            if (e.PropertyName == "InputPath")
            {
                var oldInputPath = e.OldValue as AstDataflowInputPathNode;
                var newInputPath = e.NewValue as AstDataflowInputPathNode;
                if (oldInputPath != null && oldInputPath.OutputPath.Transformation != null)
                {
                    oldInputPath.OutputPath.Transformation.UnbindInputPath(oldInputPath);
                }
                if (newInputPath != null && newInputPath.OutputPath.Transformation != null)
                {
                    newInputPath.OutputPath.Transformation.BindInputPath(newInputPath);
                }
            }
             */
        }
    }
}
