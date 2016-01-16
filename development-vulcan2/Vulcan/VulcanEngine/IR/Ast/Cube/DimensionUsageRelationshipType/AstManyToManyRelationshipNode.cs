using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VulcanEngine.IR.Ast.Cube.DimensionUsageRelationshipType
{
    public class AstManyToManyRelationshipNode : AstRelationshipNode
    {
        private MeasureGroup.AstMeasureGroupNode _intermediateMeasureGroup;

        public MeasureGroup.AstMeasureGroupNode IntermediateMeasureGroup
        {
            get { return _intermediateMeasureGroup; }
            set { _intermediateMeasureGroup = value; }
        }
    }
}
