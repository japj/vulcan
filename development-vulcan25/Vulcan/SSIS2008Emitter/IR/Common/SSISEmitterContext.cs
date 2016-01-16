using Ssis2008Emitter.IR.Framework;
using Ssis2008Emitter.IR.Tasks;

namespace Ssis2008Emitter.IR.Common
{
    public class SsisEmitterContext
    {
        public Package Package { get; private set; }

        public Container ParentContainer { get; private set; }

        public ProjectManager ProjectManager { get; private set; }

        internal SsisEmitterContext()
            : this(null, null, new ProjectManager())
        {
        }

        internal SsisEmitterContext(Package package, Container parent, ProjectManager projectManager)
        {
            Package = package;
            ParentContainer = parent;
            ProjectManager = projectManager;
        }
    }
}
