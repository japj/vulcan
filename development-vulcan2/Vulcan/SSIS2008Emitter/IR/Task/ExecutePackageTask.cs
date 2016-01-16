using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.IR.Task
{
    public class ExecutePackageTask : Task
    {
        #region Private Storage
        private string _relativePath;
        #endregion  // Private Storage

        #region Public Accessor Properties
        public string RelativePath
        {
            get { return _relativePath; }
            set { _relativePath = value; }
        }
        #endregion  // Public Accessor Properties
    }
}