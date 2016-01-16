using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.IR.Ast;

namespace Ssis2008Emitter.IR
{
    public class PhysicalIR : AstIR
    {
        #region Private Storage
        private List<Common.LogicalObject> _physicalRootNodes;
        #endregion  // Private Storage

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

        #region Public Accessors
        public List<Common.LogicalObject> PhysicalNodes
        {
            get { return this._physicalRootNodes; }
            set { this._physicalRootNodes = value; }
        }
        #endregion  // Public Accessors

        #region Initialization
        public PhysicalIR(AstIR astIR) : base(astIR) 
        {
            this._physicalRootNodes = new List<Ssis2008Emitter.IR.Common.LogicalObject>();
        }
        #endregion  / Initialization
    }
}
