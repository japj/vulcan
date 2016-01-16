using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.Phases.Lowering.Framework;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Transformation;

namespace Ssis2008Emitter.IR.Tasks.Transformations
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public class Multicast : Transformation
    {
        private readonly AstMulticastNode _astMulticastNode;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        public static void CreateAndRegister(AstNode astNode, LoweringContext context)
        {
            context.ParentObject.Children.Add((new Multicast(context, astNode)));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Required for Emitter pattern.")]
        public Multicast(LoweringContext context, AstNode astNode)
            : base(context, astNode as AstTransformationNode)
        {
            _astMulticastNode = astNode as AstMulticastNode;
            RegisterInputBinding(_astMulticastNode);
        }

        public override string Moniker
        {
            get { return "DTSTransform.Multicast"; }
        }

        public override void Initialize(SsisEmitterContext context)
        {
            base.Initialize(context);
            Flush();
        }

        public override void Emit(SsisEmitterContext context)
        {
            ProcessBindings(context);
        }
    }
}

