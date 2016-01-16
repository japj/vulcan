using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Dimension
{
    public partial class AstDimensionHierarchyLevelNode
    {
        public AstDimensionHierarchyLevelNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }

        // TODO: Do we need to customize referenceablename on this class to include the DimensionHierarchy as well?
        public static bool StructureEquals(AstDimensionHierarchyLevelNode level1, AstDimensionHierarchyLevelNode level2)
        {
            if (level1 == null || level2 == null)
            {
                return level1 == null && level2 == null;
            }

            bool match = true;
            match &= level1.Attribute == level2.Attribute;

            return match;
        }
    }
}
