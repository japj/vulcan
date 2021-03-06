//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Varigence Ast Designer tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.  Do not edit this file unless you know exactly
//     you are doing and are synchronized with the Vulcan development team.
//
//     For more information about the Varigence Ast Designer tool, email
//     support@varigence.com.
//
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Collections.Specialized;
using Vulcan.Utility.ComponentModel;
using AstFramework;
using AstFramework.Engine.Binding;
using AstFramework.Model;
using AstFramework.Markup;
using VulcanEngine.AstFramework;
using Vulcan.Utility.Collections;
using System.Reflection;
using System.Collections.Generic;
using Vulcan.Utility.Xml;
using Vulcan.Utility.Common;
using Vulcan.Utility.Markup;

namespace VulcanEngine.IR.Ast
{
    [FriendlyNameAttribute("TemplateArgumentMapping")]
    [AstSchemaTypeBindingAttribute("", "http://tempuri.org/vulcan2.xsd")]
    public partial class AstTemplateArgumentMappingNode : VulcanEngine.IR.Ast.AstNode
    {
        #region Private Storage
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected System.String ArgumentName_BackingField;
        private bool _isArgumentNameExplicitlySet;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected System.String Value_BackingField;
        private bool _isValueExplicitlySet;

        #endregion Private Storage

        #region Public Accessor Properties
        [Browsable(true)]
        [AstXNameBinding("ArgumentName", ChildType.Attribute)]
        public System.String ArgumentName
        {
            get { return ArgumentName_BackingField; }
            set
            {
                if (ArgumentName_BackingField != value)
                {
                    System.String oldValue = ArgumentName_BackingField;
                    ArgumentName_BackingField = value;
                    VulcanOnPropertyChanged("ArgumentName", oldValue, ArgumentName_BackingField);
                    IsArgumentNameExplicitlySet = true;
                }
            }
        }

        public bool IsArgumentNameExplicitlySet
        {
            get { return _isArgumentNameExplicitlySet; }
            protected set
            {
                if (_isArgumentNameExplicitlySet != value)
                {
                    bool oldValue = _isArgumentNameExplicitlySet;
                    _isArgumentNameExplicitlySet = value;
                    VulcanOnPropertyChanged("IsArgumentNameExplicitlySet", oldValue, _isArgumentNameExplicitlySet);
                }
            }
        }


        [Browsable(true)]
        [AstXNameBinding("__self", ChildType.Self)]
        public System.String Value
        {
            get { return Value_BackingField; }
            set
            {
                if (Value_BackingField != value)
                {
                    System.String oldValue = Value_BackingField;
                    Value_BackingField = value;
                    VulcanOnPropertyChanged("Value", oldValue, Value_BackingField);
                    IsValueExplicitlySet = true;
                }
            }
        }

        public bool IsValueExplicitlySet
        {
            get { return _isValueExplicitlySet; }
            protected set
            {
                if (_isValueExplicitlySet != value)
                {
                    bool oldValue = _isValueExplicitlySet;
                    _isValueExplicitlySet = value;
                    VulcanOnPropertyChanged("IsValueExplicitlySet", oldValue, _isValueExplicitlySet);
                }
            }
        }


        #endregion Public Accessor Properties

        #region Collection Methods


        #endregion Collection Methods

        #region Initialization
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private void InitializeAstNode()
        {
        }

        #endregion Initialization

        public override VulcanCollection<IFrameworkItem> DefinedAstNodes()
        {
            var definedAstNodes = new VulcanCollection<IFrameworkItem>();
            definedAstNodes.AddRange(base.DefinedAstNodes());


            return definedAstNodes;
        }


        public override IEnumerable<IScopeBoundary> BindingScopeBoundaries()
        {
            var bindingScopeBoundaries = new List<IScopeBoundary>();
            bindingScopeBoundaries.Add(ScopeBoundary);
            return bindingScopeBoundaries;
        }


        #region Cloning Support
        public override IFrameworkItem Clone()
        {
            return Clone(ParentItem);
        }

        public override IFrameworkItem Clone(Dictionary<IFrameworkItem, IFrameworkItem> cloneMapping)
        {
            return Clone(ParentItem, cloneMapping);
        }

        public override IFrameworkItem Clone(IFrameworkItem parentItem)
        {
            return Clone(parentItem, new Dictionary<IFrameworkItem, IFrameworkItem>());
        }

        public override IFrameworkItem Clone(IFrameworkItem parentItem, Dictionary<IFrameworkItem, IFrameworkItem> cloneMapping)
        {
            SymbolTable.GetSourceToCloneDefinitionMappings(this, parentItem, cloneMapping);
            CloneInto(cloneMapping[this], cloneMapping);
            return cloneMapping[this];
        }

        public override void CloneInto(IFrameworkItem targetItem, Dictionary<IFrameworkItem, IFrameworkItem> cloneMapping)
        {
            base.CloneInto(targetItem, cloneMapping);
            if (targetItem == null || !typeof(AstTemplateArgumentMappingNode).IsAssignableFrom(targetItem.GetType()))
            {
                throw new ArgumentException("Provided target node is not of the correct type.");
            }
            var castedTargetItem = (AstTemplateArgumentMappingNode)targetItem;
            castedTargetItem.ArgumentName_BackingField = ArgumentName_BackingField;

            castedTargetItem.Value_BackingField = Value_BackingField;

        }

        public override IFrameworkItem CloneHusk(IFrameworkItem parentItem)
        {
            return new AstTemplateArgumentMappingNode(parentItem);
        }

        #endregion Cloning Support


    }
}
