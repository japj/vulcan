using System.Collections.Generic;
using System.Text;

using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.Framework;
using Ssis2008Emitter.IR.Framework.Connections;
using Ssis2008Emitter.Phases.Lowering.Framework;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Task;
using VulcanEngine.IR.Ast.Transformation;
using DTS = Microsoft.SqlServer.Dts.Runtime;

namespace Ssis2008Emitter.IR.Tasks.Transformations
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public class OleDBSource : SingleOutTransformation
    {
        private readonly AstQuerySourceNode _astQuerySourceNode;
        private readonly Dictionary<string, Variable> _paramDictionary;
        private OleDBConnection _oleDBConnection;

        public string SqlCommandVarName { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        public static void CreateAndRegister(AstNode astNode, LoweringContext context)
        {
            var astQuerySourceNode = astNode as AstQuerySourceNode;
            if (astQuerySourceNode != null)
            {
                var oleDBSource = new OleDBSource(context, astNode) { _oleDBConnection = new OleDBConnection(astQuerySourceNode.Connection) };

                // Note: vsabella: workaround for different behavior regarding expressions for OLEDB sources 
                // versus how it works in Lookups.
                if (astQuerySourceNode.Query.QueryType == QueryType.Expression)
                {
                    // Variable name has to be only alphanumeric, more restrictive than astQuerySource Name
                    string varName = "__" + System.Guid.NewGuid().ToString("N");
                    var variable = new Variable(varName);
                    oleDBSource.SqlCommandVarName = "User::" + varName;

                    variable.TypeCode = System.TypeCode.String;
                    variable.EvaluateAsExpression = true;
                    variable.ValueString = astQuerySourceNode.Query.Body;
                    context.ParentObject.Children.Add(variable);
                }
                else
                {
                    foreach (AstVariableParameterMappingNode paramNode in astQuerySourceNode.Query.Parameters)
                    {
                        var variable = new Variable(paramNode.Variable);
                        oleDBSource._paramDictionary[paramNode.Name] = variable;
                        context.ParentObject.Children.Add(variable);
                    }
                }

                context.ParentObject.Children.Add(oleDBSource._oleDBConnection);
                context.ParentObject.Children.Add(oleDBSource);
            }
        }

        public OleDBSource(LoweringContext context, AstNode astNode) : base(context, astNode as AstTransformationNode)
        {
            _paramDictionary = new Dictionary<string, Variable>();
            _astQuerySourceNode = astNode as AstQuerySourceNode;
        }

        public override string Moniker
        {
            get { return "DTSAdapter.OleDBSource"; }
        }

        public override void Initialize(SsisEmitterContext context)
        {
            base.Initialize(context);
            TransformationUtility.RegisterOleDBConnection(context, _oleDBConnection, Component);
        }

        public override void Emit(SsisEmitterContext context)
        {
            if (_astQuerySourceNode.Query.QueryType == QueryType.Expression)
            {
                Instance.SetComponentProperty("AccessMode", 3);
                Instance.SetComponentProperty("SqlCommandVariable", SqlCommandVarName);
            }
            else
            {
                Instance.SetComponentProperty("AccessMode", 2);
                Instance.SetComponentProperty("SqlCommand", _astQuerySourceNode.Query.Body.Trim());
            }

            Flush();

            var paramBuilder = new StringBuilder();
            foreach (string index in _paramDictionary.Keys)
            {
                paramBuilder.AppendFormat("\"{0}\",{1};", index, _paramDictionary[index].DtsVariable.ID);
            }

            Instance.SetComponentProperty("ParameterMapping", paramBuilder.ToString());
        }
    }
}

