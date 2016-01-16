using System;
using System.Collections.Generic;
using Vulcan.Utility.Collections;

namespace Vulcan.Utility.Graph
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Name follows the naming convention of the Graph library.")]
    public class BasicBlock<T> : VulcanCollection<T>
    {
        private Guid _guid;

        public T Leader
        {
            get { return Count > 0 ? this[0] : default(T); }
        }

        public T Last
        {
            get { return Count > 0 ? this[Count - 1] : default(T); }
        }

        public BasicBlock<T> Clone()
        {
            return new BasicBlock<T>(this);
        }

        /*
        public List<DataflowItem> DefinedIn
        {
            get
            {
                // TODO: Cache this with a dirty bit
                var dataflowItems = new List<DataflowItem>();
                foreach (var tuple in this)
                {
                    foreach (var operand in tuple.LeftHandSide)
                    {
                        dataflowItems.Add(operand.DataflowItem);
                    }
                }
                return dataflowItems;
            }
        }
        */

        public BasicBlock() : base()
        {
            _guid = Guid.NewGuid();
        }

        public BasicBlock(IEnumerable<T> items)
            : base(items)
        {
            _guid = Guid.NewGuid();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Parallels the constructor from ObservableCollection.")]
        public BasicBlock(List<T> items)
            : base(items)
        {
            _guid = Guid.NewGuid();
        }

        public override string ToString()
        {
            return _guid.ToString();
        }
    }
}
