using System;
using System.Collections.Generic;
using System.Text;

using DTS = Microsoft.SqlServer.Dts.Runtime;

using VulcanEngine.Common;
using Ssis2008Emitter.IR.Task;

namespace Ssis2008Emitter.Emitters.Task
{
    internal class SsisSequence
    {
        private DTS.IDTSSequence _dtsSequence;
        private DTS.Executable _lastExecutable;
        private string _constraintMode;
        private string _transactionMode;

        public DTS.IDTSSequence DTSSequence
        {
            get { return _dtsSequence; }
        }

        public DTS.Executable AppendExecutable(string moniker)
        {
            DTS.Executable executable = _dtsSequence.Executables.Add(moniker);
            if (_constraintMode == "Linear")
            {
                if (_lastExecutable != null)
                {
                    _dtsSequence.PrecedenceConstraints.Add(_lastExecutable, executable);
                }
                _lastExecutable = executable;
            }
            return executable;
        }

        public SsisSequence(DTS.IDTSSequence dtsSequence, SequenceTask objContainer)
        {
            if (objContainer.ConstraintMode != "Linear" && objContainer.ConstraintMode != "Parallel")
            {
                MessageEngine.Global.Trace(Severity.Error, "Unknown ConstraintMode: {0} ", objContainer.ConstraintMode);
            }
            _constraintMode = objContainer.ConstraintMode;            
            _transactionMode = objContainer.TransactionMode;
            _dtsSequence = dtsSequence;
        }
    }
}
