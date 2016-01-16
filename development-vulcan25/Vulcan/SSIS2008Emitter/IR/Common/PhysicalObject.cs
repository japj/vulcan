using System;
using Ssis2008Emitter.Properties;
using Vulcan.Utility.Tree;

namespace Ssis2008Emitter.IR.Common
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public abstract class PhysicalObject : TreeNode<PhysicalObject>, ISsisEmitter
    {
        private string _name;
        
        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        protected PhysicalObject(string name) : base()
        {
            _name = name;
        }

        public abstract void Initialize(SsisEmitterContext context);
        
        public abstract void Emit(SsisEmitterContext context);

        public override string ToString()
        {
            if (!String.IsNullOrEmpty(Name))
            {
                return GetType() + Resources.LogicalNameSeperator + Name;
            }

            return GetType().ToString();
        }
    }
}
