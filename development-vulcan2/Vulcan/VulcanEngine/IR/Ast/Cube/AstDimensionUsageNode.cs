using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Cube
{
    public class AstDimensionUsageNode
    {
        // Attributes
        private bool _autoGenerateRelationships;
        // Elements
        private VulcanCollection<DimensionUsageRelationshipType.AstRelationshipNode> _relationships;

        public bool AutoGenerateRelationships
        {
            get { return _autoGenerateRelationships; }
            set { _autoGenerateRelationships = value; }
        }

        public VulcanCollection<DimensionUsageRelationshipType.AstRelationshipNode> Relationships
        {
            get { return _relationships; }
            set { _relationships = value; }
        }
    }
}
