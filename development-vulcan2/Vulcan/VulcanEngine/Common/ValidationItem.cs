using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VulcanEngine.IR.Ast;

namespace VulcanEngine.Common
{
    public class ValidationItem
    {
        private string _message;
        private string _recommendation;
        private Severity _severity;
        private AstNode _astNode;

        public ValidationItem(Severity severity, string recommendation, AstNode astNode, string message, params object[] formatParmeters)
        {
            _severity = severity;
            _recommendation = recommendation;
            _astNode = astNode;

            this._message = String.Format("{0}: {1}: {2}", severity, String.Format(message, formatParmeters));
        }

        public string Message
        {
            get { return this._message; }
        }

        public string Recommendation
        {
            get { return _recommendation; }
        }

        public Severity Severity
        {
            get { return this._severity; }
        }

        public AstNode AstNode
        {
            get { return _astNode; }
        }

        public override string ToString()
        {
            return _message;
        }
    }
}
