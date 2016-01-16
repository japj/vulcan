using VulcanEngine.IR.Ast.Task;

namespace Ssis2008Emitter.IR.Framework
{
    public class ExecutableBinding
    {
        public AstTaskflowPrecedenceConstraintsNode AstTaskflowPrecedenceConstraints { get; private set; }

        public AstTaskflowInputPathNode AstTaskflowInput { get; private set; }

        public ExecutableBinding(AstTaskflowPrecedenceConstraintsNode astTaskflowPrecedenceConstraints, AstTaskflowInputPathNode astTaskflowInput)
        {
            AstTaskflowPrecedenceConstraints = astTaskflowPrecedenceConstraints;
            AstTaskflowInput = astTaskflowInput;
        }
    }
}
