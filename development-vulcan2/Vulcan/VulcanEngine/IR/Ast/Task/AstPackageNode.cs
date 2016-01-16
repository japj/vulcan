using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Task
{
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("PackageElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstPackageNode : Task.AstContainerTaskNode, IPackageRootNode
    {
        #region Private Storage
        private PlatformType _defaultPlatform;
        private string _type;
        private bool _emit = true;
        #endregion   // Private Storage

        #region Public Accessor Properties

        public bool Emit
        {
            get { return _emit; }
            set { _emit = value; }
        }
        public PlatformType DefaultPlatform
        {
            get { return _defaultPlatform; }
            set { _defaultPlatform = value; }
        }

        // TODO: Set to enum or remove if possible
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstPackageNode() { }
        #endregion   // Default Constructor

        #region Validation
        public override IList<ValidationItem> Validate()
        {
            List<ValidationItem> validationItems = new List<ValidationItem>();
            validationItems.AddRange(base.Validate());

            return validationItems;
        }
        #endregion  // Validation
    }

    public enum PlatformType
    {
      SSIS05,
      SSIS08,
      METL,
      Debug,
    }
}
