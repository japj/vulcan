using System;
using System.Collections.Generic;
using Ssis2008Emitter.IR.Common;
using VulcanEngine.IR;

namespace Ssis2008Emitter.IR
{
    public class PhysicalIR : AstIR
    {
        #region Private Storage
        private readonly List<PhysicalObject> _emittableNodes;
        #endregion  // Private Storage

        public PhysicalIR(AstIR astIR) : base(astIR)
        {
            _emittableNodes = new List<PhysicalObject>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        public PhysicalObject InitializePackage(string packageName)
        {
            var po = new PhysicalRootObject(packageName);
            _emittableNodes.Add(po);
            return po;
        }

        #region Public Accessors

        public IEnumerable<PhysicalObject> EmittableNodes
        {
            get { return _emittableNodes; }
        }
        #endregion  // Public Accessors

        #region IIR Members
        public override string Name
        {
            get { return "PhysicalIR"; }
        }
        #endregion

        #region ICloneable Members
        public override object Clone()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
