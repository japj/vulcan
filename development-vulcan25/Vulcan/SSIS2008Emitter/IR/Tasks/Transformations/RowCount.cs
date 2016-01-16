using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.Phases.Lowering.Framework;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Transformation;

namespace Ssis2008Emitter.IR.Tasks.Transformations
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public class RowCount : SingleOutTransformation
    {
        private readonly AstRowCountNode _astRowCountNode;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        public static void CreateAndRegister(AstNode astNode, LoweringContext context)
        {
            context.ParentObject.Children.Add(new RowCount(context, astNode));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Required for Emitter pattern.")]
        public RowCount(LoweringContext context, AstNode astNode)
            : base(context, astNode as AstTransformationNode)
        {
            _astRowCountNode = astNode as AstRowCountNode;
            RegisterInputBinding(_astRowCountNode);
        }

        public override string Moniker
        {
            get { return "DTSTransform.RowCount"; }
        }

        public override void Initialize(SsisEmitterContext context)
        {
            base.Initialize(context);
            var v = new Framework.Variable(_astRowCountNode.Variable);
            v.Initialize(context);
            Component.CustomPropertyCollection["VariableName"].Value = v.DtsVariable.QualifiedName;

            ProcessBindings(context);
            Flush();
        }

        public override void Emit(SsisEmitterContext context)
        {
        }
    }
}