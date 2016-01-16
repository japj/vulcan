using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VulcanEngine.IR.Ast
{
    public class AstWalker
    {
        public static AstNodeType FirstParent<AstNodeType>(AstNode astNode) where AstNodeType : AstNode
        {
            AstNode currentNode = astNode;
            while (currentNode != null)
            {
                AstNodeType castedNode = currentNode as AstNodeType;
                if (castedNode != null)
                {
                    return castedNode;
                }
                currentNode = currentNode.ParentASTNode;
            }
            return null;
        }
    }
}
