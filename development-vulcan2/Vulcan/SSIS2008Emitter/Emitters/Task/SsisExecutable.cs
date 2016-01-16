using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using DTS = Microsoft.SqlServer.Dts.Runtime;

using VulcanEngine.Common;
using Ssis2008Emitter.Properties;
using Ssis2008Emitter.Emitters.Common;

namespace Ssis2008Emitter.Emitters.Task
{
    public class SsisExecutable
    {
        private DTS.Executable _executable;

        public SsisExecutable(DTS.Executable executable)
        {
            _executable = executable;
        }

        public DTS.Executable Executable
        {
            get { return _executable; }
            set { _executable = value; }
        }

        public string Name
        {
            get
            {
                PropertyInfo namePropertyInfo = _executable.GetType().GetProperty("Name");
                if (namePropertyInfo != null)
                {
                    return (string)namePropertyInfo.GetValue(_executable, null);
                }
                else
                {
                    return "";
                }
            }
        }

        public void Execute()
        {
            if (_executable != null)
            {
                MessageEngine.Global.Trace(Severity.Alert, "Executing DTS Package {0}", Name);

                ErrorEvents errorHandler = new ErrorEvents();
                DTS.DTSExecResult execResult = _executable.Execute(null, null, errorHandler, null, null);
                if (execResult == Microsoft.SqlServer.Dts.Runtime.DTSExecResult.Success && errorHandler.ValidationErrorCount == 0)
                {
                    MessageEngine.Global.Trace(Severity.Notification, "Success executing Task {0}", Name);
                }
                else
                {
                    MessageEngine.Global.Trace(
                        Severity.Error,
                        "Error executing Task {0}: Result = {1} (BugBug: SSIS always returns success) but ErrorCount = {2}",
                        Name,
                        execResult,
                        errorHandler.ValidationErrorCount
                        );
                }
            }
        }
    }
}
