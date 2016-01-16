using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.IR.DataFlow
{
    public class UnionAll : Transformation
    {
        #region Private Storage
        private List<UnionInputPath> _inputPathList = new List<UnionInputPath>();
        #endregion  // Private Storage

        #region Public Accessor Properties
        public IList<UnionInputPath> InputPathList
        {
            get { return _inputPathList; }
        }
        #endregion  // Public Accessor Properties
    }

    public class UnionInputPath : InputPath
    {
        #region Private Storage
        private List<UnionMapping> _mappingList = new List<UnionMapping>();
        #endregion  // Private Storage

        #region Public Accessor Properties
        public List<UnionMapping> MappingList
        {
            get { return _mappingList; }
            set { _mappingList = value; }
        }
        #endregion  // Public Accessor Properties
    }

    public class UnionMapping : LogicalObject
    {
        #region Private Storage
        private string _input;
        private string _output;
        #endregion  // Private Storage

        #region Public Accessor Properties
        public string Input
        {
            get { return _input; }
            set { _input = value; }
        }

        public string Output
        {
            get { return _output; }
            set { _output = value; }
        }
        #endregion  // Public Accessor Properties
    }
}
