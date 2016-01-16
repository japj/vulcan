using System;
using System.Collections.Generic;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.Framework;
using VulcanEngine.IR.Ast.Task;
using AST = VulcanEngine.IR.Ast;
using DTS = Microsoft.SqlServer.Dts.Runtime;

namespace Ssis2008Emitter.IR.Tasks
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Name matches naming convention for SSIS.")]
    public class EventHandler : Container
    {
        private AstTaskEventHandlerNode _astTaskEventHandlerNode;
        private DTS.DtsEventHandler _eventHandler;
        private Executable _hostExecutable;
        private string _eventName;
        
        public EventHandler(AstTaskEventHandlerNode astNode, Executable hostExecutable)
            : base(astNode.EventType.ToString()) 
        {
            _astTaskEventHandlerNode = astNode;
            _hostExecutable = hostExecutable;
            _eventName = _astTaskEventHandlerNode.EventType.ToString();
        }

        public override Microsoft.SqlServer.Dts.Runtime.DtsContainer DtsContainer
        {
            get { return _eventHandler; }
        }

        public override Microsoft.SqlServer.Dts.Runtime.Executable DtsExecutable
        {
            get { return _eventHandler; }
        }

        public override Microsoft.SqlServer.Dts.Runtime.IDTSPropertiesProvider PropertyProvider
        {
            get { return _eventHandler; }
        }

        public override Microsoft.SqlServer.Dts.Runtime.IDTSSequence DtsSequence
        {
            get { return _eventHandler; }
        }

        public override string Moniker
        {
            get { throw new InvalidOperationException(); }
        }

        public override Microsoft.SqlServer.Dts.Runtime.EventsProvider DtsEventsProvider
        {
            get { throw new NotSupportedException(); }
        }

        public override void Emit(SsisEmitterContext context)
        {
            _eventHandler = (DTS.DtsEventHandler)_hostExecutable.DtsEventsProvider.EventHandlers.Add(_eventName);

            context = new SsisEmitterContext(context.Package, this, context.ProjectManager);
            foreach (PhysicalObject po in this.Children)
            {
                po.Initialize(context);
                po.Emit(context);
            }
        }
    }
}
