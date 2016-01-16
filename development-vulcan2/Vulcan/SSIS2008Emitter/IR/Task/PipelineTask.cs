using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.Framework;
using Ssis2008Emitter.IR.DataFlow;

namespace Ssis2008Emitter.IR.Task
{
    public class PipelineTask : Task
    {
        #region Private Storage
        private bool _delayValidation;
        private IsolationLevel _isolationLevel;
        private List<Transformation> _transformations = new List<Transformation>();
        #endregion  // Private Storage

        #region Public Accessor Properties
        public bool DelayValidation
        {
            get { return _delayValidation; }
            set { _delayValidation = value; }
        }

        public IsolationLevel IsolationLevel
        {
            get { return _isolationLevel; }
            set { _isolationLevel = value; }
        }

        public IList<Transformation> Transformations
        {
            get { return _transformations; }
        }
        #endregion  // Public Accessor Properties
    }
}
