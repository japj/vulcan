using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using AstFramework.Dataflow;
using AstFramework.Engine;
using AstFramework.Model;
using Vulcan.Utility.Collections;
using Vulcan.Utility.ComponentModel;
using AstFramework;
using AstFramework.Engine.Binding;
using AstFramework.Markup;
using VulcanEngine.AstFramework;
using System.Reflection;
using System.Collections.Generic;
using Vulcan.Utility.Xml;
using Vulcan.Utility.Common;
using Vulcan.Utility.Markup;

namespace VulcanEngine.IR.Ast.Table
{
    public partial class AstTableNode : IDataflowItem
    {
        private VulcanCollection<AstNamedNode> _dataItems;

        public AstTableNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            _dataItems = new VulcanCollection<AstNamedNode>();
            EmitVersionNumber = true;
            InitializeAstNode();

            CollectionPropertyChanged += AstTableNode_CollectionPropertyChanged;
        }

        private void AstTableNode_CollectionPropertyChanged(object sender, VulcanCollectionPropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Sources" || e.PropertyName == "Lookups")
            {
                VulcanCompositeCollectionChanged(_dataItems, e);
            }

            if (e.PropertyName == "Columns" && SideEffectManager.SideEffectMode == AstSideEffectMode.ConsistencySideEffects)
            {
                CollectionActionSyncToStaticSource(e);
            }
        }

        private void CollectionActionSyncToStaticSource(VulcanCollectionPropertyChangedEventArgs e)
        {
            foreach (AstTableStaticSourceNode staticSource in Sources)
            {
                if (e.Action == NotifyCollectionChangedAction.Remove
                    || e.Action == NotifyCollectionChangedAction.Replace
                    || e.Action == NotifyCollectionChangedAction.Reset)
                {
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        foreach (var row in staticSource.Rows)
                        {
                            // Column will be null since the table column will have been undefined in its StaticSourceColumnValueNode object.
                            // Thus, search for the StaticSourceColumnValueNode whose Column is null.
                            AstStaticSourceColumnValueNode columnValueNode = row.ColumnValues.FirstOrDefault(columnValue => columnValue.Column == null);
                            row.ColumnValues.Remove(columnValueNode);
                        }
                    }
                }

                if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
                {
                    foreach (AstTableColumnBaseNode newItem in e.NewItems)
                    {
                        foreach (var row in staticSource.Rows)
                        {
                            row.ColumnValues.Add(new AstStaticSourceColumnValueNode(row) { Column = newItem, Value = newItem.DefaultValue });
                        }
                    }
                }
            }
        }

        [VulcanCategory("Optional")]
        [VulcanDefaultValue(true)]
        [VulcanDescription(@"")]
        [Browsable(true)]
        [AstXNameBinding("EmitVersionNumber", ChildType.Attribute, DefaultValue = "true")]
        public System.Boolean EmitVersionNumber
        {
            get;
            set;
        }

        [Browsable(false)]
        public bool HasIdentityKey
        {
            get { return Keys.Any(item => item is AstTableIdentityNode); }
        }

        [Browsable(false)]
        public string SchemaQualifiedName
        {
            get
            {
                if (Schema != null)
                {
                    return String.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", Schema.Name, Name);
                }

                return String.Format(CultureInfo.InvariantCulture, "[{0}]", Name);
            }
        }

        [Browsable(false)]
        public string DataSourceViewQualifiedName
        {
            get
            {
                string schemaName = (this.Schema == null) ? string.Empty : this.Schema.Name;
                string tableName = (this.Schema == null) ? this.Name : String.Format(CultureInfo.InvariantCulture, "{0}_{1}", schemaName, this.Name);
                return tableName;
            }
        }

        // TODO: Should we wire this up for property changed notifications?
        [BrowsableAttribute(false)]
        public bool HasScdColumns
        {
            get
            {
                foreach (var column in Columns)
                {
                    if (column.ScdType == ScdType.Historical)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        [BrowsableAttribute(false)]
        public VulcanCollection<AstNamedNode> DataItems
        {
            get { return _dataItems; }
        }

        [BrowsableAttribute(false)]
        public AstTableKeyBaseNode PreferredKey
        {
            get
            {
                AstTableKeyBaseNode preferredKey = null;
                foreach (AstTableKeyBaseNode candidate in this.Keys)
                {
                    if (candidate.Clustered)
                    {
                        return candidate;
                    }

                    if (preferredKey == null)
                    {
                        preferredKey = candidate;
                    }

                    if (candidate.ComparisonBytes < preferredKey.ComparisonBytes)
                    {
                        preferredKey = candidate;
                    }
                }

                return preferredKey;
            }
        }

        [BrowsableAttribute(false)]
        public VulcanCollection<AstTableColumnBaseNode> ReadOnlyColumns
        {
            get
            {
                var readOnlyColumns = new VulcanCollection<AstTableColumnBaseNode>();
                foreach (var column in Columns)
                {
                    if (!column.IsAssignable)
                    {
                        readOnlyColumns.Add(column);
                    }
                }

                return readOnlyColumns;
            }
        }
    }
}
