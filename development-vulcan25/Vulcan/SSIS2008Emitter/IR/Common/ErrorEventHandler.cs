using AstFramework;
using VulcanEngine.Common;
using DTS = Microsoft.SqlServer.Dts.Runtime;

namespace Ssis2008Emitter.IR.Common
{
    public class ErrorEvents : DTS.DefaultEvents
    {
        private int _validationErrorCount;

        public override bool OnError(
            DTS.DtsObject source, 
            int errorCode, 
            string subComponent,
            string description, 
            string helpFile, 
            int helpContext, 
            string idofInterfaceWithError)
        {
            // Add application-specific diagnostics here.
            MessageEngine.Trace(Severity.Debug, "Validation Error in {0}/{1} : {2} : {3}", source, subComponent, description, helpFile);
            _validationErrorCount++;
            return false;
        }

        public int ValidationErrorCount
        {
            get
            {
                return _validationErrorCount;
            }
        }
    }
}
