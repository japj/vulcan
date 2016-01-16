using System;
using AstFramework;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.Framework.Connections;
using Ssis2008Emitter.Phases.Lowering.Framework;
using VulcanEngine.Common;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Transformation;
using AST = VulcanEngine.IR.Ast;
using DTS = Microsoft.SqlServer.Dts.Runtime;

namespace Ssis2008Emitter.IR.Tasks.Transformations
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public class SlowlyChangingDimension : Transformation
    {
        private readonly AstSlowlyChangingDimensionNode _astSlowlyChangingDimensionNode;
        private OleDBConnection _oleDBConnection;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        public static void CreateAndRegister(AstNode astNode, LoweringContext context)
        {
            var astSlowlyChangingDimensionNode = astNode as AstSlowlyChangingDimensionNode;
            if (astSlowlyChangingDimensionNode != null)
            {
                var slowlyChangingDimension = new SlowlyChangingDimension(context, astNode) { _oleDBConnection = new OleDBConnection(astSlowlyChangingDimensionNode.Connection) };
                context.ParentObject.Children.Add(slowlyChangingDimension._oleDBConnection);
                context.ParentObject.Children.Add(slowlyChangingDimension);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Required for Emitter pattern.")]
        public SlowlyChangingDimension(LoweringContext context, AstNode astNode) : base(context, astNode as AstTransformationNode)
        {
            _astSlowlyChangingDimensionNode = astNode as AstSlowlyChangingDimensionNode;
            RegisterInputBinding(_astSlowlyChangingDimensionNode);
        }

        public override string Moniker
        {
            get { return "DTSTransform.SCD"; }
        }

        public override void Initialize(SsisEmitterContext context)
        {
            base.Initialize(context);
            TransformationUtility.RegisterOleDBConnection(context, _oleDBConnection, Component);

            Instance.SetComponentProperty("SqlCommand", _astSlowlyChangingDimensionNode.Query);
            Instance.SetComponentProperty("CurrentRowWhere", _astSlowlyChangingDimensionNode.CurrentRowWhere);
            Instance.SetComponentProperty("EnableInferredMember", _astSlowlyChangingDimensionNode.EnableInferredMember);
            Instance.SetComponentProperty("FailOnFixedAttributeChange", _astSlowlyChangingDimensionNode.FailOnFixedAttributeChange);
            Instance.SetComponentProperty("FailOnLookupFailure", _astSlowlyChangingDimensionNode.FailOnLookupFailure);
            Instance.SetComponentProperty("IncomingRowChangeType", _astSlowlyChangingDimensionNode.IncomingRowChangeType);
            Instance.SetComponentProperty("UpdateChangingAttributeHistory", _astSlowlyChangingDimensionNode.UpdateChangingAttributeHistory);

            string inferredMemberIndicatorName = String.Empty;
            if (_astSlowlyChangingDimensionNode.InferredMemberIndicator != null)
            {
                inferredMemberIndicatorName = _astSlowlyChangingDimensionNode.InferredMemberIndicator.Name;
            }

            Instance.SetComponentProperty("InferredMemberIndicator", inferredMemberIndicatorName);

            Flush();
        }

        public override void Emit(SsisEmitterContext context)
        {
            ProcessBindings(context);

            // TODO: Add Input column custom property mapping
            foreach (var column in _astSlowlyChangingDimensionNode.Mappings)
            {
                int columnType;
                switch (column.MappingType)
                {
                    case ScdColumnMappingType.ChangingAttribute: 
                        columnType = 2;
                        break;
                    case ScdColumnMappingType.FixedAttribute: 
                        columnType = 4; 
                        break;
                    case ScdColumnMappingType.HistoricalAttribute: 
                        columnType = 3; 
                        break;
                    case ScdColumnMappingType.Key: 
                        columnType = 1; 
                        break;
                    case ScdColumnMappingType.Other: 
                        columnType = 0; 
                        break;
                    default: 
                        MessageEngine.Trace(_astSlowlyChangingDimensionNode, Severity.Error, "V0140", "Unrecognized ScdColumnMappingType {0} on column {1}", column.MappingType.ToString(), column.QueryColumnName);
                        return;
                }

                IDTSInputColumn100 inputColumn = TransformationUtility.FindInputColumnByName(column.QueryColumnName, Component.InputCollection[0], true);
                if (inputColumn == null)
                {
                    IDTSVirtualInputColumn100 virtualInputColumn = TransformationUtility.FindVirtualInputColumnByName(column.QueryColumnName, Component.InputCollection[0], true);
                    if (virtualInputColumn == null)
                    {
                        MessageEngine.Trace(_astSlowlyChangingDimensionNode, Severity.Error, "V0141", "Column {0} could not be found", column.QueryColumnName);
                        return; 
                    }

                    inputColumn = Component.InputCollection[0].InputColumnCollection.New();
                    inputColumn.Name = column.QueryColumnName;
                    inputColumn.LineageID = virtualInputColumn.LineageID;
                }

                IDTSCustomProperty100 propColumnType = TransformationUtility.FindCustomPropertyByName("ColumnType", inputColumn.CustomPropertyCollection, true);
                if (propColumnType == null)
                {
                    propColumnType = inputColumn.CustomPropertyCollection.New();
                    propColumnType.Name = "ColumnType";
                    propColumnType.TypeConverter = "ColumnType";
                }

                propColumnType.Value = columnType;
            }
        }
    }
}

