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
    [FriendlyNameAttribute("MergeTask")]
    [AstSchemaTypeBindingAttribute("MergeTaskElemType", "http://tempuri.org/vulcan2.xsd")]
    public partial class AstMergeTaskNode : VulcanEngine.IR.Ast.Task.AstTaskNode
    {
        #region Private Storage
        private VulcanEngine.IR.Ast.Table.AstTableNode _sourceTable;
        private bool __isNotFirstSet__sourceTable;

        private VulcanEngine.IR.Ast.Table.AstTableKeyBaseNode _targetConstraint;
        private bool __isNotFirstSet__targetConstraint;

        private VulcanCollection<VulcanEngine.IR.Ast.Task.AstMergeColumnNode> _columns;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected System.Boolean DisableScd_BackingField;
        private bool _isDisableScdExplicitlySet;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected VulcanEngine.IR.Ast.Task.MergeColumnUsage UnspecifiedColumnDefaultUsageType_BackingField;
        private bool __isNotFirstSet_UnspecifiedColumnDefaultUsageType_BackingField;
        private bool _isUnspecifiedColumnDefaultUsageTypeExplicitlySet;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected System.Boolean TableLock_BackingField;
        private bool _isTableLockExplicitlySet;

        #endregion Private Storage

        #region Public Accessor Properties
        [VulcanCategory("Required")]
        [AstRequiredProperty]
        [VulcanDescription(@"")]
        [Browsable(true)]
        [AstXNameBinding("SourceTableName", ChildType.Attribute)]
        public VulcanEngine.IR.Ast.Table.AstTableNode SourceTable
        {
            get { return _sourceTable; }
            set
            {
                if (_sourceTable != value || !__isNotFirstSet__sourceTable)
                {
                    __isNotFirstSet__sourceTable = true;
                    if (_sourceTable != null)
                    {
                        _sourceTable.Unreference(this, "SourceTable");
                    }
                    VulcanEngine.IR.Ast.Table.AstTableNode oldValue = _sourceTable;
                    _sourceTable = value;
                    if (_sourceTable != null)
                    {
                        _sourceTable.Reference(this, "SourceTable");
                    }
                    VulcanOnPropertyChanged("SourceTable", oldValue, _sourceTable);
                }
            }
        }


        [VulcanCategory("Required")]
        [AstRequiredProperty]
        [VulcanDescription(@"")]
        [Browsable(true)]
        [AstXNameBinding("TargetConstraintName", ChildType.Attribute)]
        public VulcanEngine.IR.Ast.Table.AstTableKeyBaseNode TargetConstraint
        {
            get { return _targetConstraint; }
            set
            {
                if (_targetConstraint != value || !__isNotFirstSet__targetConstraint)
                {
                    __isNotFirstSet__targetConstraint = true;
                    if (_targetConstraint != null)
                    {
                        _targetConstraint.Unreference(this, "TargetConstraint");
                    }
                    VulcanEngine.IR.Ast.Table.AstTableKeyBaseNode oldValue = _targetConstraint;
                    _targetConstraint = value;
                    if (_targetConstraint != null)
                    {
                        _targetConstraint.Reference(this, "TargetConstraint");
                    }
                    VulcanOnPropertyChanged("TargetConstraint", oldValue, _targetConstraint);
                }
            }
        }


        [VulcanCategory("Read Only")]
        [VulcanDescription(@"")]
        [Browsable(false)]
        [AstXNameBinding("Column", ChildType.ListChildDefinition)]
        public VulcanCollection<VulcanEngine.IR.Ast.Task.AstMergeColumnNode> Columns
        {
            get { return _columns; }
        }


        [VulcanCategory("Optional")]
        [VulcanDescription(@"")]
        [Browsable(true)]
        [AstXNameBinding("DisableScd", ChildType.Attribute)]
        public System.Boolean DisableScd
        {
            get { return DisableScd_BackingField; }
            set
            {
                if (DisableScd_BackingField != value)
                {
                    System.Boolean oldValue = DisableScd_BackingField;
                    DisableScd_BackingField = value;
                    VulcanOnPropertyChanged("DisableScd", oldValue, DisableScd_BackingField);
                    IsDisableScdExplicitlySet = true;
                }
            }
        }

        public bool IsDisableScdExplicitlySet
        {
            get { return _isDisableScdExplicitlySet; }
            protected set
            {
                if (_isDisableScdExplicitlySet != value)
                {
                    bool oldValue = _isDisableScdExplicitlySet;
                    _isDisableScdExplicitlySet = value;
                    VulcanOnPropertyChanged("IsDisableScdExplicitlySet", oldValue, _isDisableScdExplicitlySet);
                }
            }
        }


        [VulcanCategory("Optional")]
        [AstRequiredProperty]
        [Browsable(true)]
        [AstXNameBinding("UnspecifiedColumnDefaultUsageType", ChildType.Attribute)]
        public VulcanEngine.IR.Ast.Task.MergeColumnUsage UnspecifiedColumnDefaultUsageType
        {
            get { return UnspecifiedColumnDefaultUsageType_BackingField; }
            set
            {
                if (UnspecifiedColumnDefaultUsageType_BackingField != value || !__isNotFirstSet_UnspecifiedColumnDefaultUsageType_BackingField)
                {
                    __isNotFirstSet_UnspecifiedColumnDefaultUsageType_BackingField = true;
                    VulcanEngine.IR.Ast.Task.MergeColumnUsage oldValue = UnspecifiedColumnDefaultUsageType_BackingField;
                    UnspecifiedColumnDefaultUsageType_BackingField = value;
                    VulcanOnPropertyChanged("UnspecifiedColumnDefaultUsageType", oldValue, UnspecifiedColumnDefaultUsageType_BackingField);
                    IsUnspecifiedColumnDefaultUsageTypeExplicitlySet = true;
                }
            }
        }

        public bool IsUnspecifiedColumnDefaultUsageTypeExplicitlySet
        {
            get { return _isUnspecifiedColumnDefaultUsageTypeExplicitlySet; }
            protected set
            {
                if (_isUnspecifiedColumnDefaultUsageTypeExplicitlySet != value)
                {
                    bool oldValue = _isUnspecifiedColumnDefaultUsageTypeExplicitlySet;
                    _isUnspecifiedColumnDefaultUsageTypeExplicitlySet = value;
                    VulcanOnPropertyChanged("IsUnspecifiedColumnDefaultUsageTypeExplicitlySet", oldValue, _isUnspecifiedColumnDefaultUsageTypeExplicitlySet);
                }
            }
        }


        [VulcanCategory("Optional")]
        [VulcanDefaultValue(false)]
        [Browsable(true)]
        [AstXNameBinding("TableLock", ChildType.Attribute, DefaultValue = "false")]
        public System.Boolean TableLock
        {
            get { return TableLock_BackingField; }
            set
            {
                if (TableLock_BackingField != value)
                {
                    System.Boolean oldValue = TableLock_BackingField;
                    TableLock_BackingField = value;
                    VulcanOnPropertyChanged("TableLock", oldValue, TableLock_BackingField);
                    IsTableLockExplicitlySet = true;
                }
            }
        }

        public bool IsTableLockExplicitlySet
        {
            get { return _isTableLockExplicitlySet; }
            protected set
            {
                if (_isTableLockExplicitlySet != value)
                {
                    bool oldValue = _isTableLockExplicitlySet;
                    _isTableLockExplicitlySet = value;
                    VulcanOnPropertyChanged("IsTableLockExplicitlySet", oldValue, _isTableLockExplicitlySet);
                }
            }
        }


        #endregion Public Accessor Properties

        #region Collection Methods


        private void _columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            VulcanOnCollectionPropertyChanged("Columns", e);
        }




        #endregion Collection Methods

        #region Initialization
        private void InitializeAstNode()
        {
            _columns = new VulcanCollection<VulcanEngine.IR.Ast.Task.AstMergeColumnNode>();
            _columns.CollectionChanged += _columns_CollectionChanged;
            TableLock_BackingField = false;
        }

        #endregion Initialization

        public override VulcanCollection<IFrameworkItem> DefinedAstNodes()
        {
            var definedAstNodes = new VulcanCollection<IFrameworkItem>();
            definedAstNodes.AddRange(base.DefinedAstNodes());


            foreach (var item in _columns)
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
            if (targetItem == null || !typeof(AstMergeTaskNode).IsAssignableFrom(targetItem.GetType()))
            {
                throw new ArgumentException("Provided target node is not of the correct type.");
            }
            var castedTargetItem = (AstMergeTaskNode)targetItem;
            if (_sourceTable == null)
            {
                castedTargetItem._sourceTable = null;
            }
            else if (cloneMapping.ContainsKey(_sourceTable))
            {
                castedTargetItem._sourceTable = (VulcanEngine.IR.Ast.Table.AstTableNode)cloneMapping[_sourceTable];
            }
            else
            {
                castedTargetItem._sourceTable = _sourceTable;
            }

            if (_targetConstraint == null)
            {
                castedTargetItem._targetConstraint = null;
            }
            else if (cloneMapping.ContainsKey(_targetConstraint))
            {
                castedTargetItem._targetConstraint = (VulcanEngine.IR.Ast.Table.AstTableKeyBaseNode)cloneMapping[_targetConstraint];
            }
            else
            {
                castedTargetItem._targetConstraint = _targetConstraint;
            }

            foreach (var item in _columns)
            {
                IFrameworkItem candidate = cloneMapping[item];
                castedTargetItem._columns.Add((VulcanEngine.IR.Ast.Task.AstMergeColumnNode)candidate);
                item.CloneInto(candidate, cloneMapping);
            }

            castedTargetItem.DisableScd_BackingField = DisableScd_BackingField;

            castedTargetItem.UnspecifiedColumnDefaultUsageType_BackingField = UnspecifiedColumnDefaultUsageType_BackingField;

            castedTargetItem.TableLock_BackingField = TableLock_BackingField;

        }

        public override IFrameworkItem CloneHusk(IFrameworkItem parentItem)
        {
            return new AstMergeTaskNode(parentItem);
        }

        #endregion Cloning Support


    }
}
