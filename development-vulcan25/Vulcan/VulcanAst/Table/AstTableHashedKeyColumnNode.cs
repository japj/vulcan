using System.ComponentModel;
using AstFramework.Markup;
using AstFramework.Model;
using Vulcan.Utility.ComponentModel;

namespace VulcanEngine.IR.Ast.Table
{
    public partial class AstTableHashedKeyColumnNode
    {
        public AstTableHashedKeyColumnNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }

        public override bool IsComputed
        {
            get { return true; }
        }

        public override bool IsAssignable
        {
            get { return false; }
        }

        [BrowsableAttribute(true)]
        [VulcanCategoryAttribute("Optional")]
        [VulcanDescriptionAttribute("Specifies IsNullable")]
        [VulcanDefaultValue(true)]
        [AstXNameBinding("IsNullable", ChildType.Attribute)]
        public override bool IsNullable
        {
            get { return false; }
            set { } //// throw new NotImplementedException(); }
        }
    }
}
