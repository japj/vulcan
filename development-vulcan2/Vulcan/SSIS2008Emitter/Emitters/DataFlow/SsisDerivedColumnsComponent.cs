using System;
using System.Collections.Generic;
using System.Text;

using DTS = Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

using VulcanEngine.Common;
using Ssis2008Emitter.Properties;
using Ssis2008Emitter.Utility;
using Ssis2008Emitter.IR.DataFlow;
using Ssis2008Emitter.Emitters.Task;

namespace Ssis2008Emitter.Emitters.DataFlow
{
    [PhysicalIRMapping(typeof(DerivedColumns))]
    public class SsisDerivedColumnsComponent : SsisComponent, ISSISEmitter
    {
        public override string ClassID
        {
            get { return "DTSTransform.DerivedColumn"; }
        }

        public SsisDerivedColumnsComponent(Transformation t, SSISEmitterContext context) : base(t, context)
        {
            SetOutputName(_transformation.OutputPath);
        }

        public SSISEmitterContext Emit()
        {
            MessageEngine.Global.Trace(Severity.Notification, Resources.EmittingDerivedColumns, _transformation.Name);
            _dataFlowTask.ChainComponent(this, _transformation.InputPath);
            this.AddOutputColumns();
            return _context;
        }

        public void AddOutputColumns()
        {
            foreach (DerivedColumn column in ((DerivedColumns)_transformation).Columns)
            {
                if (column.ReplaceExisting)
                {
                    ReplaceExistingColumn(column);
                }
                else
                {
                    AddOutputColumn(column);
                }
            }
        }

        private void ReplaceExistingColumn(DerivedColumn column)
        {
            if (column.ReplaceExisting)
            {
                IDTSVirtualInput100 vi = _component.InputCollection[0].GetVirtualInput();

                IDTSInputColumn100 col;
                SetInputUsageType(vi, vi.VirtualInputColumnCollection[column.Name], DTSUsageType.UT_READWRITE);
                col = _component.InputCollection[0].InputColumnCollection[column.Name];

                col.CustomPropertyCollection["Expression"].Value = this.ExpressionCleanerAndInputMapBuilder(column.Expression, vi, DTSUsageType.UT_READONLY);
                col.CustomPropertyCollection["FriendlyExpression"].Value = column.Expression;
            }
        }

        private void AddOutputColumn(DerivedColumn column)
        {
            IDTSOutputColumn100 col;
            col = _component.OutputCollection[0].OutputColumnCollection.New();
            col.Name = column.Name;
            col.Description = column.Name;
            col.ErrorRowDisposition = DTSRowDisposition.RD_IgnoreFailure;
            col.TruncationRowDisposition = DTSRowDisposition.RD_FailComponent;
            col.SetDataTypeProperties(GetDataTypeFromString(column.Type), column.Length, column.Precision, column.Scale, column.Codepage);
            col.ExternalMetadataColumnID = 0;

            IDTSCustomProperty100 propExpression = col.CustomPropertyCollection.New();
            propExpression.Name = "Expression";
            propExpression.Value = column.Expression;

            IDTSCustomProperty100 propFriendlyExpression = col.CustomPropertyCollection.New();
            propFriendlyExpression.Name = "FriendlyExpression";
            propFriendlyExpression.Value = column.Expression;

            IDTSVirtualInput100 vi = _component.InputCollection[0].GetVirtualInput();

            propExpression.Value = this.ExpressionCleanerAndInputMapBuilder(column.Expression, vi, DTSUsageType.UT_READONLY);
        }

        private static DataType GetDataTypeFromString(string typeAsString)
        {
            DataType type = DataType.DT_STR;

            switch (typeAsString.ToUpperInvariant())
            {
                case "BOOL": type = DataType.DT_BOOL; break;
                case "FLOAT": type = DataType.DT_R4; break;
                case "DOUBLE": type = DataType.DT_R8; break;
                case "WSTR": type = DataType.DT_WSTR; break;
                case "INT32": type = DataType.DT_I4; break;
                case "INT64": type = DataType.DT_I8; break;
                case "UINT32": type = DataType.DT_UI4; break;
                case "UINT64": type = DataType.DT_UI8; break;
                case "STR": type = DataType.DT_STR; break;
                case "DATE": type = DataType.DT_DBDATE; break;
                case "TIME": type = DataType.DT_DBTIME; break;
                case "TIMESTAMP": type = DataType.DT_DBTIMESTAMP; break;
                case "TIMESTAMP2": type = DataType.DT_DBTIMESTAMP2; break;
                default: MessageEngine.Global.Trace(Severity.Error, "Error in Type {0} - Unhandled VULCAN SSIS Type", typeAsString); break;
            }
            return type;
        }
    }
}
