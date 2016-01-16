using System;
using System.Collections.Generic;
using System.Text;

using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.IR.DataFlow
{
    // TODO: public class Transformation : LogicalObject { }

    public class Transformation : LogicalObject
    {
        #region Private Storage
        private InputPath _inputPath;
        private OutputPath _outputPath;
        private string _astNodeName;
        private bool _validateExternalMetadata = true;
        #endregion  // Private Storage

        #region Public Accessor Properties
        
        public bool ValidateExternalMetadata
        {
            get { return _validateExternalMetadata; }
            set { _validateExternalMetadata = value; }
        }

        public string AstNodeName
        {
            get { return _astNodeName; }
            set { _astNodeName = value; }
        }

        public InputPath InputPath
        {
            get { return _inputPath; }
            set { _inputPath = value; }
        }

        public OutputPath OutputPath
        {
            get { return _outputPath; }
            set { _outputPath = value; }
        }
        #endregion  // Public Accessor Properties

        public Transformation()
        {
            this._inputPath = new InputPath();
            this._outputPath = new OutputPath();
        }
    }
}
