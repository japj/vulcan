using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Table
{
    public partial class AstTableKeyBaseNode
    {
        protected AstTableKeyBaseNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }

        public int ComparisonBytes
        {
            get
            {
                int comparisonBytes = 0;
                foreach (AstTableKeyColumnNode keyColumn in this.Columns)
                {
                    if (keyColumn.Column != null)
                    {
                        // TODO: Add in size mapping for primitive types
                        comparisonBytes += keyColumn.Column.Length;
                    }
                }

                return comparisonBytes;
            }
        }

        public static bool StructureEquals(AstTableKeyBaseNode key1, AstTableKeyBaseNode key2)
        {
            if (key1 == null || key2 == null)
            {
                return key1 == null && key2 == null;
            }

            bool match = true;
            match &= key1.GetType().Equals(key2.GetType());
            match &= key1.Clustered == key2.Clustered;
            match &= key1.IgnoreDupKey == key2.IgnoreDupKey;
            match &= key1.PadIndex == key2.PadIndex;
            match &= key1.Unique == key2.Unique;
            match &= key1.Columns.Count == key2.Columns.Count;

            if (match)
            {
                for (int i = 0; i < key1.Columns.Count; i++)
                {
                    match &= AstTableKeyColumnNode.StructureEquals(key1.Columns[i], key2.Columns[i]);
                }
            }

            return match;
        }
    }
}
