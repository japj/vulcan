using System.ComponentModel;
using AstFramework.Model;
using Vulcan.Utility.Collections;

namespace VulcanEngine.IR.Ast.Table
{
    public partial class AstTableIndexNode
    {
        private VulcanCollection<AstNode> _items;

        public AstTableIndexNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            _items = new VulcanCollection<AstNode>();

            InitializeAstNode();

            CollectionPropertyChanged += AstTableIndexNode_CollectionPropertyChanged;
        }

        private void AstTableIndexNode_CollectionPropertyChanged(object sender, VulcanCollectionPropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Columns" || e.PropertyName == "Leafs")
            {
                VulcanCompositeCollectionChanged(_items, e);                
            }
        }

        [Browsable(false)]
        public VulcanCollection<AstNode> Items
        {
            get { return _items; }
        }

        public static bool StructureEquals(AstTableIndexNode index1, AstTableIndexNode index2)
        {
            if (index1 == null || index2 == null)
            {
                return index1 == null && index2 == null;
            }

            bool match = true;
            match &= index1.Clustered == index2.Clustered;
            match &= index1.DropExisting == index2.DropExisting;
            match &= index1.IgnoreDupKey == index2.IgnoreDupKey;
            match &= index1.Online == index2.Online;
            match &= index1.PadIndex == index2.PadIndex;
            match &= index1.SortInTempDB == index2.SortInTempDB;
            match &= index1.Unique == index2.Unique;
            match &= index1.Columns.Count == index2.Columns.Count;
            match &= index1.Leafs.Count == index2.Leafs.Count;

            if (match)
            {
                for (int i = 0; i < index1.Columns.Count; i++)
                {
                    match &= AstTableIndexColumnNode.StructureEquals(index1.Columns[i], index2.Columns[i]);
                }

                for (int i = 0; i < index1.Leafs.Count; i++)
                {
                    match &= index1.Leafs[i] == index2.Leafs[i];
                }
            }

            return match;
        }
    }
}
