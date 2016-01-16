using System;
using System.Collections.Generic;
using System.Text;

using DTS = Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

using Ssis2008Emitter.Utility;
using Ssis2008Emitter.IR.DataFlow;
using Ssis2008Emitter.Emitters.Task;

namespace Ssis2008Emitter.Emitters.DataFlow
{
    [PhysicalIRMapping(typeof(ConditionalSplit))]
    public class SsisConditionalSplitComponent : SsisComponent, ISSISEmitter
    {
        int _iEvaluationOrder;

        public override string ClassID
        {
            get { return "DTSTransform.ConditionalSplit"; }
        }

        public SsisConditionalSplitComponent(Transformation t, SSISEmitterContext context) : base(t, context)
        {
        }

        public SSISEmitterContext Emit()
        {
            _dataFlowTask.ChainComponent(this, _transformation.InputPath);
            this.ProcessOutput();
            return _context;
        }

        public void ProcessOutput()
        {
            _iEvaluationOrder = 0;

            string defaultOutputPath = ((ConditionalSplit)_transformation).DefaultCondition.OutputPath;
            SetOutputName(defaultOutputPath);
            AddDefaultOutput(defaultOutputPath);
            
            foreach (Condition condition in ((ConditionalSplit)_transformation).ConditionList)
            {
                SetOutputProperties(condition, defaultOutputPath);
            }
        }

        private void AddDefaultOutput(string defaultOutputPath)
        {
            _component.OutputCollection.SetIndex(_component.OutputCollection.GetObjectIndexByID(_component.OutputCollection[defaultOutputPath].ID), 0);
        }

        private void SetOutputProperties(Condition condition, string defaultOutputPath)
        {
            IDTSOutput100 newPath = _component.OutputCollection.New();
            newPath.Name = condition.OutputPath;
            newPath.Description = condition.OutputPath;
//            newPath.ExclusionGroup = _component.OutputCollection["Conditional Split Default Output"].ExclusionGroup;
//            newPath.SynchronousInputID = _component.OutputCollection["Conditional Split Default Output"].SynchronousInputID;
            newPath.ExclusionGroup = _component.OutputCollection[defaultOutputPath].ExclusionGroup;
            newPath.SynchronousInputID = _component.OutputCollection[defaultOutputPath].SynchronousInputID;
            newPath.ErrorOrTruncationOperation = "Computation";
            newPath.ErrorRowDisposition = DTSRowDisposition.RD_IgnoreFailure;
            newPath.TruncationRowDisposition = DTSRowDisposition.RD_FailComponent;

            IDTSCustomProperty100 propEvaluationOrder = newPath.CustomPropertyCollection.New();
            propEvaluationOrder.Name = "EvaluationOrder";
            propEvaluationOrder.Value = _iEvaluationOrder;

            IDTSCustomProperty100 propFriendlyExpression = newPath.CustomPropertyCollection.New();
            propFriendlyExpression.Name = "FriendlyExpression";
            propFriendlyExpression.Value = condition.Expression;

            IDTSCustomProperty100 propExpression = newPath.CustomPropertyCollection.New();
            propExpression.Name = "Expression";
            IDTSVirtualInput100 vi = _component.InputCollection[0].GetVirtualInput();
            propExpression.Value = ExpressionCleanerAndInputMapBuilder(condition.Expression, vi, DTSUsageType.UT_READONLY); ;

            _component.OutputCollection.SetIndex(_component.OutputCollection.GetObjectIndexByID(newPath.ID), 0);
            _iEvaluationOrder++;
        }
    }
}
