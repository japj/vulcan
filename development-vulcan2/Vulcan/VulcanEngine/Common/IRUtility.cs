using System;
using System.Collections.Generic;
using System.Text;

using VulcanEngine.IR;
using VulcanEngine.Common;
using VulcanEngine.Properties;

namespace VulcanEngine.Common
{
    public static class IRUtility
    {
        public static IIR MergeIRList(string CallerPhaseName, List<IIR> IRs)
        {
            if (IRs.Count == 0)
            {
                MessageEngine.Global.Trace(Severity.Error, Resources.ErrorPhaseInputIRListEmpty, CallerPhaseName);
                return null;
            }
            else if (IRs.Count > 1)
            {
                MessageEngine.Global.Trace(Severity.Error, Resources.ErrorIRMergingNotYetSupported, CallerPhaseName);
                return null;
            }
            else
            {
                return IRs[0];
            }
        }

        public static readonly string NamespaceSeparator = Resources.NamespaceSeparator;
            
    }
}
