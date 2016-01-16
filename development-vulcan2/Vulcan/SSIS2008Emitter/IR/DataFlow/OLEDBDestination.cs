using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.IR.DataFlow
{
    public class Destination : Transformation
    {
        #region Private Storage
        private string _connection;
        private string _table;
        private string _accessMode;
        private bool _tableLock;
        private bool _checkConstraints;
        private bool _keepIdentity;
        private bool _keepNulls;
        private int _rowsPerBatch;
        private int _maximumInsertCommitSize;
        private List<Mapping> _mappings = new List<Mapping>();
        private bool _useStaging = false;

        #endregion  // Private Storage

        #region Public Accessor Properties
        public bool UseStaging
        {
            get { return _useStaging; }
            set { _useStaging = value; }
        }

        public IList<Mapping> Mappings
        {
            get { return _mappings; }
        }

        public string Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        public string Table
        {
            get { return _table; }
            set { _table = value; }
        }

        public string AccessMode
        {
            get { return _accessMode; }
            set { _accessMode = value; }
        }

        public bool TableLock
        {
            get { return _tableLock; }
            set { _tableLock = value; }
        }

        public bool CheckConstraints
        {
            get { return _checkConstraints; }
            set { _checkConstraints = value; }
        }

        public bool KeepIdentity
        {
            get { return _keepIdentity; }
            set { _keepIdentity = value; }
        }

        public bool KeepNulls
        {
            get { return _keepNulls; }
            set { _keepNulls = value; }
        }

        public int RowsPerBatch
        {
            get { return _rowsPerBatch; }
            set { _rowsPerBatch = value; }
        }

        public int MaximumInsertCommitSize
        {
            get { return _maximumInsertCommitSize; }
            set { _maximumInsertCommitSize = value; }
        }

        #endregion  // Public Accessor Properties
    }

    public class Mapping : LogicalObject
    {
        #region Private Storage
        private string _source;
        private string _destination;
        #endregion  // Private Storage

        #region Public Accessor Properties
        public string Source
        {
            get { return _source; }
            set { _source = value; }
        }

        public string Destination
        {
            get { return _destination; }
            set { _destination = value; }
        }
        #endregion  // Public Accessor Properties
    }
}