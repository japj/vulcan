using System;
using System.Collections.Generic;
using System.Text;

using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.IR.TSQL
{
    public class Columns : LogicalGroup
    {
        private List<LogicalObject> _tags = new List<LogicalObject>();
        private List<Column> _columns = new List<Column>();
        private List<DimensionMapping> _dimensionMappings = new List<DimensionMapping>();

        public IList<Column> ColumnList
        {
            get { return _columns; }
        }

        public IList<DimensionMapping> DimensionMappingList
        {
            get { return _dimensionMappings; }
        }

        public IList<LogicalObject> Tags
        {
            get { return _tags; }
        }
    }
    public class Column : LogicalObject
    {
        private string _columnType;
        private bool _isNullable;
        private string _default = "";
        private string _computed = null;
        private bool _isAssignable = true;
        private bool _isComputed = false;

        public bool IsAssignable
        {
            get
            {
                return _isAssignable;
            }
            set
            {
                _isAssignable = value;
            }
        }

        public bool IsComputed
        {
            get
            {
                return _isComputed;
            }
            set
            {
                _isComputed = value;
            }
        }

        public string Computed
        {
            get { return _computed; }
            set { _computed = value; }
        }


        public string Type
        {
            get { return _columnType; }
            set { _columnType = value; }
        }

        public bool IsNullable
        {
            get { return _isNullable; }
            set { _isNullable = value; }
        }

        public string Default
        {
            get { return _default; }
            set { _default = value; }
        }
    }
}
