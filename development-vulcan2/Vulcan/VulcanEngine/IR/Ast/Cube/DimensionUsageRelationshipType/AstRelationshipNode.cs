using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VulcanEngine.IR.Ast;

namespace VulcanEngine.IR.Ast.Cube.DimensionUsageRelationshipType
{
    public abstract class AstRelationshipNode
    {
        private DimensionInstance.AstDimensionInstanceNode _dimensionInstance;
        private MeasureGroup.AstMeasureGroupNode _measureGroup;

        public DimensionInstance.AstDimensionInstanceNode DimensionInstance
        {
            get { return _dimensionInstance; }
            set { _dimensionInstance = value; }
        }

        public MeasureGroup.AstMeasureGroupNode MeasureGroup
        {
            get { return _measureGroup; }
            set { _measureGroup = value; }
        }
    }
}
