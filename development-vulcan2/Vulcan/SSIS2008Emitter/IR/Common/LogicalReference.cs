using System;
using System.Collections.Generic;
using System.Text;

namespace Ssis2008Emitter.IR.Common
{
    public abstract class LogicalReference : LogicalObject
    {
        #region Private Storage
        private LogicalObject _refObject;
        private bool _lateBinding = false;
        #endregion  // Private Storage

        #region Public Accessor Properties
        public LogicalObject RefObject
        {
            get { return _refObject; }
            set { _refObject = value; }
        }

        public bool LateBinding
        {
            get { return _lateBinding; }
            set { _lateBinding = value; }
        }
        #endregion  // Public Accessor Properties
    }
}
