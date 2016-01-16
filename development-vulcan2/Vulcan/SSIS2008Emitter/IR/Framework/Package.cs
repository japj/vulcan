using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.Task;

namespace Ssis2008Emitter.IR.Framework
{
    public class Package : SequenceTask
    {
        #region Private Storage
        private string _defaultPlatform;
        private List<ConnectionConfiguration> _connectionConfigurations = new List<ConnectionConfiguration>();
        private List<PackageConfiguration> _packageConfigurations = new List<PackageConfiguration>();
        private string _type;
        #endregion  // Private Storage

        #region Public Accessor Properties
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public string DefaultPlatform
        {
            get { return _defaultPlatform; }
            set { _defaultPlatform = value; }
        }

        public IList<ConnectionConfiguration> ConnectionConfigurationList
        {
            get { return _connectionConfigurations; }
        }

        public IList<PackageConfiguration> PackageConfigurationList
        {
            get { return _packageConfigurations; }
        }
        #endregion  // Public Accessor Properties
    }
}
