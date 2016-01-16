using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Ssis2008Emitter.Properties;

namespace Ssis2008Emitter.IR.Common
{
    public abstract class LogicalObject
    {
        #region Private Storage
        // metadata
        private LogicalObject _mtParent;
        private List<LogicalObject> _mtChildren = new List<LogicalObject>();
        private List<LogicalReference> _mtReferences = new List<LogicalReference>();

        private string _name;
        #endregion  // Private Storage

        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        //Other information
        public LogicalObject Parent
        {
            get { return _mtParent; }
            set { _mtParent = value; }
        }

        public IList<LogicalObject> Children
        {
            get { return _mtChildren; }
        }

        public IList<LogicalReference> References
        {
            get { return _mtReferences; }
        }

        public override string ToString()
        {
            if (Name != null)
            {
                return this.GetType().ToString()
                    + Resources.LogicalNameSeperator
                    + this.Name;
            }
            else
            {
                return this.GetType().ToString();
            }
        }

        public virtual void Initialize()
        {
        }

        public virtual void AddChild(LogicalObject obj)
        {
            _mtChildren.Add(obj);
            obj.Parent = this;
        }

        public virtual void InsertChild(int index, LogicalObject obj)
        {
            _mtChildren.Insert(index, obj);
            obj.Parent = this;
        }
    }

    public abstract class LogicalGroup : LogicalObject
    {
    }
}
