using System;
using System.Collections.Generic;
using System.Text;

using DTS = Microsoft.SqlServer.Dts.Runtime;

using VulcanEngine.Common;

namespace Ssis2008Emitter.Emitters.Common
{
    public class ErrorEvents : DTS.DefaultEvents
    {
        private int _validationErrorCount = 0;

        public override bool OnError(
            DTS.DtsObject source, 
            int errorCode, 
            string subComponent,
            string description, 
            string helpFile, 
            int helpContext, 
            string idofInterfaceWithError
            )
        {
            // Add application-specific diagnostics here.
            MessageEngine.Global.Trace(Severity.Warning,"Validation Error in {0}/{1} : {2} : {3}", source, subComponent, description,helpFile);
            _validationErrorCount++;
            return false;
        }

        public int ValidationErrorCount
        {
            get
            {
                return this._validationErrorCount;
            }
        }
    }
}
