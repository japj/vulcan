using System;
using System.Collections.Generic;
using System.Text;

using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.IR.TSQL
{
    public class DimensionMapping : LogicalObject
    {
        private string _dimensionColumn;
        private string _outputName;

        public string DimensionColumn
        {
            get { return _dimensionColumn; }
            set { _dimensionColumn = value; }
        }

        public string OutputName
        {
            get { return _outputName; }
            set { _outputName = value; }
        }
    }
}
