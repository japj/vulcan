using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.Properties;
using VulcanEngine.Common;
using VulcanEngine.IR.Ast;

namespace VulcanEngine.Phases.Parser
{
    public class AstParserScopeManager
    {
        Stack<AstNode> _PathTracker;
        Stack<AstNode> _ScopeTracker;

        public bool IsEmpty
        {
            get { return _ScopeTracker.Count == 0; }
        }

        public AstParserScopeManager Clone()
        {
            AstParserScopeManager clone = new AstParserScopeManager();
            clone._PathTracker = new Stack<AstNode>(_PathTracker);
            clone._ScopeTracker = new Stack<AstNode>(_ScopeTracker);
            return clone;
        }
            

        public AstParserScopeManager()
        {
            this._PathTracker = new Stack<AstNode>();
            this._ScopeTracker = new Stack<AstNode>();
        }

        private bool IsScopeBoundary(AstNode astNode)
        {
            return (astNode.GetType().GetCustomAttributes(typeof(AstScopeBoundaryAttribute), true).Length > 0);
        }
        public void Push(AstNode astNode)
        {
            if (IsScopeBoundary(astNode))
            {
                this._ScopeTracker.Push(astNode);
            }
            this._PathTracker.Push(astNode);
        }

        public void Pop()
        {
            AstNode astNode = this._PathTracker.Pop();
            if (IsScopeBoundary(astNode))
            {
                this._ScopeTracker.Pop();
            }
        }

        public string FullPath
        {
            get
            {
                return StackStringBuilder(this._PathTracker);
            }
        }

        public string ScopedPath
        {
            get
            {
                return StackStringBuilder(this._ScopeTracker);
            }
        }

        public string GetScopedName(string name)
        {
            if (ScopedPath == null || ScopedPath.Equals(String.Empty))
            {
                return name;
            }
            else 
            {
                return String.Format("{0}.{1}", ScopedPath, name);
            }
        }

        private string StackStringBuilder(Stack<AstNode> queue)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (AstNode astNode in _ScopeTracker)
            {
                AstNamedNode astNamedNode = astNode as AstNamedNode;
                if (astNamedNode != null)
                {
                    sb.Append(astNamedNode.Name);
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sb.Append(".");
                    }
                }
            }
            return sb.ToString();
        }
    }
}
