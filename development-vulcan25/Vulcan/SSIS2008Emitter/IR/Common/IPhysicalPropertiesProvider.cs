using DTS = Microsoft.SqlServer.Dts.Runtime;

namespace Ssis2008Emitter.IR.Common
{
    internal interface IPhysicalPropertiesProvider
    {
        void SetProperty(string name, object value);
        
        void SetExpression(string name, string expression);

        DTS.IDTSPropertiesProvider PropertyProvider { get; }
    }
}
