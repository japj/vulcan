using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.IR.DataFlow
{
    public class TermLookup : Transformation
    {
        #region Private Storage
        private string _refTermTable;
        private string _refTermColumn;
        private bool _isCaseSensitive;
        private string _connection;
        private IList<InputColumn> _inputColumnList = new List<InputColumn>();
        #endregion  // Private Storage

        #region Public Accessor Properties
        public string Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        public bool IsCaseSensitive
        {
            get { return _isCaseSensitive; }
            set { _isCaseSensitive = value; }
        }

        public string RefTermColumn
        {
            get { return _refTermColumn; }
            set { _refTermColumn = value; }
        }

        public string RefTermTable
        {
            get { return _refTermTable; }
            set { _refTermTable = value; }
        }

        public IList<InputColumn> InputColumnList
        {
            get { return _inputColumnList; }
            set { _inputColumnList = value; }
        }
        #endregion  // Public Accessor Properties
    }

    public class InputColumn : LogicalObject
    {
        #region Private Storage
        private string _usageType;
        #endregion  // Private Storage

        #region Public Accessor Properties
        public string UsageType
        {
            get { return _usageType; }
            set { _usageType = value; }
        }
        #endregion  // Public Accessor Properties
    }
}
