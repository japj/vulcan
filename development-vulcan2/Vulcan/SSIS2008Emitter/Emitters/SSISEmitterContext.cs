using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using DTS = Microsoft.SqlServer.Dts.Runtime;

using VulcanEngine.Common;
using Ssis2008Emitter.Properties;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.Emitters.Framework;
using Ssis2008Emitter.Emitters.Task;

namespace Ssis2008Emitter.Emitters
{
    public class SSISEmitterContext
    {
        private Guid _guid;
        private MessageEngine _message;
        private SsisPackage _package;
        private DTS.IDTSSequence _parentContainer;
        private SsisSequence _ssisSequence;
        private PluginLoader<ISSISEmitter, PhysicalIRMappingAttribute> _pluginLoader;
        private SsisPipelineTask _ssisDataFlowTask;

        public SsisPipelineTask SSISDataFlowTask
        {
            get { return _ssisDataFlowTask; }
        }

        public bool HasSSISDataFlowTask
        {
            get { return _ssisDataFlowTask != null; }
        }

        public PluginLoader<ISSISEmitter, PhysicalIRMappingAttribute> PluginLoader
        {
            get { return _pluginLoader; }
        }

        public SsisPackage Package
        {
            get { return _package; }
        }

        public DTS.IDTSSequence ParentContainer
        {
            get { return _parentContainer; }
        }

        internal SsisSequence SSISSequence
        {
            get { return _ssisSequence; }
        }

        internal SSISEmitterContext(SsisPackage package, SsisSequence parentSequence, PluginLoader<ISSISEmitter, PhysicalIRMappingAttribute> pluginLoader)
        {
            _package = package;
            _ssisSequence = parentSequence;
            _guid = Guid.NewGuid();
            _message = MessageEngine.Create(String.Format(System.Globalization.CultureInfo.InvariantCulture, "SSISFactory: {0}", _guid.ToString()));
            if (_ssisSequence != null)
            {
                _parentContainer = _ssisSequence.DTSSequence;
            }
            _pluginLoader = pluginLoader;
        }

        internal SSISEmitterContext(SsisPackage package, SsisSequence parentSequence, PluginLoader<ISSISEmitter, PhysicalIRMappingAttribute> pluginLoader, SsisPipelineTask ssisDataFlowTask)
            : this(package, parentSequence, pluginLoader)
        {
            this._ssisDataFlowTask = ssisDataFlowTask;
        }

        internal SSISEmitterContext NewParentSequence(SsisSequence parentSequence)
        {
            return new SSISEmitterContext(this._package, parentSequence, this._pluginLoader);
        }

        internal SSISEmitterContext AddDataFlow(SsisPipelineTask dataFlowTask)
        {
            return new SSISEmitterContext(this._package, this._ssisSequence, this._pluginLoader, dataFlowTask);
        }

        public ISSISEmitter InstantiateEmitter(LogicalObject obj, SSISEmitterContext context)
        {
            Type ssisEmitterType = null;

            if (_pluginLoader.PluginTypesByAttribute.TryGetValue(new PhysicalIRMappingAttribute(obj.GetType()), out ssisEmitterType))
            {
                ConstructorInfo constructor = ssisEmitterType.GetConstructor(new Type[] { obj.GetType(), context.GetType() });
                ISSISEmitter objEmitter = (ISSISEmitter)constructor.Invoke(new object[] { obj, context });

                return objEmitter;
            }
            else
            {
                _message.Trace(Severity.Error, Resources.SSISObjectEmitterNotFound, obj.GetType());

                return null;
            }
        }
    }
}
