using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.IR.DataFlow
{
    public class OLEDBCommand : Transformation
    {
        #region Private Storage
        private string _connection;
        private IList<Mapping> _mappings = new List<Mapping>();
        private string _command;
        #endregion  // Private Storage

        #region Public Accessor Properties
        public IList<Mapping> Mappings
        {
            get { return _mappings; }
            set { _mappings = value; }
        }

        public string Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        public string Command
        {
            get { return _command; }
            set { _command = value; }
        }
        #endregion  // Public Accessor Properties
    }
}
