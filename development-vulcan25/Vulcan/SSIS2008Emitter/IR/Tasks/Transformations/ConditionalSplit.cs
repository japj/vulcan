using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.Phases.Lowering.Framework;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Transformation;

namespace Ssis2008Emitter.IR.Tasks.Transformations
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public class ConditionalSplit : Transformation
    {
        private readonly AstConditionalSplitNode _astConditionalSplitNode;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        public static void CreateAndRegister(AstNode astNode, LoweringContext context)
        {
            context.ParentObject.Children.Add((new ConditionalSplit(context, astNode)));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Required for Emitter pattern.")]
        public ConditionalSplit(LoweringContext context, AstNode astNode) : base(context, astNode as AstTransformationNode)
        {
            _astConditionalSplitNode = astNode as AstConditionalSplitNode;
            RegisterInputBinding(_astConditionalSplitNode);
        }

        public override string Moniker
        {
            get { return "DTSTransform.ConditionalSplit"; }
        }

        public override void Initialize(SsisEmitterContext context)
        {
            base.Initialize(context);
            Flush();
        }

        public override void Emit(SsisEmitterContext context)
        {
            ProcessBindings(context);

            // Configure Default Ouput Path
            Component.OutputCollection[0].Name = _astConditionalSplitNode.DefaultOutputPath.Name;

            foreach (AstConditionalSplitOutputPathNode opn in _astConditionalSplitNode.OutputPaths)
            {
                AppendOutputPath(opn.Name, opn.Expression);
            }
        }

        private int _evaluationOrder;

        private void AppendOutputPath(string name, string expression)
        {
            IDTSOutput100 newPath = Component.OutputCollection.New();
            newPath.Name = name;
            ////newPath.Description = name; // No need to set description to same value as name

            newPath.ExclusionGroup = Component.OutputCollection[0].ExclusionGroup;
            newPath.SynchronousInputID = Component.OutputCollection[0].SynchronousInputID;
            newPath.ErrorOrTruncationOperation = "Computation";
            newPath.ErrorRowDisposition = DTSRowDisposition.RD_IgnoreFailure;
            newPath.TruncationRowDisposition = DTSRowDisposition.RD_FailComponent;

            IDTSCustomProperty100 propExpression = newPath.CustomPropertyCollection.New();
            propExpression.Name = "Expression";
            IDTSVirtualInput100 vi = Component.InputCollection[0].GetVirtualInput();
            ////propExpression.Value = ExpressionHandler.ExpressionCleanerAndInputMapBuilder(expression, this, vi, DTSUsageType.UT_READONLY);
            Expression exp = ExpressionHandler.ExpressionCleanerAndInputMapBuilder(expression.Trim(), this, vi, DTSUsageType.UT_READONLY);
            propExpression.Value = exp.ProcessedExpression;
            propExpression.ContainsID = exp.ContainsId;
            ////propExpression = ExpressionHandler.ExpressionCleanerAndInputMapBuilder(propExpression, this, vi, DTSUsageType.UT_READONLY);

            IDTSCustomProperty100 propFriendlyExpression = newPath.CustomPropertyCollection.New();
            propFriendlyExpression.Name = "FriendlyExpression";
            propFriendlyExpression.Value = exp.FriendlyExpression;
            propFriendlyExpression.ExpressionType = DTSCustomPropertyExpressionType.CPET_NOTIFY;
            propFriendlyExpression.ContainsID = exp.ContainsId;
            
            IDTSCustomProperty100 propEvaluationOrder = newPath.CustomPropertyCollection.New();
            propEvaluationOrder.Name = "EvaluationOrder";
            propEvaluationOrder.Value = _evaluationOrder;

            Component.OutputCollection.SetIndex(Component.OutputCollection.GetObjectIndexByID(newPath.ID), _evaluationOrder);
            _evaluationOrder++;
        }
    }
}

