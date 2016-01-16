using System;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.Framework.Connections;

namespace Ssis2008Emitter.IR.Tasks.Transformations
{
    internal static class TransformationUtility
    {
        public static void RegisterOleDBConnection(SsisEmitterContext context, OleDBConnection oleDBConnection, IDTSComponentMetaData100 component)
        {
            oleDBConnection.Initialize(context);
            component.RuntimeConnectionCollection[0].ConnectionManager = DtsConvert.GetExtendedInterface(oleDBConnection.ConnectionManager);
            component.RuntimeConnectionCollection[0].ConnectionManagerID = oleDBConnection.ConnectionManager.ID;
        }

        public static IDTSVirtualInputColumn100 FindVirtualInputColumnByName(string name, IDTSInput100 input, bool ignoreCase)
        {
            if (input != null)
            {
                foreach (IDTSVirtualInputColumn100 vic in input.GetVirtualInput().VirtualInputColumnCollection)
                {
                    StringComparison comparisonType = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
                    if (vic.Name.Equals(name, comparisonType))
                    {
                        return vic;
                    }
                }
            }

            return null;
        }

        public static IDTSInputColumn100 FindInputColumnByName(string name, IDTSInput100 input, bool ignoreCase)
        {
            if (input != null)
            {
                foreach (IDTSInputColumn100 ic in input.InputColumnCollection)
                {
                    StringComparison comparisonType = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
                    if (ic.Name.Equals(name, comparisonType))
                    {
                        return ic;
                    }
                }
            }

            return null;
        }

        public static IDTSOutputColumn100 FindOutputColumnByName(string name, IDTSOutput100 output, bool ignoreCase)
        {
            if (output != null)
            {
                foreach (IDTSOutputColumn100 oc in output.OutputColumnCollection)
                {
                    StringComparison comparisonType = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;

                    if (oc.Name.Equals(name, comparisonType))
                    {
                        return oc;
                    }
                }
            }

            return null;
        }

        public static IDTSExternalMetadataColumn100 FindExternalColumnByName(string name, IDTSExternalMetadataColumnCollection100 externalColumnCollection, bool ignoreCase)
        {
            if (externalColumnCollection != null)
            {
                foreach (IDTSExternalMetadataColumn100 ec in externalColumnCollection)
                {
                    StringComparison comparisonType = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
                    if (ec.Name.Equals(name, comparisonType))
                    {
                        return ec;
                    }
                }
            }

            return null;
        }

        public static IDTSCustomProperty100 FindCustomPropertyByName(string name, IDTSCustomPropertyCollection100 customPropertyCollection, bool ignoreCase)
        {
            if (customPropertyCollection != null)
            {
                foreach (IDTSCustomProperty100 cp in customPropertyCollection)
                {
                    StringComparison comparisonType = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
                    if (cp.Name.Equals(name, comparisonType))
                    {
                        return cp;
                    }
                }
            }

            return null;
        }
    }
}
