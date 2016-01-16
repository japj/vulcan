using System;
using System.Collections.Generic;
using System.Text;

using Ssis2008Emitter.IR;
using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.IR.TSQL
{
    public class StoredProc : LogicalObject
    {
        private string _body;
        private StoredProcColumns _columns = new StoredProcColumns();
        private Framework.Connection _connection;
        private bool _executeDuringDesignTime;

        public string Body
        {
            get { return _body; }
            set { _body = value; }
        }

        public StoredProcColumns Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }

        public Framework.Connection Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        public bool ExecuteDuringDesignTime
        {
            get { return _executeDuringDesignTime; }
            set { _executeDuringDesignTime = value; }
        }

    }

    public class StoredProcColumns : LogicalObject
    {
        private List<StoredProcColumn> _columns = new List<StoredProcColumn>();

        public StoredProcColumns() { }

        public IList<StoredProcColumn> ColumnList
        {
            get { return _columns; }
        }
    }

    public class StoredProcColumn : LogicalObject
    {
        private string _ColumnType;
        private bool _IsOutput;
        private string _Default;

        public string Type
        {
            get { return _ColumnType; }
            set { _ColumnType = value; }
        }

        public bool IsOutput
        {
            get { return _IsOutput; }
            set { _IsOutput = value; }
        }

        public string Default
        {
            get { return _Default; }
            set { _Default = value; }
        }
    }
}
