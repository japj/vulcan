using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ssis2008Emitter.IR.Common
{
    public class OutputPath : LogicalObject
    {
        public override string ToString()
        {
            return Name;
        }

        public OutputPath()
        {
        }

        public OutputPath(string name)
        {
            Name = name;
        }
    }

    public class InputPath : LogicalObject
    {
        #region Private Storage
        private string _source;
        #endregion  // Private Storage

        #region Public Accessor Properties
        public string Source
        {
            get { return _source; }
            set { _source = value; }
        }
        #endregion  // Public Accessor Properties

        public InputPath()
        {
        }

        public InputPath(string name, string source)
        {
            Name = name;
            Source = source;
        }
    }
}
