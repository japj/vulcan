using System;
using System.Globalization;
using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.IR.Framework.Connections
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public class FileConnection : Connection
    {
        private string _filePath;

        public FileConnection(string name, string filePath) : base(name)
        {
            ConnectionType = "FILE";
            FilePath = filePath;
        }

        public override void Emit(SsisEmitterContext context)
        {
            Initialize(context);
            SetExpression("ConnectionString", ConnectionString);
        }

        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                ConnectionString = String.Format(CultureInfo.InvariantCulture, "{0}", StringManipulation.BuildExpressionPath(_filePath)); 
            }
        }
    }
}
