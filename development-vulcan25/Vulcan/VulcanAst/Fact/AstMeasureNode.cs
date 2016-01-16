using System;
using System.Globalization;
using AstFramework.Dataflow;
using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Fact
{
    public partial class AstMeasureNode : IDataflowItem
    {
        public bool HasExpression
        {
            get { return Expression != null; }
        }

        public bool HasAggregateColumn
        {
            get { return AggregateColumn != null; }
        }

        public string QualifiedName
        {
            get { return String.Format(CultureInfo.InvariantCulture, "[Measures].[{0}]", Name); }
        }

        public AstMeasureNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }
    }
}
