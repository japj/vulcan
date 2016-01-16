using System;
using System.Collections.Generic;
using System.Text;

using DTS = Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

using VulcanEngine.Common;
using Ssis2008Emitter.Properties;
using Ssis2008Emitter.Utility;
using Ssis2008Emitter.IR.DataFlow;
using Ssis2008Emitter.Emitters.Task;

namespace Ssis2008Emitter.Emitters.DataFlow
{
    [PhysicalIRMapping(typeof(Lookup))]
    public class SsisLookupComponent : SsisComponent, ISSISEmitter
    {
        public override string ClassID
        {
            get { return "DTSTransform.Lookup"; }
        }

        public SsisLookupComponent(Transformation t, SSISEmitterContext context) : base(t, context)
        {
            InitializeConnection(((Lookup)_transformation).Connection);
            SetOutputName(_transformation.OutputPath);
            SetComponentProperties();
        }

        public SSISEmitterContext Emit()
        {
            MessageEngine.Global.Trace(Severity.Notification, Resources.EmittingLookup, _transformation.Name);
            _dataFlowTask.ChainComponent(this, _transformation.InputPath);

            this.MapInput();
            this.MapOutput();

            return _context;
        }

        private void SetComponentProperties()
        {
            Lookup lookup = (Lookup)_transformation;
            string query;

            if (lookup.Table != null)
            {
                query = null; // TODO: new SelectEmitter().Emit(lookup.Table.TextContent, "*", null);
            }
            else
            {
                query = lookup.Query;
            }

            _instance.SetComponentProperty("SqlCommand", query);

            _component.OutputCollection[0].ErrorRowDisposition = DTSRowDisposition.RD_IgnoreFailure;
        }

        public void MapInput()
        {
            foreach (LookupJoin input in ((Lookup)_transformation).InputList)
            {
                SetInputUsageType(input.Name, DTSUsageType.UT_READONLY);
                
                _instance.SetInputColumnProperty(
                    _component.InputCollection[0].ID,
                    _component.InputCollection[0].InputColumnCollection[input.Name].ID,
                    "JoinToReferenceColumn",
                    input.ReferenceColumnName
                    );
            }
        }

        public void MapOutput()
        {
            foreach (LookupJoin output in ((Lookup)_transformation).OutputList)
            {
                _instance.InsertOutputColumnAt(
                    _component.OutputCollection[0].ID,
                    _component.OutputCollection[0].OutputColumnCollection.Count,
                    output.Name,
                    output.Name
                    );

                _instance.SetOutputColumnProperty(
                    _component.OutputCollection[0].ID,
                    _component.OutputCollection[0].OutputColumnCollection[output.Name].ID,
                    "CopyFromReferenceColumn",
                    output.ReferenceColumnName
                    );
            }
        }
    }
}
