using System;
using AstFramework;
using Microsoft.SqlServer.Dts.Runtime;
using Ssis2008Emitter.IR.Common;
using VulcanEngine.Common;
using AST = VulcanEngine.IR.Ast;
using DTS = Microsoft.SqlServer.Dts.Runtime;

namespace Ssis2008Emitter.IR.Tasks
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public class Sequence : Container
    {
        private DTS.Sequence _sequenceContainer;

        public Sequence(AST.Task.AstContainerTaskNode astNode) : base(astNode)
        {
        }

        public override EventsProvider DtsEventsProvider
        {
            get { return _sequenceContainer; }
        }

        public override string Moniker
        {
            ////TODO: Validate - changed from lower case (sequence) to upper (SEQUENCE) 
            get { return "STOCK:SEQUENCE"; }
        }

        public override IDTSPropertiesProvider PropertyProvider
        {
            get { return _sequenceContainer; }
        }

        public override Executable DtsExecutable
        {
            get { return _sequenceContainer; }
        }

        public override IDTSSequence DtsSequence
        {
            get { return _sequenceContainer; }
        }

        public override DtsContainer DtsContainer
        {
            get { return _sequenceContainer; }
        }

        public override void Emit(SsisEmitterContext context)
        {
            try
            {
               _sequenceContainer = (DTS.Sequence)context.ParentContainer.AppendExecutable(Moniker);
                context.ParentContainer.ProcessTaskBinding(this);
                _sequenceContainer.Name = Name;
                _sequenceContainer.TransactionOption = (DTSTransactionOption)Enum.Parse(typeof(DTSTransactionOption), TransactionMode);

                base.Emit(context);
            }
            catch (DtsException e)
            {
                if (e.ErrorCode == -1073659647)
                {
                    MessageEngine.Trace(AstNamedNode, Severity.Error, "V1050", "Attempted to reuse the name '{0}' within an SSIS package, which is illegal. Use a unique name.", Name);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
