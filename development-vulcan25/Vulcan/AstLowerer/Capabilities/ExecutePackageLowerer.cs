using AstFramework.Engine.Binding;
using AstFramework.Model;
using VulcanEngine.IR.Ast.Task;

namespace AstLowerer.Capabilities
{
    public static class ExecutePackageLowerer
    {
        public static void ProcessExecutePackageTransformations(SymbolTable symbolTable)
        {
            foreach (var astNamedNode in symbolTable)
            {
                var executePackageNode = astNamedNode as AstExecutePackageTaskNode;
                if (executePackageNode != null && astNamedNode.FirstThisOrParent<ITemplate>() == null)
                {
                    if (executePackageNode.Package != null)
                    {
                        executePackageNode.RelativePath = executePackageNode.Package.PackageRelativePath;
                    }
                }
            }
        }
    }
}