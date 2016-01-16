using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VulcanEngine.IR.Ast.Cube
{
    public class AstDrillthroughActionNode : AstNamedNode
    {
        // Attributes
        // <xs:attribute name="measureGroupMembers" type="MeasureGroupListWithAllFacet" use="required" />
        //private ;
        private bool _default;
        private int _maximumRows;
        private string _invocation;
        private string _application;
        private string _description;
        private string _caption;
        private bool _captionIsMDX;
        // Elements
        //private AstMDXExpressionNode _condition;
        //private List<AstSSAScolumn> _drillthroughColumns;

        public bool Default
        {
            get { return _default; }
            set { _default = value; }
        }

        public int MaximumRows
        {
            get { return _maximumRows; }
            set { _maximumRows = value; }
        }

        public string Invocation
        {
            get { return _invocation; }
            set { _invocation = value; }
        }

        public string Application
        {
            get { return _application; }
            set { _application = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public string Caption
        {
            get { return _caption; }
            set { _caption = value; }
        }

        public bool CaptionIsMDX
        {
            get { return _captionIsMDX; }
            set { _captionIsMDX = value; }
        }

        public override IList<VulcanEngine.Common.ValidationItem> Validate()
        {
            throw new NotImplementedException();
        }
    }
}
