using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using Vulcan.IR.Physical.SSIS;
using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Transformation
{
    public abstract class AstTransformationBlockNode : AstNamedNode
    {
        #region Private Storage

        #endregion   // Private Storage

        #region Public Accessor Properties

        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstTransformationBlockNode() : base()
        {
            /*
            _predecessors = new List<ISSISPhysical>();
            _successors = new List<ISSISPhysical>();
            _rootChildren = new List<ISSISPhysical>();
            _columnMappings = new List<SSISColumnMapping>();
             */
        }
        #endregion  // Default Constuctor

        #region Validation
        public override IList<ValidationItem> Validate()
        {
            List<ValidationItem> validationItems = new List<ValidationItem>();
            validationItems.AddRange(base.Validate());

            return validationItems;
        }
        #endregion  // Validation

    }
}
