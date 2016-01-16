using System;
using System.Collections.Generic;
using System.Reflection;
using AstFramework;
using VulcanEngine.Common;
using VulcanEngine.Phases;
using VulcanEngine.Properties;

namespace VulcanEngine.Kernel
{
    // REVIEW: any need to make this generic on type and instantiate the generic version with T=IPhase?  I'm not seeing where else this functionality is required right now.
    // REVIEW: vsabella - I think we should get rid of FriendlyNames - it has too much leeway for abuse.  
    // Otherwise, you can use a single dictionary to track this and automatically flag name collisions
    public class PhasePluginLoader 
    {
        private readonly PluginLoader<IPhase, PhaseFriendlyNameAttribute> _pluginLoader;

        public PhasePluginLoader()
        {
            _pluginLoader = new PluginLoader<IPhase, PhaseFriendlyNameAttribute>(Settings.Default.SubpathPhasePluginFolder, 0, 1);
            _pluginLoader.LoadPlugins();
        }

        private Type RetrievePhaseType(string phaseName)
        {
            Type phaseType = null;
            if (_pluginLoader.PluginTypesByAttributeString.ContainsKey(phaseName))
            {
                phaseType = _pluginLoader.PluginTypesByAttributeString[phaseName];
            }
            else if (_pluginLoader.PluginTypesByFullName.ContainsKey(phaseName))
            {
                phaseType = _pluginLoader.PluginTypesByFullName[phaseName];
            }
            else
            {
                // REVIEW: I'm effectively using Message.Trace for flow control here and in a few other places.  Is that permitted by the contract?  Can I assume that Severity.Error equates to a non-Debug ASSERT?
                MessageEngine.Trace(Severity.Error, Resources.ErrorSpecifiedPhaseNameNotFound, phaseName);
            }

            return phaseType;
        }

        public IPhase RetrievePhase(string phaseName, IDictionary<string, object> phaseParameters)
        {
            Type phaseType = RetrievePhaseType(phaseName);
            ConstructorInfo[] constructors = phaseType.GetConstructors();

            foreach (ConstructorInfo cctor in constructors)
            {
                bool cctorParamsSubsetOfRequired = true;
                ParameterInfo[] cctorParams = cctor.GetParameters();

                foreach (ParameterInfo cctorParam in cctorParams)
                {
                    if (!(phaseParameters.ContainsKey(cctorParam.Name) && phaseParameters[cctorParam.Name].GetType().Equals(cctorParam.ParameterType)))
                    {
                        cctorParamsSubsetOfRequired = false;
                        break;
                    }
                }

                if (cctorParamsSubsetOfRequired && phaseParameters.Count == cctorParams.Length)
                {
                    object[] invokeParameters = new object[cctorParams.LongLength];
                    for (int i = 0; i < cctorParams.LongLength; i++)
                    {
                        string parameterName = cctorParams[i].Name;
                        invokeParameters[i] = phaseParameters[parameterName];
                    }

                    return (IPhase)cctor.Invoke(invokeParameters);
                }
            }

            MessageEngine.Trace(Severity.Error, Resources.ErrorPhaseLacksSpecifiedConstructor, phaseName);
            return null;
        }
    }
}
