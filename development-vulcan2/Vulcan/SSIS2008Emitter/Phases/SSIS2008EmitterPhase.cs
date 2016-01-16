using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using DTS = Microsoft.SqlServer.Dts.Runtime;

using VulcanEngine.Common;
using VulcanEngine.Phases;
using Ssis2008Emitter.Properties;
using Ssis2008Emitter.Utility;
using Ssis2008Emitter.IR;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.Framework;
using Ssis2008Emitter.Emitters;
using Ssis2008Emitter.Emitters.Framework;
using Ssis2008Emitter.Emitters.Common;
using Ssis2008Emitter.Emitters.Task;


namespace Ssis2008Emitter
{
    [PhaseFriendlyNameAttribute("SSIS2008EmitterPhase")]
    public class SSIS2008EmitterPhase : IPhase
    {
        private Guid _guid;
        private MessageEngine _message;
        private string _workflowUniqueName;
        private PluginLoader<ISSISEmitter, PhysicalIRMappingAttribute> _pluginLoader;

        public SSIS2008EmitterPhase(string WorkflowUniqueName)
        {
            this._workflowUniqueName = WorkflowUniqueName;
            _guid = Guid.NewGuid();
            _message = MessageEngine.Create(String.Format(System.Globalization.CultureInfo.InvariantCulture, "SSISFactory: {0}", _guid.ToString()));
            _pluginLoader = new PluginLoader<ISSISEmitter, PhysicalIRMappingAttribute>(null, 1, 1);
        }

        #region IPhase Members

        public string Name
        {
            get { return "SSIS2008EmitterPhase"; }
        }

        public string WorkflowUniqueName
        {
            get { return this._workflowUniqueName; }
        }

        public Type InputIRType
        {
            get { return typeof(PhysicalIR); }
        }

        public VulcanEngine.IR.IIR Execute(List<VulcanEngine.IR.IIR> PredecessorIRs)
        {
            return this.Execute(IRUtility.MergeIRList(this.Name, PredecessorIRs));
        }

        public VulcanEngine.IR.IIR Execute(VulcanEngine.IR.IIR PredecessorIR)
        {
            PhysicalIR physicalIR = PredecessorIR as PhysicalIR;
            if (physicalIR == null)
            {
                // TODO: Message.Trace(Severity.Error, Resources.ErrorPhaseWorkflowIncorrectInputIRType, PredecessorIR.GetType().ToString(), this.Name);
            }

            foreach (LogicalObject physicalNode in physicalIR.PhysicalNodes)
            {
                if (physicalNode is ConnectionConfiguration)
                {
                    SsisConnectionConfiguration configEmitter = new SsisConnectionConfiguration((ConnectionConfiguration)physicalNode, new SSISEmitterContext(null, null, _pluginLoader));
                    configEmitter.Emit();
                }
            }

            int PackagesCount = physicalIR.PhysicalNodes.Count(delegate(LogicalObject o) { return o is Package; });
            int PackagesProcessed = 0;

            foreach (LogicalObject physicalNode in physicalIR.PhysicalNodes)
            {
                if (physicalNode is Package)
                {
                    SsisPackage emitterPackage = new SsisPackage((Package)physicalNode, new SSISEmitterContext(null, null, _pluginLoader));
                    MessageEngine.Global.UpdateProgress(PackagesProcessed / (double)PackagesCount);
                    emitterPackage.Emit();
                    MessageEngine.Global.UpdateProgress(++PackagesProcessed / (double)PackagesCount);
                }
            }
            return null;
        }

        #endregion

        public SSISEmitterContext Emit(LogicalObject obj, SSISEmitterContext context)
        {
            Type ssisEmitterType;

            if (_pluginLoader.PluginTypesByAttribute.TryGetValue(new PhysicalIRMappingAttribute(obj.GetType()), out ssisEmitterType))
            {
                ConstructorInfo constructor = ssisEmitterType.GetConstructor(new Type[] {obj.GetType(), context.GetType()});
                ISSISEmitter objEmitter = (ISSISEmitter)constructor.Invoke(new object[] { obj, context });
                
                return objEmitter.Emit();
            }
            else
            {
                _message.Trace(Severity.Warning, Resources.SSISObjectEmitterNotFound, obj.GetType());

                return context;
            }
        }
    }
}