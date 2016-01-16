using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using Vulcan.IR.Physical.SSIS;
using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Transformation
{
    public abstract class AstTransformationNode : AstNamedNode
    {
        #region Private Storage
        private AstETLInputPathNode _inputPath;
        private AstETLOutputPathNode _outputPath;
        #endregion   // Private Storage

        private bool _validateExternalMetadata = true;

        public virtual bool ValidateExternalMetadata
        {
            get { return _validateExternalMetadata; }
            set { _validateExternalMetadata = value; }
        }

        #region Public Accessor Properties
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("InputPath", "http://tempuri.org/vulcan2.xsd")]
        public AstETLInputPathNode InputPath
        {
            get { return _inputPath; }
            set { _inputPath = value; }
        }
        
        [VulcanEngine.IR.Ast.AstXNameBindingAttribute("OutputPath", "http://tempuri.org/vulcan2.xsd")]
        public AstETLOutputPathNode OutputPath
        {
            get { return _outputPath; }
            set { _outputPath = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstTransformationNode()
            : base()
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
