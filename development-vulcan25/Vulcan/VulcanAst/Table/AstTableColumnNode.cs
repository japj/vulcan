using System;
using System.ComponentModel;
using AstFramework.Markup;
using AstFramework.Model;
using Vulcan.Utility.ComponentModel;

namespace VulcanEngine.IR.Ast.Table
{
    public partial class AstTableColumnNode
    {
        public AstTableColumnNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }

        private bool _nullable;

        [BrowsableAttribute(true)]
        [VulcanCategoryAttribute("Optional")]
        [VulcanDescriptionAttribute("Specifies IsNullable")]
        [VulcanDefaultValue(false)]
        [AstXNameBindingAttribute("IsNullable", ChildType.Attribute)]
        public override bool IsNullable
        {
            get { return _nullable; }
            set
            {
                if (_nullable != value)
                {
                    bool oldValue = _nullable;
                    _nullable = value;
                    VulcanOnPropertyChanged("IsNullable", oldValue, _nullable);
                }
            }
        }

        public override bool IsComputed
        {
            get { return !String.IsNullOrEmpty(Computed); }
        }

        public override bool IsAssignable
        {
            get { return base.IsAssignable && !IsComputed; }
        }
    }
}
