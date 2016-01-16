using System;
using System.Globalization;
using AstFramework;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.Framework.Connections;
using Ssis2008Emitter.Phases.Lowering.Framework;
using VulcanEngine.Common;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Task;
using VulcanEngine.IR.Ast.Transformation;
using DTS = Microsoft.SqlServer.Dts.Runtime;

namespace Ssis2008Emitter.IR.Tasks.Transformations
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public class Lookup : Transformation
    {
        private readonly AstLookupNode _astLookupNode;
        private OleDBConnection _oleDBConnection;
        private string _parameterMap;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        public static void CreateAndRegister(AstNode astNode, LoweringContext context)
        {
            var astLookupNode = astNode as AstLookupNode;
            if (astLookupNode != null)
            {
                var lookup = new Lookup(context, astNode) { _oleDBConnection = new OleDBConnection(astLookupNode.Connection) };
                context.ParentObject.Children.Add(lookup._oleDBConnection);
                context.ParentObject.Children.Add(lookup);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Required for Emitter pattern.")]
        public Lookup(LoweringContext context, AstNode astNode)
            : base(context, astNode as AstTransformationNode)
        {
            _astLookupNode = astNode as AstLookupNode;
            RegisterInputBinding(_astLookupNode);
        }

        public override string Moniker
        {
            get { return "DTSTransform.Lookup"; }
        }

        public override void Initialize(SsisEmitterContext context)
        {
            base.Initialize(context);
            TransformationUtility.RegisterOleDBConnection(context, _oleDBConnection, Component);
        }

        private static DTSRowDisposition ConvertErrorRowDisposition(ErrorRowDisposition errorRowDisposition)
        {
            switch (errorRowDisposition)
            {
                case ErrorRowDisposition.IgnoreFailure: return DTSRowDisposition.RD_IgnoreFailure;
                case ErrorRowDisposition.RedirectRow: return DTSRowDisposition.RD_RedirectRow;
                case ErrorRowDisposition.FailComponent: return DTSRowDisposition.RD_FailComponent;
                case ErrorRowDisposition.NotUsed: return DTSRowDisposition.RD_NotUsed;
            }

            return DTSRowDisposition.RD_IgnoreFailure;
        }

        public override void Emit(SsisEmitterContext context)
        {
            // select * from (select FactIncidentID, CaseNumber  from FactIncident) [refTable]
            // where [refTable].[FactIncidentID] = ?
            // Enable Cache and No-Cache Modes

            ////switch (_astLookupNode.CacheMode)
            ////{
            ////    case AstLookupCacheMode.Full: // Keep default
            ////        Instance.SetComponentProperty("CacheType", 0);
            ////        break;
            ////    case AstLookupCacheMode.Partial:
            ////        Instance.SetComponentProperty("CacheType", 1);
            ////        break;
            ////    case AstLookupCacheMode.None:
            ////        Instance.SetComponentProperty("CacheType", 2);
            ////        break;
            ////    default: // do nothing and keep default
            ////        break;
            ////}

            ////if (_astLookupNode.CacheMode != AstLookupCacheMode.Full)
            ////{
            ////    var sqlCommandParamPropertyName = string.Format("[{0}].[SqlCommandParam]", Component.Name);
            ////    var sqlCommandParamBody = "";
            ////    this.Dataflow.SetExpression(sqlCommandParamPropertyName, sqlCommandParamBody);

            ////    Instance.SetComponentProperty("SqlCommandParam",
            ////        SSISUtilities.EvaluateSSISExpression(sqlCommandParamBody, context.ParentContainer.DtsContainer)
            ////        );
            ////}

            if (_astLookupNode.Query.QueryType == QueryType.Expression)
            {
                var sqlCommandPropertyName = string.Format(CultureInfo.InvariantCulture, "[{0}].[SqlCommand]", Component.Name);
                Dataflow.SetExpression(sqlCommandPropertyName, _astLookupNode.Query.Body);
                Instance.SetComponentProperty(
                    "SqlCommand",
                    SSISUtilities.EvaluateSSISExpression(_astLookupNode.Query.Body, context.ParentContainer.DtsContainer));
            }
            else
            {
                Instance.SetComponentProperty("SqlCommand", _astLookupNode.Query.Body);
            }

            if (_astLookupNode.EnableNoMatchOutputPath)
            {
                Instance.SetComponentProperty("NoMatchBehavior", 1);
            }
            else
            {
                foreach (IDTSOutput100 output in Component.OutputCollection)
                {
                    output.ErrorRowDisposition = ConvertErrorRowDisposition(_astLookupNode.DefaultErrorRowDisposition);
                }
            }

            Flush();
            ProcessBindings(context);
            foreach (var input in _astLookupNode.Inputs)
            {
                MapInput(input.LocalColumnName, input.RemoteColumnName);
            }

            Instance.SetComponentProperty("ParameterMap", _parameterMap);
            _parameterMap = string.Empty;

            foreach (var output in _astLookupNode.Outputs)
            {
                MapOutput(output.LocalColumnName, output.RemoteColumnName);
            }

            Flush();
        }

        protected void MapInput(string columnName, string referenceColumnName)
        {
            if (SetInputColumnUsage(0, columnName, DTSUsageType.UT_READONLY, false) != null)
            {
                Instance.SetInputColumnProperty(
                    Component.InputCollection[0].ID,
                    Component.InputCollection[0].InputColumnCollection[columnName].ID,
                    "JoinToReferenceColumn",
                    referenceColumnName);

                _parameterMap += String.Format(CultureInfo.InvariantCulture, "#{0};", Component.InputCollection[0].InputColumnCollection[columnName].LineageID);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Need general exception handling.  No risk of bad state.")]
        protected void MapOutput(string columnName, string referenceColumnName)
        {
            try
            {
                Instance.InsertOutputColumnAt(Component.OutputCollection[0].ID, Component.OutputCollection[0].OutputColumnCollection.Count, columnName, string.Empty);
                Instance.SetOutputColumnProperty(Component.OutputCollection[0].ID, Component.OutputCollection[0].OutputColumnCollection[columnName].ID, "CopyFromReferenceColumn", referenceColumnName);
            }
            catch (Exception)
            {
                MessageEngine.Trace(_astLookupNode, Severity.Error, "V0110", "Could not locate remote column {0}", referenceColumnName);
            }
        }
    }
}

