using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using DTS = Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

using VulcanEngine.Common;
using Ssis2008Emitter.Utility;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.DataFlow;
using Ssis2008Emitter.Emitters.Framework;
using Ssis2008Emitter.Emitters.Task;

namespace Ssis2008Emitter.Emitters.DataFlow
{
    public abstract class SsisComponent
    {
        protected SsisPipelineTask _dataFlowTask;
        protected Transformation _transformation;
        protected SSISEmitterContext _context;
        protected IDTSComponentMetaData100 _component;
        protected CManagedComponentWrapper _instance;
        protected SsisConnection _sourceConnection;

        public abstract string ClassID { get; }

        public IDTSComponentMetaData100 Component
        {
            get { return _component; }
        }

        public CManagedComponentWrapper Instance
        {
            get { return _instance; }
        }

        public SsisComponent(Transformation transformation, SSISEmitterContext context)
        {
            if (!context.HasSSISDataFlowTask)
            {
                // TODO: Message.Trace(Severity.Error)
            }

            _transformation = transformation;
            _dataFlowTask = context.SSISDataFlowTask;
            _context = context;

            _component = _dataFlowTask.NewComponentMetaData();
            _component.ComponentClassID = ClassID;

            _instance = _component.Instantiate();
            _instance.ProvideComponentProperties();

            _component.Name = _transformation.Name;
            _component.ValidateExternalMetadata = transformation.ValidateExternalMetadata;
        }

        protected void InitializeConnection(string sConnection)
        {
            _sourceConnection = new SsisConnection(sConnection);

            if (_component.RuntimeConnectionCollection.Count > 0)
            {
                _component.RuntimeConnectionCollection[0].ConnectionManager =
                    DTS.DtsConvert.GetExtendedInterface(_sourceConnection.ConnectionManager);
                _component.RuntimeConnectionCollection[0].ConnectionManagerID = _sourceConnection.ConnectionManager.ID;
            }
        }

        public void Flush()
        {
            try
            {
                _instance.AcquireConnections(null);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                string strErr = _component.GetErrorDescription(e.ErrorCode);
                try
                {
                    string temp = _component.GetErrorDescription(e.ErrorCode).Replace("%1", "{0}").Replace("%2", "{1:X}");
                    strErr = string.Format(temp, _sourceConnection.ConnectionManager.Name, e.ErrorCode);
                }
                catch (Exception)
                {
                    strErr = _component.GetErrorDescription(e.ErrorCode);
                }
                MessageEngine.Global.Trace(Severity.Warning, strErr);
            }

            try
            {
                _instance.ReinitializeMetaData();
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                MessageEngine.Global.Trace(Severity.Warning, _component.GetErrorDescription(e.ErrorCode));
            }
            _instance.ReleaseConnections();
        }

        protected void Validate()
        {
            DTSValidationStatus dtsStatus = _instance.Validate();

            // TODO: Error messages
            if (dtsStatus != DTSValidationStatus.VS_ISVALID)
            {
                if (dtsStatus == DTSValidationStatus.VS_NEEDSNEWMETADATA)
                {
                    _instance.ReinitializeMetaData();
                }
            }
        }

        protected virtual void SetOutputName(OutputPath outputPath)
        {
            if (outputPath != null)
            {
                SetOutputName(outputPath.Name);
            }
        }

        protected virtual void SetOutputName(string outputPathName)
        {
            if (!string.IsNullOrEmpty(outputPathName))
            {
                _component.OutputCollection[0].Name = outputPathName;
            }
        }

        protected virtual void SetInputUsageType(string name, DTSUsageType usageType)
        {
            IDTSVirtualInput100 vi = Component.InputCollection[0].GetVirtualInput();
            IDTSVirtualInputColumn100 vcol = vi.VirtualInputColumnCollection[name];
            SetInputUsageType(vi, vcol, usageType);
        }

        protected virtual void SetInputUsageType(IDTSVirtualInput100 vi, IDTSVirtualInputColumn100 vcol, DTSUsageType usageType)
        {
            this.SetInputUsageType(vi, vcol, usageType, false);
        }

        protected virtual void SetInputUsageType(IDTSVirtualInput100 vi, IDTSVirtualInputColumn100 vcol, DTSUsageType usageType, bool forceOverwrite)
        {
            if (vcol.UsageType != DTSUsageType.UT_READWRITE || forceOverwrite)
            {
                _instance.SetUsageType(Component.InputCollection[0].ID, vi, vcol.LineageID, usageType);
            }
        }

        protected virtual void SafeMapInputToExternalMetadataColumn(string inputColumnName, string externalMetadataColumnName, bool unMap)
        {
            try
            {
                IDTSVirtualInput100 cvi = _component.InputCollection[0].GetVirtualInput();
                IDTSExternalMetadataColumn100 eCol = _component.InputCollection[0].ExternalMetadataColumnCollection[externalMetadataColumnName];

                foreach (IDTSInputColumn100 inCol in _component.InputCollection[0].InputColumnCollection)
                {
                    //Unmap anything else that maps to this external metadata column)
                    if (inCol.ExternalMetadataColumnID == eCol.ID)
                    {
                        MessageEngine.Global.Trace(Severity.Debug, "{0}: {1} Unmapping Input {2}", GetType(), _component.Name, inCol.Name);
                        SetInputUsageType(cvi, cvi.VirtualInputColumnCollection[inCol.Name], DTSUsageType.UT_IGNORED, true);
                        break;
                    }
                }
                if (!unMap)
                {
                    SetInputUsageType(cvi, cvi.VirtualInputColumnCollection[inputColumnName], DTSUsageType.UT_READONLY);
                    _component.InputCollection[0].InputColumnCollection[inputColumnName].ExternalMetadataColumnID = eCol.ID;
                }
            }
            catch (System.Runtime.InteropServices.COMException ce)
            {
                MessageEngine.Global.Trace(Severity.Warning, "WarningMapColumnsDoNotExist {0} {1} {2} {3}", inputColumnName, externalMetadataColumnName, _component.Name, ce.Message);
            }
        }

        //TODO clean or optimze the below code
        protected string ExpressionCleanerAndInputMapBuilder(string expression, IDTSVirtualInput100 vi, DTSUsageType inputColumnUsageType)
        {
            foreach (IDTSVirtualInputColumn100 vcol in vi.VirtualInputColumnCollection)
            {
                Regex regex = new Regex(@"\[\b" + Regex.Escape(vcol.Name) + @"\b\]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                if (regex.IsMatch(expression))
                {
                    SetInputUsageType(vi, vcol, inputColumnUsageType);
                    expression = regex.Replace(expression, "#" + vcol.LineageID);
                }
                else
                {
                    regex = new Regex(@"\b" + Regex.Escape(vcol.Name) + @"\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    //Slow, speed this up with a proper lookup and parse
                    if (regex.IsMatch(expression))
                    {
                        SetInputUsageType(vi, vcol, inputColumnUsageType);
                        expression = regex.Replace(expression, "#" + vcol.LineageID);
                    }
                }
            }

            return expression;
        }
    }
}
