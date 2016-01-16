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
    [PhysicalIRMapping(typeof(UnionAll))]
    public class SsisUnionAllComponent : SsisComponent, ISSISEmitter
    {
        public override string ClassID
        {
            get { return "DTSTransform.UnionAll"; }
        }

        public SsisUnionAllComponent(Transformation transformation, SSISEmitterContext context) : base(transformation, context)
        {
            SetOutputName(_transformation.OutputPath);
        }
        
        public SSISEmitterContext Emit()
        {
            this.ProcessInput(_dataFlowTask);
            _dataFlowTask.AddComponent(this);
            return _context;
        }
        
        public void ProcessInput(SsisPipelineTask dataFlowTask)
        {
            int nNewInput = ((UnionAll)_transformation).InputPathList.Count - _component.InputCollection.Count;
            if (nNewInput > 0)
            {
                for (int i = 0; i < nNewInput; i++)
                {
                    _component.InputCollection.New();
                }
            }

            for (int i = 0; i < ((UnionAll)_transformation).InputPathList.Count; i++)
            {
                UnionInputPath inputPath = ((UnionAll)_transformation).InputPathList[i];

                IDTSInput100 input = _component.InputCollection[i];

                input.Name = inputPath.Source + "_" + inputPath.Name;

                dataFlowTask.ChainComponent(this, inputPath, input);

                foreach (UnionMapping unionMapping in inputPath.MappingList)
                {
                    MapInput(input, unionMapping);
                }
            }
        }

        private void MapInput(IDTSInput100 input, UnionMapping mapping)
        {
            string inputColumn = mapping.Input;
            string outputColumn = mapping.Output == null ? mapping.Input : mapping.Output;

            IDTSInputColumn100 newInputColumn = input.InputColumnCollection[inputColumn];
            if (newInputColumn == null)
            {
                newInputColumn = input.InputColumnCollection.New();
            }
            newInputColumn.Name = inputColumn;

            const string strPropertyName = "OutputColumnLineageID";
            IDTSCustomProperty100 newInputColumnCustomProperty = null;
            foreach (IDTSCustomProperty100 property in newInputColumn.CustomPropertyCollection)
            {
                if (property.Name == strPropertyName)
                {
                    newInputColumnCustomProperty = property;
                    break;
                }
            }
            if (newInputColumnCustomProperty == null)
            {
                newInputColumnCustomProperty = newInputColumn.CustomPropertyCollection.New();
            }

            newInputColumnCustomProperty.Name = "OutputColumnLineageID";
            newInputColumnCustomProperty.Value = _component.OutputCollection[0].OutputColumnCollection[outputColumn].LineageID;
        }
    }
}
