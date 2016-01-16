using System;
using System.Collections.Generic;
using System.Text;

using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.IR.TSQL
{
    public class TablePattern : LogicalObject
    {
        private Table _table;
        private Framework.Connection _connection;
        private bool _executeDuringDesignTime;

        public override string Name
        {
            get { return _table != null ? _table.Name : null; }
            set
            {
                if (_table != null)
                {
                    _table.Name = value;
                }
            }
        }

        public Table Table
        {
            get { return _table; }
            set { _table = value; }
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
}
