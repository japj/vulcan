using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VulcanEngine.IR.Ast.Cube
{
    public class AstKpiNode : AstNamedNode
    {
        // Attributes
        private MeasureGroup.AstMeasureGroupNode _associatedMeasureGroup;
        private string _displayFolder;
        private AstKpiNode _parentKPI;
        private string _description;
        // Elements
        //private ASTMDXExpressionNode _value;
        //private ASTMDXExpressionNode _goal;
        //private ASTMDXExpressionNode _status;
        //private ASTMDXExpressionNode _trend;
        //private ASTMDXExpressionNode _currentTimeMember;
        //private ASTMDXExpressionNode _weight;

        public MeasureGroup.AstMeasureGroupNode AssociatedMeasureGroup
        {
            get { return _associatedMeasureGroup; }
            set { _associatedMeasureGroup = value; }
        }

        public string DisplayFolder
        {
            get { return _displayFolder; }
            set { _displayFolder = value; }
        }

        public AstKpiNode ParentKPI
        {
            get { return _parentKPI; }
            set { _parentKPI = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public override IList<VulcanEngine.Common.ValidationItem> Validate()
        {
            throw new NotImplementedException();
        }
    }
}
