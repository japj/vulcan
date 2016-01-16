using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using VulcanEngine.IR.Ast;

namespace Ssis2008Emitter
{
    public class SSISEmitterException : Exception
    {
        private AstNode _node;
        private string _errorMessage;


        public SSISEmitterException(AstNode node, Exception e) : base(string.Empty, e)
        {
            _node = node;

            if (node == null)
            {
                _errorMessage = base.Message;
            }
            else if (node is AstNamedNode)
            {
                SetMessages(((AstNamedNode)node).Name, GetTagName(node.BoundXElement));
            }
            else if (node.ReferenceableName != null)
            {
                SetMessages(node.ReferenceableName, GetTagName(node.BoundXElement));
                Source = node.ReferenceableName;
            }
            else if (node.ParentASTNode != null && node.ParentASTNode.ReferenceableName != null)
            {
                SetMessages(node.ParentASTNode.ReferenceableName, GetTagName(node.ParentASTNode.BoundXElement));
            }
            else
            {
                _errorMessage = base.Message;
            }
        }

        private void SetMessages(string name, string tagName)
        {
            _errorMessage = String.Format("[<{0}> \"{1}\"]", tagName, name);
        }

        private string GetTagName(XObject xObject)
        {
            string tagName = string.Empty;

            if (xObject is XElement)
            {
                tagName = ((XElement)xObject).Name.LocalName;
            }
            else if (xObject is XAttribute)
            {
                tagName = ((XAttribute)xObject).Name.LocalName;
            }

            return tagName;
        }

        public override string Message
        {
            get
            {
                return _errorMessage;
            }
        }
    }
}
