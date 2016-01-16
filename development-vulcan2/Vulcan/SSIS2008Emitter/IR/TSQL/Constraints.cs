using System;
using System.Collections.Generic;
using System.Text;

using Ssis2008Emitter.Properties;
using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.IR.TSQL
{
    public class Constraint : IndexBase
    {
        public override void Initialize()
        {
            if (String.IsNullOrEmpty(this.Name) && this.Parent != null)
            {
                this.Name = Resources.CON + Resources.Seperator + Parent.Name;
            }
        }
    }

    public class PrimaryKeyConstraint : Constraint
    {
        public override void Initialize()
        {
            if (String.IsNullOrEmpty(this.Name) && this.Parent != null)
            {
                this.Name = Resources.PK + Resources.Seperator + Parent.Name;
            }
        }
    }

    public class NaturalKeyConstraint : Constraint
    {
        public override void Initialize()
        {
            if (String.IsNullOrEmpty(this.Name) && this.Parent != null)
            {
                this.Name = Resources.NK + Resources.Seperator + Parent.Name;
            }
        }
    }

    public class IdentityConstraint : Constraint
    {
        private int _seed;
        private int _increment;

        public int Seed
        {
            get { return _seed; }
            set { _seed = value; }
        }

        public int Increment
        {
            get { return _increment; }
            set { _increment = value; }
        }

        public override void Initialize()
        {
            if (String.IsNullOrEmpty(this.Name) && this.Parent != null)
            {
                this.Name = Resources.PK + Resources.Seperator + Parent.Name;
            }
        }
    }

    public class ForeignKeyConstraint : LogicalObject
    {
        private List<Key> _localColumnList = new List<Key>();
        private List<Key> _foreignColumnList = new List<Key>();
        private string _table;

        public IList<Key> LocalColumnList
        {
            get { return _localColumnList; }
        }

        public IList<Key> ForeignColumnList
        {
            get { return _foreignColumnList; }
        }

        public string Table
        {
            get { return _table; }
            set { _table = value; }
        }
    }

    public class LocalColumn : Key { }

    public class ForeignColumn : Key { }
}