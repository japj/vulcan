using AstFramework;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.Phases.Lowering.Framework;
using VulcanEngine.Common;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Transformation;

namespace Ssis2008Emitter.IR.Tasks.Transformations
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public class DerivedColumns : SingleOutTransformation
    {
        private readonly AstDerivedColumnListNode _astDerivedColumnListNode;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        public static void CreateAndRegister(AstNode astNode, LoweringContext context)
        {
            context.ParentObject.Children.Add((new DerivedColumns(context, astNode)));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Required for Emitter pattern.")]
        public DerivedColumns(LoweringContext context, AstNode astNode)
            : base(context, astNode as AstTransformationNode)
        {
            _astDerivedColumnListNode = astNode as AstDerivedColumnListNode;
            RegisterInputBinding(_astDerivedColumnListNode);
        }

        public override string Moniker
        {
            get { return "DTSTransform.DerivedColumn"; }
        }

        public override void Initialize(SsisEmitterContext context)
        {
            base.Initialize(context);
            Flush();
        }

        public override void Emit(SsisEmitterContext context)
        {
            ProcessBindings(context);
            foreach (AstDerivedColumnNode dc in _astDerivedColumnListNode.Columns)
            {
                if (dc.ReplaceExisting)
                {
                    EmitReplaceExisting(dc);
                }
                else
                {
                    EmitAddNewColumn(dc);
                }
            }
        }

        private void EmitReplaceExisting(AstDerivedColumnNode dc)
        {
            IDTSInputColumn100 col = SetInputColumnUsage(0, dc.Name, DTSUsageType.UT_READWRITE, true);
            IDTSVirtualInput100 vi = Component.InputCollection[0].GetVirtualInput();
            Expression expression = ExpressionHandler.ExpressionCleanerAndInputMapBuilder(dc.Expression, this, vi, DTSUsageType.UT_READONLY);

            col.CustomPropertyCollection["Expression"].Value = expression.ProcessedExpression;
            col.CustomPropertyCollection["FriendlyExpression"].Value = expression.FriendlyExpression;
        }

        private void EmitAddNewColumn(AstDerivedColumnNode column)
        {
            IDTSOutputColumn100 col = Component.OutputCollection[0].OutputColumnCollection.New();
            col.Name = column.Name;

            col.ErrorRowDisposition = DTSRowDisposition.RD_IgnoreFailure;
            col.TruncationRowDisposition = DTSRowDisposition.RD_FailComponent;

            // TODO: Workaround for AstDesigner bug where IsExplicitlySet isn't working
            col.SetDataTypeProperties(
                GetDataTypeFromString(column.DerivedColumnType),
                column.IsLengthExplicitlySet || column.Length > 0 ? column.Length : 0,
                column.IsPrecisionExplicitlySet || column.Precision > 0 ? column.Precision : 0,
                column.IsScaleExplicitlySet || column.Scale > 0 ? column.Scale : 0,
                column.IsCodepageExplicitlySet || column.Codepage > 0 ? column.Codepage : 0);
            col.ExternalMetadataColumnID = 0;
            col.ErrorOrTruncationOperation = "Computation";

            IDTSVirtualInput100 vi = Component.InputCollection[0].GetVirtualInput();

            IDTSCustomProperty100 propExpression = col.CustomPropertyCollection.New();
            propExpression.Name = "Expression";
            Expression exp = ExpressionHandler.ExpressionCleanerAndInputMapBuilder(column.Expression.Trim(), this, vi, DTSUsageType.UT_READONLY);
            propExpression.Value = exp.ProcessedExpression;
            propExpression.ContainsID = exp.ContainsId;

            IDTSCustomProperty100 propFriendlyExpression = col.CustomPropertyCollection.New();
            propFriendlyExpression.Name = "FriendlyExpression";
            propFriendlyExpression.Value = exp.FriendlyExpression;
            propFriendlyExpression.ExpressionType = DTSCustomPropertyExpressionType.CPET_NOTIFY;
            propFriendlyExpression.ContainsID = exp.ContainsId;
        }

        protected DataType GetDataTypeFromString(ColumnType ColumnType)
        {
            switch (ColumnType)
            {
                case ColumnType.AnsiString: return DataType.DT_STR;
                case ColumnType.Binary: return DataType.DT_BYTES;
                case ColumnType.Byte: return DataType.DT_UI1;
                case ColumnType.Boolean: return DataType.DT_BOOL;
                case ColumnType.Currency: return DataType.DT_CY;
                case ColumnType.Date: return DataType.DT_DBDATE;
                case ColumnType.DateTime: return DataType.DT_DBTIMESTAMP;
                case ColumnType.Decimal: return DataType.DT_DECIMAL;
                case ColumnType.Double: return DataType.DT_R8;
                case ColumnType.Guid: return DataType.DT_GUID;
                case ColumnType.Int16: return DataType.DT_I2;
                case ColumnType.Int32: return DataType.DT_I4;
                case ColumnType.Int64: return DataType.DT_I8;
                case ColumnType.Object: return DataType.DT_EMPTY;
                case ColumnType.SByte: return DataType.DT_I1;
                case ColumnType.Single: return DataType.DT_R4;
                case ColumnType.String: return DataType.DT_WSTR;
                case ColumnType.Time: return DataType.DT_DBTIME;
                case ColumnType.UInt16: return DataType.DT_UI2; 
                case ColumnType.UInt32: return DataType.DT_UI4; 
                case ColumnType.UInt64: return DataType.DT_UI8;
                case ColumnType.VarNumeric: return DataType.DT_NUMERIC;
                case ColumnType.AnsiStringFixedLength: return DataType.DT_STR;
                case ColumnType.StringFixedLength: return DataType.DT_WSTR;
                case ColumnType.Xml: return DataType.DT_NTEXT;
                case ColumnType.DateTime2: return DataType.DT_DBTIMESTAMP2;
                case ColumnType.DateTimeOffset: return DataType.DT_DBTIMESTAMPOFFSET;

                default:
                    MessageEngine.Trace(_astDerivedColumnListNode, Severity.Error, "V0104", "Error in Type {0} - Unhandled VULCAN SSIS Type", ColumnType.ToString()); 
                    return DataType.DT_NULL;
            }
        }
    }
}