using System;
using System.Globalization;
using AstFramework;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.Framework;
using VulcanEngine.IR.Ast.Task;
using VulcanEngine.Common;
using AST = VulcanEngine.IR.Ast;
using DTS = Microsoft.SqlServer.Dts.Runtime;

namespace Ssis2008Emitter.IR.Tasks
{
    public enum ContainerConstraintMode
    {
        Linear,
        Parallel,
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public abstract class Container : Executable
    {
        // TODO: Why is this unused?
        ////private readonly List<Variable> _variables = new List<Variable>();

        private DTS.Executable _lastExecutable;

        public System.Data.IsolationLevel ContainerIsolationLevel { get; set; }

        public string TransactionMode { get; set; }

        public ContainerConstraintMode ConstraintMode { get; set; }

        public bool DelayValidation { get; set; }

        protected Container(string name)
            : base(name)
        {
            ContainerIsolationLevel = System.Data.IsolationLevel.Serializable;
            TransactionMode = "Supported";
            ConstraintMode = ContainerConstraintMode.Linear;
            DelayValidation = false;
        }

        protected Container(AstContainerTaskNode astNode)
            : base(astNode)
        {
            DelayValidation = astNode.DelayValidation;

            switch (astNode.IsolationLevel)
            {
                case IsolationLevel.Chaos:
                    ContainerIsolationLevel = System.Data.IsolationLevel.Chaos;
                    break;
                case IsolationLevel.ReadCommitted:
                    ContainerIsolationLevel = System.Data.IsolationLevel.ReadCommitted;
                    break;
                case IsolationLevel.ReadUncommitted:
                    ContainerIsolationLevel = System.Data.IsolationLevel.ReadUncommitted;
                    break;
                case IsolationLevel.RepeatableRead:
                    ContainerIsolationLevel = System.Data.IsolationLevel.RepeatableRead;
                    break;
                case IsolationLevel.Serializable:
                    ContainerIsolationLevel = System.Data.IsolationLevel.Serializable;
                    break;
                case IsolationLevel.Snapshot:
                    ContainerIsolationLevel = System.Data.IsolationLevel.Snapshot;
                    break;
                case IsolationLevel.Unspecified:
                    ContainerIsolationLevel = System.Data.IsolationLevel.Unspecified;
                    break;
                default:
                    ContainerIsolationLevel = System.Data.IsolationLevel.Serializable;
                    break;
            }

            switch (astNode.ConstraintMode)
            {
                case VulcanEngine.IR.Ast.Task.ContainerConstraintMode.Linear:
                    ConstraintMode = ContainerConstraintMode.Linear;
                    break;
                case VulcanEngine.IR.Ast.Task.ContainerConstraintMode.Parallel:
                    ConstraintMode = ContainerConstraintMode.Parallel;
                    break;
                default:
                    ConstraintMode = ContainerConstraintMode.Linear;
                    break;
            }

            switch (astNode.TransactionMode)
            {
                case ContainerTransactionMode.StartOrJoin:
                    TransactionMode = "Required";
                    if (astNode.ConstraintMode == VulcanEngine.IR.Ast.Task.ContainerConstraintMode.Parallel)
                    {
                        VulcanEngine.Common.MessageEngine.Trace(
                            astNode,
                            Severity.Alert,
                            "CT001",
                                                                "Container {0} has ConstraintMode of {1} and TransactionOption of {2}.  SSIS does not support transactions when the package flow is not linear.  Please configure precedence constraints appropriately for this scenario.",
                                                                astNode.Name,
                                                                astNode.ConstraintMode,
                                                                astNode.TransactionMode);
                    }

                    break;
                case ContainerTransactionMode.Join:
                    TransactionMode = "Supported";
                    break;
                case ContainerTransactionMode.NoTransaction:
                    TransactionMode = "NotSupported";
                    break;
                default:
                    TransactionMode = "Supported";
                    break;
            }
        }

        public DTS.Executable AppendExecutable(string moniker)
        {
            DTS.Executable executable = DtsSequence.Executables.Add(moniker);
            return executable;
        }

        public void ProcessTaskBinding(Executable executable)
        {
            if (executable.BindingList != null && executable.BindingList.Count > 0)
            {
                foreach (var binding in executable.BindingList)
                {
                    var parentExecutable = DtsSequence.Executables[((AST.AstNamedNode)binding.AstTaskflowInput.OutputPath.ParentItem).Name];
                    if (parentExecutable == null)
                    {
                        throw new InvalidOperationException("Could not find " + binding.AstTaskflowInput.OutputPath.Name);
                    }

                    var constraint = DtsSequence.PrecedenceConstraints.Add(parentExecutable, executable.DtsExecutable);
                    constraint.Name = GetPrecedenceConstraintEmissionName(binding.AstTaskflowPrecedenceConstraints, binding.AstTaskflowInput);
                    switch (binding.AstTaskflowInput.EvaluationOperation)
                    {
                        case TaskEvaluationOperationType.Constraint:
                            constraint.EvalOp = Microsoft.SqlServer.Dts.Runtime.DTSPrecedenceEvalOp.Constraint;
                            break;
                        case TaskEvaluationOperationType.Expression:
                            constraint.EvalOp = Microsoft.SqlServer.Dts.Runtime.DTSPrecedenceEvalOp.Expression;
                            constraint.Expression = binding.AstTaskflowInput.Expression;
                            break;
                        case TaskEvaluationOperationType.ExpressionAndConstraint:
                            constraint.EvalOp = Microsoft.SqlServer.Dts.Runtime.DTSPrecedenceEvalOp.ExpressionAndConstraint;
                            constraint.Expression = binding.AstTaskflowInput.Expression;
                            break;
                        case TaskEvaluationOperationType.ExpressionOrConstraint:
                            constraint.EvalOp = Microsoft.SqlServer.Dts.Runtime.DTSPrecedenceEvalOp.ExpressionOrConstraint;
                            constraint.Expression = binding.AstTaskflowInput.Expression;
                            break;
                        default:
                            throw new NotImplementedException(String.Format(CultureInfo.InvariantCulture, "TaskEvaluationOperationType {0} is not supported.", binding.AstTaskflowInput.EvaluationOperation));
                    }

                    switch (binding.AstTaskflowInput.EvaluationValue)
                    {
                        case TaskEvaluationOperationValue.Completion:
                            constraint.Value = Microsoft.SqlServer.Dts.Runtime.DTSExecResult.Completion;
                            break;
                        case TaskEvaluationOperationValue.Failure:
                            constraint.Value = Microsoft.SqlServer.Dts.Runtime.DTSExecResult.Failure;
                            break;
                        case TaskEvaluationOperationValue.Success:
                            constraint.Value = Microsoft.SqlServer.Dts.Runtime.DTSExecResult.Success;
                            break;
                        default:
                            throw new NotImplementedException(String.Format(CultureInfo.InvariantCulture, "EvaluationValue {0} is not supported.", binding.AstTaskflowInput.EvaluationValue));
                    }

                    switch (binding.AstTaskflowPrecedenceConstraints.LogicalType)
                    {
                        case LogicalOperationType.And:
                            constraint.LogicalAnd = true;
                            break;
                        case LogicalOperationType.Or:
                            constraint.LogicalAnd = false;
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                if (ConstraintMode == ContainerConstraintMode.Linear)
                {
                    if (_lastExecutable != null)
                    {
                        var constraint = DtsSequence.PrecedenceConstraints.Add(_lastExecutable, executable.DtsExecutable);
                        constraint.Name = String.Format(CultureInfo.InvariantCulture, "__DefaultLinear_Sink_{0}", executable.Name);
                    }

                    if (DtsSequence.Executables.Count > 0)
                    {
                        _lastExecutable = executable.DtsExecutable;
                    }
                }
            }
        }

        private static string GetPrecedenceConstraintEmissionName(AstTaskflowPrecedenceConstraintsNode astTaskflowPrecedenceConstraintsNode, AstTaskflowInputPathNode astTaskflowInputPathNode)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "__PC_{0}_{1}_{2}_{3}_{4}_{5}",
                ((AST.AstNamedNode)astTaskflowInputPathNode.OutputPath.ParentItem).Name,
                ((AST.AstNamedNode)astTaskflowInputPathNode.ParentItem.ParentItem).Name,
                (int)astTaskflowInputPathNode.EvaluationOperation,
                (int)astTaskflowInputPathNode.EvaluationValue,
                astTaskflowInputPathNode.Expression == null ? String.Empty : astTaskflowInputPathNode.Expression.GetHashCode().ToString(CultureInfo.InvariantCulture),
                (int)astTaskflowPrecedenceConstraintsNode.LogicalType);
        }

        public abstract DTS.IDTSSequence DtsSequence { get; }

        public abstract DTS.DtsContainer DtsContainer { get; }

        public override void Emit(SsisEmitterContext context)
        {
            DtsContainer.IsolationLevel = ContainerIsolationLevel;
            DtsContainer.DelayValidation = DelayValidation;

            context = new SsisEmitterContext(context.Package, this, context.ProjectManager);

            foreach (PhysicalObject po in this.Children)
            {
                try
                {
                    po.Initialize(context);
                    po.Emit(context);
                }
                catch (DTS.DtsException e)
                {
                    if (e.ErrorCode == -1073659647)
                    {
                        MessageEngine.Trace(AstNamedNode, Severity.Error, "V1050", "Attempted to reuse the name '{0}' within an SSIS package, which is illegal. Use a unique name.\nContainer: {1}\nTask: {0}", po.Name, this.Name);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}
