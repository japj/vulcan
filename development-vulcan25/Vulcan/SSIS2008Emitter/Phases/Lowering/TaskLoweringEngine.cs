using Ssis2008Emitter.IR.Tasks;
using Ssis2008Emitter.Phases.Lowering.Framework;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Task;

namespace Ssis2008Emitter.Phases.Lowering
{
    public static class TaskLoweringEngine
    {
        [Lowering(typeof(AstExecuteSqlTaskNode))]
        public static void LowerSqlTask(AstNode astNode, LoweringContext context)
        {
            var astTask  = astNode as AstExecuteSqlTaskNode;
            if (astTask != null)
            {
                var s = new SqlTask(astTask);
                ContainerLoweringEngine.LowerConnection(astTask.Connection, context);
                context.ParentObject.Children.Add(s);
                ContainerLoweringEngine.LowerEventHandlers(astTask, s, context);
            }
        }

        [Lowering(typeof(AstExecutePackageTaskNode))]
        public static void LowerExecutePackage(AstNode astNode, LoweringContext context)
        {
            var astTask = astNode as AstExecutePackageTaskNode;
            if (astTask != null)
            {
                var ep = new ExecutePackageTask(astTask);
                context.ParentObject.Children.Add(ep);
                ContainerLoweringEngine.LowerEventHandlers(astTask, ep, context);
            }
        }
    }
}
