using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.Framework.Connections;
using VulcanEngine.IR.Ast.Task;
using Dts = Microsoft.SqlServer.Dts;

namespace Ssis2008Emitter.IR.Tasks
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public class ExecutePackageTask : Task
    {
        private readonly string _relativePath;

        public ExecutePackageTask(AstExecutePackageTaskNode astExecutePackageTaskNode) : base(astExecutePackageTaskNode)
        {
            _relativePath = astExecutePackageTaskNode.RelativePath;
        }

        protected Dts.Tasks.ExecutePackageTask.ExecutePackageTask ExecutePackageTaskObject
        {
            get { return (Dts.Tasks.ExecutePackageTask.ExecutePackageTask)DtsTaskHost.InnerObject; }
        }

        public override string Moniker
        {
            get { return "STOCK:ExecutePackageTask"; }
        }

        public override Dts.Runtime.Executable DtsExecutable
        {
            get { return DtsTaskHost; }
        }

        public override void Initialize(SsisEmitterContext context)
        {
            base.Initialize(context);
            ////DtsTaskHost.Name = StringManipulation.NameCleanerAndUniqifier(Name + _relativePath);
            DtsTaskHost.Name = Name;
        }

        public override void Emit(SsisEmitterContext context)
        {
            base.Emit(context);
            var fc = new FileConnection(Name + _relativePath, _relativePath);
            fc.Initialize(context);
            fc.Emit(context);
            ExecutePackageTaskObject.Connection = fc.Name;
        }
    }
}
