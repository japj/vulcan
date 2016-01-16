using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using VulcanEngine.Common;

namespace VulcanEngine.IR.Ast.Dimension
{
    [AstSchemaTypeBindingAttribute("AttributeRelationshipElemType", "http://tempuri.org/vulcan2.xsd")]
    public class AstAttributeRelationshipNode : AstDimensionNamedBaseNode
    {
        #region Private Storage
        private bool _visible;
        private AstAttributeNode _parent;
        private AstAttributeNode _child;
        private RelationshipType _type;
        private CardinalityType _cardinality;
        private OptionalityType _optionality;
        #endregion   // Private Storage

        #region Public Accessor Properties
        public bool RequiredFieldsSet
        {
            get
            {
                return ((Parent != null) &&
                        (Child != null));
            }
        }

        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        [BrowsableAttribute(false)]
        public AstAttributeNode Parent
        {
            get { return _parent; }
            set
            {
                if (_parent != value)
                {
                    _parent = value;
                    VulcanOnPropertyChanged("Parent");
                }
            }
        }

        [BrowsableAttribute(false)]
        public AstAttributeNode Child
        {
            get { return _child; }
            set
            {
                if (_parent != value)
                {
                    _child = value;
                    VulcanOnPropertyChanged("Child");
                }
            }
        }

        public RelationshipType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public CardinalityType Cardinality
        {
            get { return _cardinality; }
            set { _cardinality = value; }
        }

        public OptionalityType Optionality
        {
            get { return _optionality; }
            set { _optionality = value; }
        }
        #endregion   // Public Accessor Properties

        #region Default Constructor
        public AstAttributeRelationshipNode() : base() { }
        #endregion   // Default Constructor

        #region Validation
        public override IList<ValidationItem> Validate()
        {
            List<ValidationItem> validationItems = new List<ValidationItem>();
            validationItems.AddRange(base.Validate());

            validationItems.AddRange(this.Parent.Validate());
            validationItems.AddRange(this.Child.Validate());

            return validationItems;
        }
        #endregion  // Validation
    }

    public enum CardinalityType
    {
        OneToOne,
        OneToMany,
    };

    public enum OptionalityType
    {
        Mandatory,
        Optional
    };

    public enum RelationshipType
    {
        Rigid,
        Flexible
    }
}
