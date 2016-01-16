using System;

namespace Ssis2008Emitter.Phases.Lowering.Framework
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class LoweringAttribute : Attribute
    {
        private Type _astNodeType;

        public LoweringAttribute(Type astNodeType)
        {
            _astNodeType = astNodeType;
        }

        public Type AstNodeType
        {
            get { return _astNodeType; }
        }
    }
}
