using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.IR.DataFlow
{
    public class Lookup : Transformation
    {
        #region Private Storage
        private string _connection;
        private string _tableName;
        private string _query;
        private IList<LookupJoin> _input = new List<LookupJoin>();
        private IList<LookupJoin> _output = new List<LookupJoin>();
        #endregion  // Private Storage

        #region Public Accessor Properties
        public string Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        public string Table
        {
            get { return _tableName; }
            set { _tableName = value; }
        }

        public string Query
        {
            get { return _query; }
            set { _query = value; }
        }

        public IList<LookupJoin> InputList
        {
            get { return _input; }
            set { _input = value; }
        }

        public IList<LookupJoin> OutputList
        {
            get { return _output; }
            set { _output = value; }
        }
        #endregion  // Public Accessor Properties
    }

    public abstract class LookupJoin : LogicalObject
    {
        #region Private Storage
        private string _joinToReferenceColumn;
        private string _copyFromReferenceColumn;
        #endregion  // Private Storage

        #region Public Accessor Properties

        public string ReferenceColumnName
        {
            get
            {
                if (_joinToReferenceColumn != null)
                {
                    return _joinToReferenceColumn;
                }
                else if (_copyFromReferenceColumn != null)
                {
                    return _copyFromReferenceColumn;
                }
                else
                {
                    return null;
                }
            }
        }

        public string JoinToReferenceColumn
        {
            get { return _joinToReferenceColumn; }
            set { _joinToReferenceColumn = value; }
        }

        public string CopyFromReferenceColumn
        {
            get { return _copyFromReferenceColumn; }
            set { _copyFromReferenceColumn = value; }
        }
        #endregion  // Public Accessor Properties
    }

    public class LookupInputJoin : LookupJoin { }

    public class LookupOutputJoin : LookupJoin { }
}
