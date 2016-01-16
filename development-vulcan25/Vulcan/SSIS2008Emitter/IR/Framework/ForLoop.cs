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
    public class ForLoop : Container
    {
        private DTS.ForLoop _forLoop;
        private string _initializerExpression = string.Empty;
        private string _loopTestExpression = string.Empty;
        private string _countingExpression = string.Empty;

        public ForLoop(AST.Task.AstForLoopContainerTaskNode astNode)
            : base(astNode)
        {
            _initializerExpression = astNode.InitializerExpression;
            _loopTestExpression = astNode.LoopTestExpression;
            _countingExpression = astNode.CountingExpression;
        }

        public string InitializerExpression
        {
            get { return _initializerExpression; }
            set { _initializerExpression = value; }
        }

        public string LoopTestExpression
        {
            get { return _loopTestExpression; }
            set { _loopTestExpression = value; }
        }

        public string CountingExpression
        {
            get { return _countingExpression; }
            set { _countingExpression = value; }
        }

        public override EventsProvider DtsEventsProvider
        {
            get { return _forLoop; }
        }

        public override string Moniker
        {
            get { return "STOCK:FORLOOP"; }
        }

        public override IDTSPropertiesProvider PropertyProvider
        {
            get { return _forLoop; }
        }

        public override Executable DtsExecutable
        {
            get { return _forLoop; }
        }

        public override IDTSSequence DtsSequence
        {
            get { return _forLoop; }
        }

        public override DtsContainer DtsContainer
        {
            get { return _forLoop; }
        }

        public override void Emit(SsisEmitterContext context)
        {
            try
            {
                _forLoop = (DTS.ForLoop)context.ParentContainer.AppendExecutable(Moniker);
                context.ParentContainer.ProcessTaskBinding(this);
                _forLoop.Name = Name;
                _forLoop.TransactionOption = (DTSTransactionOption)Enum.Parse(typeof(DTSTransactionOption), TransactionMode);
                _forLoop.AssignExpression = _countingExpression;
                _forLoop.EvalExpression = _loopTestExpression;
                _forLoop.InitExpression = _initializerExpression;
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
