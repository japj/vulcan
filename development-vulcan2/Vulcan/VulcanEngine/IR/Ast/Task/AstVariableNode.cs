using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Task
{
    [VulcanEngine.IR.Ast.AstSchemaTypeBindingAttribute("VariableTaskElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstVariableNode : AstTaskNode
    {
        #region Private Storage
        private VariableTypeFacet _type;
        private string _value;
        #endregion   // Private Storage

        #region Public Accessor Properties
        public VariableTypeFacet Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstVariableNode() : base() { }
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

    public enum VariableTypeFacet
    {
        Boolean,
        Byte,
        Char,
        DateTime,
        DBNull,
        Double,
        Int16,
        Int32,
        Int64,
        Object,
        Sbyte,
        Single,
        String,
        UInt32,
        UInt64
    }
}
