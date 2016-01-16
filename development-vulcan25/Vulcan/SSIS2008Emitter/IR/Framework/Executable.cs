using System.Collections.Generic;
using AstFramework;
using Ssis2008Emitter.IR.Common;
using VulcanEngine.Common;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Task;
using DTS = Microsoft.SqlServer.Dts.Runtime;

namespace Ssis2008Emitter.IR.Framework
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public abstract class Executable : PhysicalObject, IPhysicalPropertiesProvider
    {
        protected AstNamedNode AstNamedNode { get; private set; }

        protected Executable(string name) : base(name) 
        {
            BindingList = new List<ExecutableBinding>();
        }

        protected Executable(AstNamedNode astNamedNode) : this(astNamedNode.Name, astNamedNode)
        {
        }

        protected Executable(string name, AstNamedNode astNamedNode) : this(name)
        {
            AstNamedNode = astNamedNode;
        }

        public abstract string Moniker { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces", Justification = "Required for Emitter pattern.")]
        public abstract DTS.IDTSPropertiesProvider PropertyProvider { get; }

        public abstract DTS.Executable DtsExecutable { get; }

        public abstract DTS.EventsProvider DtsEventsProvider { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        public List<ExecutableBinding> BindingList { get; private set; }

        public override void Initialize(SsisEmitterContext context)
        {
            InitializeBindings();
        }

        protected virtual void InitializeBindings()
        {
            var astTaskNode = this.AstNamedNode as AstTaskNode;
            if (astTaskNode != null && astTaskNode.PrecedenceConstraints != null)
            {
                foreach (var astTaskflow in astTaskNode.PrecedenceConstraints.Inputs)
                {
                    BindingList.Add(new ExecutableBinding(astTaskNode.PrecedenceConstraints, astTaskflow));
                }
            }
        }

        public bool ExecuteDuringDesignTime { get; set; }

        public void TryExecuteDuringDesignTime()
        {
            if (ExecuteDuringDesignTime)
            {
                Execute();
            }
        }

        public void Execute()
        {
            if (DtsExecutable != null)
            {
                var errorHandler = new ErrorEvents();
                DTS.DTSExecResult execResult = DtsExecutable.Execute(null, null, errorHandler, null, null);
                if (execResult != Microsoft.SqlServer.Dts.Runtime.DTSExecResult.Success || errorHandler.ValidationErrorCount > 0)
                {
                    MessageEngine.Trace(AstNamedNode, Severity.Warning, "V0108", "Failed to execute {0} during compilation", Name);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces", Justification = "Required for Emitter pattern.")]
        public virtual void SetExpression(string name, string expression)
        {
            PropertyProvider.SetExpression(name, expression);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces", Justification = "Required for Emitter pattern.")]
        public virtual void SetProperty(string name, object value)
        {
            if (PropertyProvider.Properties.Contains(name))
            {
                PropertyProvider.Properties[name].SetValue(PropertyProvider, value);
            }
        }
    }
}
