using System;
using System.Collections.Generic;
using System.Text;

using DTS = Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

using VulcanEngine.Common;
using Ssis2008Emitter.Properties;
using Ssis2008Emitter.Utility;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.DataFlow;
using Ssis2008Emitter.IR.Task;
using Ssis2008Emitter.Emitters.Common;
using Ssis2008Emitter.Emitters.DataFlow;

namespace Ssis2008Emitter.Emitters.Task
{
    //SSISETLEmitter
    [PhysicalIRMapping(typeof(PipelineTask))]
    public class SsisPipelineTask : SsisTaskEmitter, ISSISEmitter
    {
        private Guid _guid;
        private MessageEngine _message;
        private List<SsisComponent> _componentList;
        private IDTSComponentMetaData100 _lastComponent;
        private DTS.TaskHost _dataFlowTask;
        private PipelineTask _logicalETL;

        public SsisPipelineTask(PipelineTask objETL, SSISEmitterContext context)
            : base(objETL, context)
        {
            _logicalETL = objETL;

            _guid = Guid.NewGuid();
            // TODO: Do this for everything
            _message = MessageEngine.Create(String.Format(System.Globalization.CultureInfo.InvariantCulture, "__SSIS2008Emitter:SSISDataFlow {0}", _guid.ToString()));
            _componentList = new List<SsisComponent>();
        }


        public SSISEmitterContext Emit()
        {
            MessageEngine.Global.Trace(Severity.Notification, Resources.EmittingETL, _logicalETL.Name);

            _dataFlowTask = (DTS.TaskHost)Context.SSISSequence.AppendExecutable("STOCK:PipelineTask");
            _dataFlowTask.Properties["DelayValidation"].SetValue(_dataFlowTask, _logicalETL.DelayValidation);
            _dataFlowTask.Properties["IsolationLevel"].SetValue(_dataFlowTask, _logicalETL.IsolationLevel);

            _dataFlowTask.Name = _logicalETL.Name;
            
            this.NewDataFlow();

            SSISEmitterContext dataFlowContext = _context.AddDataFlow(this);

            foreach (Transformation t in _logicalETL.Transformations)
            {
                dataFlowContext.InstantiateEmitter(t, dataFlowContext).Emit();
            }
            return _context;
        }

        public override Microsoft.SqlServer.Dts.Runtime.IDTSPropertiesProvider PropertyProvider
        {
            get { return _dataFlowTask; }
        }

        public MainPipe MainPipe
        {
            get { return (MainPipe)_dataFlowTask.InnerObject; }
        }

        public IDTSComponentMetaData100 NewComponentMetaData()
        {
            return MainPipe.ComponentMetaDataCollection.New();
        }

        public void NewDataFlow()
        {
            _lastComponent = null;
        }

        public void ChainComponent(SsisComponent component)
        {
            if (_lastComponent != null)
            {
                MainPipe.PathCollection.New().AttachPathAndPropagateNotifications(_lastComponent.OutputCollection[0], component.Component.InputCollection[0]);
                component.Flush();
            }
            AddComponent(component);
        }

        public void ChainComponent(SsisComponent component, InputPath inputPath)
        {
            if (inputPath == null || (string.IsNullOrEmpty(inputPath.Name) && string.IsNullOrEmpty(inputPath.Source)))
            {
                ChainComponent(component);
            }
            else
            {
                IDTSOutput100 dtsOutput = FindOutput(inputPath);
                MainPipe.PathCollection.New().AttachPathAndPropagateNotifications(dtsOutput, component.Component.InputCollection[0]);
                component.Flush();
                AddComponent(component);
            }
        }

        public void ChainComponent(SsisComponent component, InputPath inputPath, IDTSInput100 input)
        {
            if (input == null)
            {
                ChainComponent(component, inputPath);
            }
            else
            {
                IDTSOutput100 dtsOutput = FindOutput(inputPath);
                MainPipe.PathCollection.New().AttachPathAndPropagateNotifications(dtsOutput, input);
                component.Flush();
                AddComponent(component);
            }
        }

        public void AddComponent(SsisComponent component)
        {
            if (_lastComponent != component.Component)
            {
                _lastComponent = component.Component;
                _componentList.Add(component);
            }
        }

        private IDTSOutput100 FindOutput(InputPath inputPath)
        {
            if (!string.IsNullOrEmpty(inputPath.Source))
            {
                foreach (SsisComponent component in _componentList)
                {
                    if (string.Compare(inputPath.Source, component.Component.Name, true, System.Globalization.CultureInfo.InvariantCulture) == 0)
                    {
                        foreach (IDTSOutput100 dtsOutput in component.Component.OutputCollection)
                        {
                            if (string.Compare(inputPath.Name, dtsOutput.Name, true, System.Globalization.CultureInfo.InvariantCulture) == 0)
                            {
                                return dtsOutput;
                            }
                        }
                    }
                }
                this._message.Trace(Severity.Error, Resources.InputPathError1, inputPath.Source, inputPath.Name);
            }
            else
            {
                foreach (SsisComponent component in _componentList)
                {
                    foreach (IDTSOutput100 dtsOutput in component.Component.OutputCollection)
                    {
                        if (string.Compare(inputPath.Name, dtsOutput.Name, true, System.Globalization.CultureInfo.InvariantCulture) == 0)
                        {
                            return dtsOutput;
                        }
                    }
                }
                this._message.Trace(Severity.Error, Resources.InputPathError2, inputPath.Name);
            }
            return null;
        }
    }
}
