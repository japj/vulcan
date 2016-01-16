using System;
using System.Globalization;
using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Task
{
    public partial class AstStoredProcNode
    {
        public AstStoredProcNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }

        public string SchemaQualifiedName
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture,"[{0}]", this.Name);
            }
        }
    }
}
