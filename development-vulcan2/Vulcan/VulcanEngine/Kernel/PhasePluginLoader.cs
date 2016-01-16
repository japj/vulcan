using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

using VulcanEngine.Phases;
using VulcanEngine.Common;
using VulcanEngine.Properties;

namespace VulcanEngine.Kernel
{
    // REVIEW: any need to make this generic on type and instantiate the generic version with T=IPhase?  I'm not seeing where else this functionality is required right now.
    //REVIEW: vsabella - I think we should get rid of FriendlyNames - it has too much leeway for abuse.  
    //Otherwise, you can use a single dictionary to track this and automatically flag name collisions

    public class PhasePluginLoader 
    {
        private PluginLoader<IPhase, PhaseFriendlyNameAttribute> _PluginLoader;
        MessageEngine _Message;

        public PhasePluginLoader()
        {
            this._PluginLoader = new PluginLoader<IPhase, PhaseFriendlyNameAttribute>(Settings.Default.SubpathPhasePluginFolder, 0, 1);
            this._Message = MessageEngine.Create("__PHASE PLUGIN LOADER");
        }


        private Type RetrievePhaseType(string PhaseName)
        {
            Type phaseType = null;
            if (this._PluginLoader.PluginTypesByAttributeString.ContainsKey(PhaseName))
            {
                phaseType = this._PluginLoader.PluginTypesByAttributeString[PhaseName];
            }
            else if (this._PluginLoader.PluginTypesByFullName.ContainsKey(PhaseName))
            {
                phaseType = this._PluginLoader.PluginTypesByFullName[PhaseName];
            }
            else
            {
                // REVIEW: I'm effectively using Message.Trace for flow control here and in a few other places.  Is that permitted by the contract?  Can I assume that Severity.Error equates to a non-Debug ASSERT?
                _Message.Trace(Severity.Error, Resources.ErrorSpecifiedPhaseNameNotFound, PhaseName);
            }
            return phaseType;
        }

        public IPhase RetrievePhase(string PhaseName, IDictionary<string, object> phaseParameters)
        {
            Type phaseType = RetrievePhaseType(PhaseName);
            ConstructorInfo[] constructors = phaseType.GetConstructors();

            foreach (ConstructorInfo cctor in constructors)
            {
                bool cctorParamsSubsetOfRequired = true;
                ParameterInfo[] cctorParams = cctor.GetParameters();

                foreach (ParameterInfo cctorParam in cctorParams)
                {
                    if(!(phaseParameters.ContainsKey(cctorParam.Name) && phaseParameters[cctorParam.Name].GetType().Equals(cctorParam.ParameterType)))
                    {
                        cctorParamsSubsetOfRequired = false;
                        break;
                    }
                }

                if (cctorParamsSubsetOfRequired && phaseParameters.Count == cctorParams.Length)
                {
                    object[] invokeParameters = new object[cctorParams.LongLength];
                    for(int i=0; i < cctorParams.LongLength; i++)
                    {
                        string parameterName = cctorParams[i].Name;
                        invokeParameters[i] = phaseParameters[parameterName];
                    }
                    return (IPhase)cctor.Invoke(invokeParameters);
                }
            }
            _Message.Trace(Severity.Error, Resources.ErrorPhaseLacksSpecifiedConstructor, PhaseName);
            return null;
        }

    }
}
