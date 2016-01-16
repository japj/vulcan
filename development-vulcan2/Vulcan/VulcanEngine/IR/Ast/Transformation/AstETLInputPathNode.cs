using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VulcanEngine.IR.Ast.Transformation
{
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("ETLInputPathElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstETLInputPathNode : AstNamedNode
    {
        #region Private Storage
        private string _source;
        private string _pathName;
        #endregion   // Private Storage

        #region Public Accessor Properties
        [AstXNameBindingAttribute("__self", "http://tempuri.org/vulcan2.xsd", "@Source")]
        public string Source
        {
            get { return _source; }
            set { _source = value; }
        }

        [AstXNameBindingAttribute("__self", "http://tempuri.org/vulcan2.xsd", "@PathName")]
        public string PathName
        {
            get { return _pathName; }
            set { _pathName = value; }
        }

        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstETLInputPathNode()
        {
        }
        #endregion   // Default Constructor
    }
}
