using System;
using System.Globalization;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.Framework.Connections;
using Ssis2008Emitter.Phases.Lowering.Framework;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Transformation;
using DTS = Microsoft.SqlServer.Dts.Runtime;

namespace Ssis2008Emitter.IR.Tasks.Transformations
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public class TermLookup : SingleOutTransformation
    {
        private readonly AstTermLookupNode _astTermLookupNode;
        private OleDBConnection _oleDBConnection;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        public static void CreateAndRegister(AstNode astNode, LoweringContext context)
        {
            var astTermLookupNode = astNode as AstTermLookupNode;
            if (astTermLookupNode != null)
            {
                var termLookup = new TermLookup(context, astNode) { _oleDBConnection = new OleDBConnection(astTermLookupNode.Connection) };
                context.ParentObject.Children.Add(termLookup._oleDBConnection);
                context.ParentObject.Children.Add(termLookup);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Required for Emitter pattern.")]
        public TermLookup(LoweringContext context, AstNode astNode) : base(context, astNode as AstTransformationNode)
        {
            _astTermLookupNode = astNode as AstTermLookupNode;
            RegisterInputBinding(_astTermLookupNode);
        }

        public override string Moniker
        {
            get { return "DTSTransform.TermLookup"; }
        }

        public override void Initialize(SsisEmitterContext context)
        {
            base.Initialize(context);
            TransformationUtility.RegisterOleDBConnection(context, _oleDBConnection, Component);

            Instance.SetComponentProperty("IsCaseSensitive", _astTermLookupNode.IsCaseSensitive);
            Instance.SetComponentProperty("RefTermColumn", _astTermLookupNode.RefTermColumnName);
            Instance.SetComponentProperty("RefTermTable", _astTermLookupNode.RefTermTableName);

            Component.OutputCollection[0].ErrorRowDisposition = DTSRowDisposition.RD_IgnoreFailure;
            ProcessBindings(context);
            Flush();
        }

        public override void Emit(SsisEmitterContext context)
        {
            if (_astTermLookupNode.AutoPassthrough)
            {
                foreach (string inputColumnName in GetVirtualInputColumns(0))
                {
                    SetInputColumnUsage(0, inputColumnName, DTSUsageType.UT_READONLY, false);
                    Instance.SetInputColumnProperty(Component.InputCollection[0].ID, Component.InputCollection[0].InputColumnCollection[inputColumnName].ID, "InputColumnType", 0);
                }
            }

            foreach (AstTermLookupColumnNode lookupColumn in _astTermLookupNode.InputColumns)
            {
                int inputColumnUsageType;
                switch (lookupColumn.InputColumnUsageType)
                {
                    case AstTermLookupInputUsageType.DoNotPassthrough:
                        inputColumnUsageType = -1;
                        break;
                    case AstTermLookupInputUsageType.Passthrough:
                        inputColumnUsageType = 0;
                        break;
                    case AstTermLookupInputUsageType.Lookup:
                        inputColumnUsageType = 1;
                        break;
                    case AstTermLookupInputUsageType.LookupAndPassthrough:
                        inputColumnUsageType = 2;
                        break;
                    default:
                        throw new NotImplementedException(String.Format(CultureInfo.InvariantCulture, Properties.Resources.ErrorEnumerationTypeNotSupported, lookupColumn.InputColumnUsageType));
                }

                if (inputColumnUsageType >= 0)
                {
                    IDTSInputColumn100 input = SetInputColumnUsage(0, lookupColumn.Name, DTSUsageType.UT_READONLY, false);
                    Instance.SetInputColumnProperty(
                        Component.InputCollection[0].ID,
                        Component.InputCollection[0].InputColumnCollection[lookupColumn.Name].ID,
                        "InputColumnType",
                        inputColumnUsageType);
                    
                    // 1 means Lookup only
                    // 2 means Lookup + Passthrough                    
                    if (inputColumnUsageType == 0 || inputColumnUsageType == 2)
                    {
                        IDTSOutputColumn100 output = Component.OutputCollection[0].OutputColumnCollection[input.Name];
                        output.CustomPropertyCollection["CustomLineageID"].Value = input.LineageID;
                        if (!String.IsNullOrEmpty(lookupColumn.OutputAs))
                        {
                            output.Name = lookupColumn.OutputAs;
                        }
                    }
                }
                else
                {
                    SetInputColumnUsage(0, lookupColumn.Name, DTSUsageType.UT_IGNORED, true);
                }
            }
        }
    }
}