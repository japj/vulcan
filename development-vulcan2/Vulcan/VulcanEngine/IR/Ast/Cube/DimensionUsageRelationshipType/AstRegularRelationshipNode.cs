using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VulcanEngine.IR.Ast.Cube.DimensionUsageRelationshipType
{
    public class AstRegularRelationshipNode : AstRelationshipNode
    {
        private DimensionInstance.AstDimensionInstanceAttributeNode _granularityAttribute;
        //private Fact.AstFactColumnNode _factTableColumn;

        public DimensionInstance.AstDimensionInstanceAttributeNode GranularityAttribute
        {
            get { return _granularityAttribute; }
            set { _granularityAttribute = value; }
        }
    }
}
