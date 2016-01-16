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
    [PhysicalIRMapping(typeof(TermLookup))]
    public class SsisTermLookupComponent : SsisComponent, ISSISEmitter
    {
        public override string ClassID
        {
            get { return "DTSTransform.TermLookup"; }
        }

        public SsisTermLookupComponent(Transformation t, SSISEmitterContext context) : base(t, context)
        {
            InitializeConnection(((TermLookup)_transformation).Connection);
            SetOutputName(_transformation.OutputPath);
            SetComponentProperties();
        }

        public SSISEmitterContext Emit()
        {
            _dataFlowTask.ChainComponent(this, _transformation.InputPath);
            this.MapInput();
            return _context;
        }

        private void SetComponentProperties()
        {
            TermLookup termLookup = (TermLookup)_transformation;
            _instance.SetComponentProperty("IsCaseSensitive", termLookup.IsCaseSensitive);
            _instance.SetComponentProperty("RefTermColumn", termLookup.RefTermColumn);
            _instance.SetComponentProperty("RefTermTable", termLookup.RefTermTable);

            _component.OutputCollection[0].ErrorRowDisposition = DTSRowDisposition.RD_IgnoreFailure;
        }

        public void MapInput()
        {
            foreach (InputColumn column in ((TermLookup)_transformation).InputColumnList)
            {
                SetInputUsageType(column.Name, DTSUsageType.UT_READONLY);

                _instance.SetInputColumnProperty(
                    _component.InputCollection[0].ID,
                    _component.InputCollection[0].InputColumnCollection[column.Name].ID,
                    "InputColumnType",
                    (int)((InputColumnUsageType)Enum.Parse(typeof(InputColumnUsageType), column.UsageType))
                );
            }
        }

        private enum InputColumnUsageType
        {
            PassThrough,
            Lookup,
            BothPassThroughAndLookup
        }
    }
}
