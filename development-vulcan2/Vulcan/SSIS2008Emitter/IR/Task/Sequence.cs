using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.Framework;

namespace Ssis2008Emitter.IR.Task
{
    public class SequenceTask : Task
    {
        #region Private Storage
        private List<Task> _tasks = new List<Task>();
        private List<Variable> _variableList = new List<Variable>();
        private bool _logEnabled = false;
        private string _logConnectionName;
        private string _constraintMode = "Linear";
        private string _transactionMode = "Supported";
        #endregion  // Private Storage

        #region Public Accessor Properties
        public IList<Task> Tasks
        {
            get { return _tasks; }
        }

        public IList<Variable> VariableList
        {
            get { return _variableList; }
        }

        public bool Log
        {
            get { return _logEnabled; }
            set { _logEnabled = value; }
        }

        public string LogConnectionName
        {
            get { return _logConnectionName; }
            set { _logConnectionName = value; }
        }

        public string ConstraintMode
        {
            get { return _constraintMode; }
            set { _constraintMode = value; }
        }

        public string TransactionMode
        {
            get { return _transactionMode; }
            set { _transactionMode = value; }
        }
        #endregion  // Public Accessor Properties
    }
}
