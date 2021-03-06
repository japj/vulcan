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

namespace VulcanEngine.IR.Ast.Task
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    [FriendlyNameAttribute("StagingContainer")]
    [AstSchemaTypeBindingAttribute("StagingTaskElemType", "http://tempuri.org/vulcan2.xsd")]
    public partial class AstStagingContainerTaskNode : VulcanEngine.IR.Ast.Task.AstContainerTaskNode
    {
        #region Private Storage
        private VulcanCollection<VulcanEngine.IR.Ast.Table.AstTableBaseNode> _tables;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected System.Boolean ExecuteDuringDesignTime_BackingField;
        private bool _isExecuteDuringDesignTimeExplicitlySet;

        #endregion Private Storage

        #region Public Accessor Properties
        [AstRequiredProperty]
        [Browsable(false)]
        [AstXNameBinding("Table", ChildType.ListChildDefinition, SubtypeOverride = typeof(VulcanEngine.IR.Ast.Table.AstTableNode))]
        [AstXNameBinding("CloneTable", ChildType.ListChildDefinition, SubtypeOverride = typeof(VulcanEngine.IR.Ast.Table.AstTableCloneNode))]
        [AstXNameBinding("TableTemplateInstance", ChildType.ListChildDefinition, SubtypeOverride = typeof(VulcanEngine.IR.Ast.Table.AstTableTemplateInstanceNode))]
        public VulcanCollection<VulcanEngine.IR.Ast.Table.AstTableBaseNode> Tables
        {
            get { return _tables; }
        }


        [VulcanDefaultValue(true)]
        [Browsable(true)]
        [AstXNameBinding("ExecuteDuringDesignTime", ChildType.Attribute, DefaultValue = "true")]
        public System.Boolean ExecuteDuringDesignTime
        {
            get { return ExecuteDuringDesignTime_BackingField; }
            set
            {
                if (ExecuteDuringDesignTime_BackingField != value)
                {
                    System.Boolean oldValue = ExecuteDuringDesignTime_BackingField;
                    ExecuteDuringDesignTime_BackingField = value;
                    VulcanOnPropertyChanged("ExecuteDuringDesignTime", oldValue, ExecuteDuringDesignTime_BackingField);
                    IsExecuteDuringDesignTimeExplicitlySet = true;
                }
            }
        }

        public bool IsExecuteDuringDesignTimeExplicitlySet
        {
            get { return _isExecuteDuringDesignTimeExplicitlySet; }
            protected set
            {
                if (_isExecuteDuringDesignTimeExplicitlySet != value)
                {
                    bool oldValue = _isExecuteDuringDesignTimeExplicitlySet;
                    _isExecuteDuringDesignTimeExplicitlySet = value;
                    VulcanOnPropertyChanged("IsExecuteDuringDesignTimeExplicitlySet", oldValue, _isExecuteDuringDesignTimeExplicitlySet);
                }
            }
        }


        #endregion Public Accessor Properties

        #region Collection Methods
        private void _tables_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            VulcanEngine.IR.Ast.Table.AstTableBaseNode.ProcessAstNamedNodeCollectionAction(e);
            VulcanOnCollectionPropertyChanged("Tables", e);
        }


        #endregion Collection Methods

        #region Initialization
        private void InitializeAstNode()
        {
            _tables = new VulcanCollection<VulcanEngine.IR.Ast.Table.AstTableBaseNode>();
            _tables.CollectionChanged += _tables_CollectionChanged;
            ExecuteDuringDesignTime_BackingField = true;
        }

        #endregion Initialization

        public override VulcanCollection<IFrameworkItem> DefinedAstNodes()
        {
            var definedAstNodes = new VulcanCollection<IFrameworkItem>();
            definedAstNodes.AddRange(base.DefinedAstNodes());
            foreach (var item in _tables)
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
            if (targetItem == null || !typeof(AstStagingContainerTaskNode).IsAssignableFrom(targetItem.GetType()))
            {
                throw new ArgumentException("Provided target node is not of the correct type.");
            }
            var castedTargetItem = (AstStagingContainerTaskNode)targetItem;
            foreach (var item in _tables)
            {
                IFrameworkItem candidate = cloneMapping[item];
                castedTargetItem._tables.Add((VulcanEngine.IR.Ast.Table.AstTableBaseNode)candidate);
                item.CloneInto(candidate, cloneMapping);
            }

            castedTargetItem.ExecuteDuringDesignTime_BackingField = ExecuteDuringDesignTime_BackingField;

        }

        public override IFrameworkItem CloneHusk(IFrameworkItem parentItem)
        {
            return new AstStagingContainerTaskNode(parentItem);
        }

        #endregion Cloning Support


    }
}
