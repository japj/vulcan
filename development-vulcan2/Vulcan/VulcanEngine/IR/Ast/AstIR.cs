using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;
using VulcanEngine.Properties;

namespace VulcanEngine.IR.Ast
{
    public class AstIR : XmlIR
    {
        #region Private Storage
        private AstRootNode _astRootNode;
        #endregion  // Private Storage

        #region IIR Members
        public override string Name
        {
            get { return "ASTIR"; }
        }
        #endregion

        #region ICloneable Members
        public override object Clone()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Public Accessors
        public Ast.AstRootNode AstRootNode
        {
            get { return this._astRootNode; }
            set { this._astRootNode = value; }
        }
        #endregion  // Public Accessors

        #region Initialization
        public AstIR(XmlIR xmlIR) : base(xmlIR) { }
        
        public AstIR(AstIR astIR) : base(astIR)
        {
            this._astRootNode = astIR._astRootNode;
        }
        #endregion  / Initialization
    }
}
