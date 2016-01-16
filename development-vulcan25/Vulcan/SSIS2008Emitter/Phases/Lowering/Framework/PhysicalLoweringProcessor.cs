using System;
using System.Collections.Generic;
using Ssis2008Emitter.IR;
using VulcanEngine.IR.Ast;

namespace Ssis2008Emitter.Phases.Lowering.Framework
{
    public delegate void AstLoweringHandler(AstNode astNode, LoweringContext context);

    public static class PhysicalLoweringProcessor
    {
        private static Dictionary<Type, IList<AstLoweringHandler>> _loweringDictionary = new Dictionary<Type, IList<AstLoweringHandler>>();

        public static void Register(Type astNodeType, AstLoweringHandler handler)
        {
            if (!_loweringDictionary.ContainsKey(astNodeType))
            {
                _loweringDictionary[astNodeType] = new List<AstLoweringHandler>();
            }

            _loweringDictionary[astNodeType].Add(handler);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Advance API for advanced developers.")]
        public static IDictionary<Type, IList<AstLoweringHandler>> LoweringDictionary
        {
            get { return _loweringDictionary; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        public static void Lower(AstNode node, LoweringContext context)
        {
            Type nodeType = node.GetType();
            if (LoweringDictionary.ContainsKey(nodeType))
            {
                foreach (AstLoweringHandler handler in LoweringDictionary[nodeType])
                {
                    handler(node, context);
                }
            }
        }
    }
}
