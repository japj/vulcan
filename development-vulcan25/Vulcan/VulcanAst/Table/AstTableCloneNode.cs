using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using AstFramework.Model;
using Vulcan.Utility.ComponentModel;

namespace VulcanEngine.IR.Ast.Table
{
    public partial class AstTableCloneNode
    {
        private readonly Dictionary<AstTableColumnBaseNode, AstTableColumnBaseNode> _baseTableColumnsToCloneColumns;

        public AstTableCloneNode(IFrameworkItem parentItem)
            : base(parentItem)
        {
            _baseTableColumnsToCloneColumns = new Dictionary<AstTableColumnBaseNode, AstTableColumnBaseNode>();

            InitializeAstNode();
            SingletonPropertyChanged += AstTableNode_SingletonPropertyChanged;
        }

        private void AstTableNode_SingletonPropertyChanged(object sender, VulcanPropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Table")
            {
                if (e.OldValue != null)
                {
                    ((AstTableNode)e.OldValue).Columns.CollectionChanged -= TableColumns_CollectionChanged;
                    OnClearBaseTableColumns();
                }

                if (Table != null)
                {
                    foreach (var column in Table.Columns)
                    {
                        OnAddBaseTableColumn(column);
                    }

                    Table.Columns.CollectionChanged += TableColumns_CollectionChanged;
                }
            }
        }

        private void TableColumns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Debug.Assert(e.Action == NotifyCollectionChangedAction.Reset || e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove, "Unsupported collection change type.");
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                OnClearBaseTableColumns();
            }

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (AstTableColumnBaseNode item in e.NewItems)
                {
                    OnAddBaseTableColumn(item);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (AstTableColumnBaseNode item in e.OldItems)
                {
                    OnRemoveBaseTableColumn(item);
                }
            }
        }

        protected void OnAddBaseTableColumn(AstTableColumnBaseNode column)
        {
            if (!(column is AstTableHashedKeyColumnNode))
            {   
                var clonedColumn = (AstTableColumnBaseNode)column.Clone(this);
                if (NullClonedColumns)
                {
                    clonedColumn.IsNullable = true;
                }
                else
                {
                    clonedColumn.IsNullable = column.IsNullable;
                }

                Columns.Add(clonedColumn);
                _baseTableColumnsToCloneColumns.Add(column, clonedColumn);
            }
        }

        protected void OnRemoveBaseTableColumn(AstTableColumnBaseNode column)
        {
            if (_baseTableColumnsToCloneColumns.ContainsKey(column))
            {
                Columns.Remove(_baseTableColumnsToCloneColumns[column]);
                _baseTableColumnsToCloneColumns.Remove(column);
            }
        }

        protected void OnClearBaseTableColumns()
        {
            foreach (var columnPair in _baseTableColumnsToCloneColumns)
            {
                Columns.Remove(columnPair.Value);
            }

            _baseTableColumnsToCloneColumns.Clear();
        }
    }
}

