using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ssis2008Emitter.IR.Common;

namespace Ssis2008Emitter.IR.DataFlow
{
    public class DerivedColumns : Transformation
    {
        #region Private Storage
        private List<DerivedColumn> _columns = new List<DerivedColumn>();
        #endregion  // Private Storage

        #region Public Accessor Properties
        public IList<DerivedColumn> Columns
        {
            get { return _columns; }
        }
        #endregion  // Public Accessor Properties
    }

    public class DerivedColumn : LogicalObject
    {
        #region Private Storage
        private string _type;
        private int _length;
        private int _precision;
        private int _scale;
        private int _codepage;
        private bool _replaceExisting;
        private string _expression;
        #endregion  // Private Storage

        #region Public Accessor Properties
        public String Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public int Length
        {
            get { return _length; }
            set { _length = value; }
        }

        public int Precision
        {
            get { return _precision; }
            set { _precision = value; }
        }

        public int Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        public int Codepage
        {
            get { return _codepage; }
            set { _codepage = value; }
        }

        public bool ReplaceExisting
        {
            get { return _replaceExisting; }
            set { _replaceExisting = value; }
        }

        public string Expression
        {
            get { return _expression; }
            set { _expression = value; }
        }
        #endregion  // Public Accessor Properties
    }
}
