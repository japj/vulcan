using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ssis2008Emitter.IR.TSQL;
using Ssis2008Emitter.IR.Task;

namespace Ssis2008Emitter.Emitters.TSQL.PlatformEmitter
{
    public class LogPlatformEmitter : PlatformEmitter
    {
        public string LogStart(SequenceTask logContainer, string packageName, string taskName, string varScopeXML)
        {
            TemplatePlatformEmitter te = new TemplatePlatformEmitter("LogStart", packageName, taskName, varScopeXML);
            return te.Emit(logContainer);
        }

        public string LogGetPredefinedValues(SequenceTask logContainer)
        {
            TemplatePlatformEmitter te = new TemplatePlatformEmitter("LogGetPredefinedValues");
            return te.Emit(logContainer);
        }

        public string LogGetValue(SequenceTask logContainer, string logIDName, string pathVariable, string nameVariable)
        {
            TemplatePlatformEmitter te = new TemplatePlatformEmitter("LogGetValue", logIDName, pathVariable, nameVariable);
            return te.Emit(logContainer);
        }

        public string LogPrepareToSetValue(SequenceTask logContainer)
        {
            TemplatePlatformEmitter te = new TemplatePlatformEmitter("LogPrepareToSetValue");
            return te.Emit(logContainer);
        }

        public string LogSetValue(SequenceTask logContainer, string logIDName, string pathVariable, string nameVariable, string value)
        {
            TemplatePlatformEmitter te = new TemplatePlatformEmitter("LogSetValue", logIDName, pathVariable, nameVariable, value);
            return te.Emit(logContainer);
        }

        public string LogEnd(SequenceTask logContainer, string logIDName)
        {
            TemplatePlatformEmitter te = new TemplatePlatformEmitter("LogEnd", logIDName);
            return te.Emit(logContainer);
        }
    }
}
