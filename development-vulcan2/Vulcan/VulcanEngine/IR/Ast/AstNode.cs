using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.ComponentModel;
using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast
{
    public abstract class AstNode : VulcanNotifyPropertyChanged
    {
        // Private Storage
        private AstNode _parentASTNode;
        private XObject _boundXElement;
        private List<XObject> _unmappedXObjects;

        [BrowsableAttribute(false)]
        public virtual string ReferenceableName
        {
            get
            {
                if (this.ParentASTNode != null)
                {
                    return this.ParentASTNode.ReferenceableName;
                }
                else
                    return null;
            }
        }

        [BrowsableAttribute(false)]
        public AstNode ParentASTNode
        {
            get { return _parentASTNode; }
            set { _parentASTNode = value; }
        }

        [BrowsableAttribute(false)]
        public XObject BoundXElement
        {
            get { return this._boundXElement; }
            set { this._boundXElement = value; }
        }

        [BrowsableAttribute(false)]
        public List<XObject> UnmappedXObjects
        {
            get { return this._unmappedXObjects; }
        }

        public AstNode()
        {
            this._unmappedXObjects = new List<XObject>();
        }

        public virtual IList<ValidationItem> Validate()
        {
            return new List<ValidationItem>();
        }
    }
}
