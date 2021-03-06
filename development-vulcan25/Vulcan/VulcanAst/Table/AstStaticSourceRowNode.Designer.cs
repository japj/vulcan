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

namespace VulcanEngine.IR.Ast.Table
{
    [FriendlyNameAttribute("Static Source Row")]
    [AstSchemaTypeBindingAttribute("AstStaticSourceRowElemType", "http://tempuri.org/vulcan2.xsd")]
    public partial class AstStaticSourceRowNode : VulcanEngine.IR.Ast.AstNode
    {
        #region Private Storage
        private VulcanCollection<VulcanEngine.IR.Ast.Table.AstStaticSourceColumnValueNode> _columnValues;

        #endregion Private Storage

        #region Public Accessor Properties
        [VulcanCategory("ReadOnly")]
        [AstRequiredProperty]
        [VulcanDescription(@"")]
        [Browsable(false)]
        [AstXNameBinding("ColumnValue", ChildType.ListChildDefinition)]
        public VulcanCollection<VulcanEngine.IR.Ast.Table.AstStaticSourceColumnValueNode> ColumnValues
        {
            get { return _columnValues; }
        }


        #endregion Public Accessor Properties

        #region Collection Methods
        private void _columnValues_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            VulcanOnCollectionPropertyChanged("ColumnValues", e);
        }

        #endregion Collection Methods

        #region Initialization
        private void InitializeAstNode()
        {
            _columnValues = new VulcanCollection<VulcanEngine.IR.Ast.Table.AstStaticSourceColumnValueNode>();
            _columnValues.CollectionChanged += _columnValues_CollectionChanged;
        }

        #endregion Initialization

        public override VulcanCollection<IFrameworkItem> DefinedAstNodes()
        {
            var definedAstNodes = new VulcanCollection<IFrameworkItem>();
            definedAstNodes.AddRange(base.DefinedAstNodes());
            foreach (var item in _columnValues)
            {
                definedAstNodes.Add(item);
            }

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
            if (targetItem == null || !typeof(AstStaticSourceRowNode).IsAssignableFrom(targetItem.GetType()))
            {
                throw new ArgumentException("Provided target node is not of the correct type.");
            }
            var castedTargetItem = (AstStaticSourceRowNode)targetItem;
            foreach (var item in _columnValues)
            {
                IFrameworkItem candidate = cloneMapping[item];
                castedTargetItem._columnValues.Add((VulcanEngine.IR.Ast.Table.AstStaticSourceColumnValueNode)candidate);
                item.CloneInto(candidate, cloneMapping);
            }

        }

        public override IFrameworkItem CloneHusk(IFrameworkItem parentItem)
        {
            return new AstStaticSourceRowNode(parentItem);
        }

        #endregion Cloning Support


    }
}
