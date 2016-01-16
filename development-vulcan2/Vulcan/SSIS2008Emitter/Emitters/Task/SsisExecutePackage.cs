using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using DTS = Microsoft.SqlServer.Dts.Runtime;
using DTSTasks = Microsoft.SqlServer.Dts.Tasks;

using Ssis2008Emitter.Utility;
using Ssis2008Emitter.IR.Framework;
using Ssis2008Emitter.IR.Task;
using Ssis2008Emitter.Emitters.Common;
using Ssis2008Emitter.Emitters.Framework;

namespace Ssis2008Emitter.Emitters.Task
{
    [PhysicalIRMapping(typeof(ExecutePackageTask))]
    public class SsisExecutePackage : SsisTaskEmitter, ISSISEmitter
    {
        private DTS.TaskHost _execPackageTaskHost;

        public SsisExecutePackage(ExecutePackageTask execPackage, SSISEmitterContext context)
            : base(execPackage, context)
        {
            _execPackageTaskHost = (DTS.TaskHost)Context.SSISSequence.AppendExecutable("STOCK:ExecutePackageTask");
            Initialize(execPackage);
        }

        private void Initialize(ExecutePackageTask execPackage)
        {
            string expression;

            if (Path.IsPathRooted(execPackage.RelativePath))
            {
                expression = String.Format("\"{0}\"", execPackage.RelativePath);
            }
            else
            {
                expression = SSISExpressionPathBuilder.BuildExpressionPath
                    (
                    Context.Package.PackageRelativeDirectory 
                    + Path.DirectorySeparatorChar 
                    + execPackage.RelativePath
                    );
            }

            string name = execPackage.RelativePath.Replace(Path.DirectorySeparatorChar, '_');
            name = name.Replace('.', '_');
            _execPackageTaskHost.Name = name + Guid.NewGuid().ToString();

            ConnectionConfiguration config = new ConnectionConfiguration();
            config.Name = name;
            config.Type = "FILE";
            config.ConnectionString = expression;

            SsisConnection connection = new SsisConnection(config);
            
            SSISTask.Connection = connection.ConnectionManager.Name;
        }

        public SSISEmitterContext Emit()
        {
            // TODO: Should this do something?
            return _context;
        }

        public override Microsoft.SqlServer.Dts.Runtime.IDTSPropertiesProvider PropertyProvider
        {
            get { return _execPackageTaskHost; }
        }

        public override SsisExecutable SSISExecutable
        {
            get { return new SsisExecutable((DTS.Executable)_execPackageTaskHost); }
        }

        public DTSTasks.ExecutePackageTask.ExecutePackageTask SSISTask
        {
            get { return (DTSTasks.ExecutePackageTask.ExecutePackageTask)_execPackageTaskHost.InnerObject; }
        }
    }
}
