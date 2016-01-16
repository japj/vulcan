using System;
using System.Collections.Generic;
using System.Text;

using Ssis2008Emitter.Properties;
using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.IR.TSQL
{
    public class Indexes : LogicalObject
    {
        private List<Index> _Indexes = new List<Index>();

        public IList<Index> IndexList
        {
            get { return _Indexes; }
        }
    }

    public abstract class IndexBase : LogicalObject
    {
        private List<Key> _keys = new List<Key>();
        private bool _clustered;
        private bool _unique;
        private bool _padIndex;
        private bool _sortInTempdb;
        private bool _dropExisting;
        private bool _ignoreDupKey;
        private bool _online;

        public IList<Key> Keys
        {
            get { return _keys; }
        }

        public bool Clustered
        {
            get { return _clustered; }
            set { _clustered = value; }
        }

        public bool Unique
        {
            get { return _unique; }
            set { _unique = value; }
        }

        public bool PadIndex
        {
            get { return _padIndex; }
            set { _padIndex = value; }
        }

        public bool SortInTempdb
        {
            get { return _sortInTempdb; }
            set { _sortInTempdb = value; }
        }

        public bool DropExisting
        {
            get { return _dropExisting; }
            set { _dropExisting = value; }
        }

        public bool IgnoreDupKey
        {
            get { return _ignoreDupKey; }
            set { _ignoreDupKey = value; }
        }

        public bool Online
        {
            get { return _online; }
            set { _online = value; }
        }

        public override void Initialize()
        {
            if (String.IsNullOrEmpty(this.Name) && this.Parent != null)
            {
                this.Name = Resources.IX + Resources.Seperator + Parent.Name;
            }
        }
    }

    public class Index : IndexBase
    {
        private List<Leaf> _leaves = new List<Leaf>();

        public IList<Leaf> Leaves
        {
            get { return _leaves; }
        }
    }

    public class Key : LogicalReference
    {
        private string _SortOrder = "ASC"; // default sort order

        public string SortOrder
        {
            get { return _SortOrder; }
            set { _SortOrder = value; }
        }
    }

    public class Leaf : LogicalObject { }
}
