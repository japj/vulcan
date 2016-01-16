using AstFramework.Model;

namespace VulcanEngine.IR.Ast.PersistentVariables
{
    public partial class AstPersistentVariableNode
    {
        public AstPersistentVariableNode(IFrameworkItem parentItem)
            : base(parentItem)
        {
            InitializeAstNode();
        }
    }
}

