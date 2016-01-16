using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VulcanEngine.IR.Ast.Cube.DimensionUsageRelationshipType
{
    public class AstReferencedRelationshipNode : AstRelationshipNode
    {
        private DimensionInstance.AstDimensionInstanceNode _intermediateDimensionInstance;
        private DimensionInstance.AstDimensionInstanceAttributeNode _referenceDimensionAttribute;
        private DimensionInstance.AstDimensionInstanceAttributeNode _intermediateDimensionAttribute;
        private bool _materialize;

        public DimensionInstance.AstDimensionInstanceNode IntermediateDimensionInstance
        {
            get { return _intermediateDimensionInstance; }
            set { _intermediateDimensionInstance = value; }
        }

        public DimensionInstance.AstDimensionInstanceAttributeNode ReferenceDimensionAttribute
        {
            get { return _referenceDimensionAttribute; }
            set { _referenceDimensionAttribute = value; }
        }

        public DimensionInstance.AstDimensionInstanceAttributeNode IntermediateDimensionAttribute
        {
            get { return _intermediateDimensionAttribute; }
            set { _intermediateDimensionAttribute = value; }
        }

        public bool Materialize
        {
            get { return _materialize; }
            set { _materialize = value; }
        }
    }
}
