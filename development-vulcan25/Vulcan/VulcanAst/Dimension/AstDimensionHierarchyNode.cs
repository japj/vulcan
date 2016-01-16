using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Dimension
{
    public partial class AstDimensionHierarchyNode
    {
        public AstDimensionHierarchyNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }

        public static bool StructureEquals(AstDimensionHierarchyNode hierarchy1, AstDimensionHierarchyNode hierarchy2)
        {
            if (hierarchy1 == null || hierarchy2 == null)
            {
                return hierarchy1 == null && hierarchy2 == null;
            }

            bool match = true;
            match &= hierarchy1.Levels.Count == hierarchy2.Levels.Count;

            if (match)
            {
                for (int i = 0; i < hierarchy1.Levels.Count; i++)
                {
                    match &= AstDimensionHierarchyLevelNode.StructureEquals(hierarchy1.Levels[i], hierarchy2.Levels[i]);
                }
            }

            return match;
        }
    }
}
