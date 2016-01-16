using System.Collections.ObjectModel;
using AstFramework;
using VulcanEngine.IR;
using VulcanEngine.Properties;

namespace VulcanEngine.Common
{
    public static class IRUtility
    {
        public static IIR MergeIRList(string callerPhaseName, Collection<IIR> list)
        {
            if (list.Count == 0)
            {
                MessageEngine.Trace(Severity.Error, Resources.ErrorPhaseInputIRListEmpty, callerPhaseName);
                return null;
            }

            if (list.Count > 1)
            {
                MessageEngine.Trace(Severity.Error, Resources.ErrorIRMergingNotYetSupported, callerPhaseName);
                return null;
            }

            return list[0];
        }

        public static readonly string NamespaceSeparator = Resources.NamespaceSeparator;
    }
}
