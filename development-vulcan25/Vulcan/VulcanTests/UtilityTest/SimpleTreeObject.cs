using System;
using System.Globalization;
using Vulcan.Utility.Tree;

namespace UtilityTest
{
    public class SimpleTreeObjectCollection : TreeNode<SimpleTreeObjectCollection>
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return String.Format(
                                 CultureInfo.InvariantCulture,
                                 "{0}: Name={1}, Depth={2}, Parent={3}",
                                 GetType(),
                                 Name,
                                 Depth,
                                 Parent == null ? "<NULL>" : Parent.Name);
        }
    }
}
