using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.IR.DataFlow
{
    public class ConditionalSplit : Transformation
    {
        #region Private Storage
        private List<Condition> _conditionList = new List<Condition>();
        private Condition _defaultCondition;
        #endregion  // Private Storage

        #region Public Accessor Properties
        public IList<Condition> ConditionList
        {
            get { return _conditionList; }
        }

        public Condition DefaultCondition
        {
            get { return _defaultCondition; }
            set { _defaultCondition = value; }
        }
        #endregion  // Public Accessor Properties
    }

    public class Condition : LogicalObject
    {
        #region Private Storage
        private string _expression;
        private string _outputPath;
        #endregion  // Private Storage

        #region Public Accessor Properties
        public string Expression
        {
            get { return _expression; }
            set { _expression = value; }
        }

        public string OutputPath
        {
            get { return _outputPath; }
            set { _outputPath = value; }
        }
        #endregion  // Public Accessor Properties
    }
}
