using System.ComponentModel;
using AstFramework.Dataflow;
using AstFramework.Model;
using Vulcan.Utility.Collections;

namespace VulcanEngine.IR.Ast.Dimension
{
    public partial class AstAttributeNode : IDataflowItem
    {
        private VulcanCollection<AstAttributeColumnNode> _columns;

        public AstAttributeNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            _columns = new VulcanCollection<AstAttributeColumnNode>();

            InitializeAstNode();

            SingletonPropertyChanged += AstAttributeNode_SingletonPropertyChanged;
            CollectionPropertyChanged += AstAttributeNode_CollectionPropertyChanged;
        }

        private void AstAttributeNode_SingletonPropertyChanged(object sender, Vulcan.Utility.ComponentModel.VulcanPropertyChangedEventArgs e)
        {
            if (e.PropertyName == "NameColumn")
            {
                VulcanCompositeCollectionChanged(_columns, e.OldValue as AstAttributeColumnNode, e.NewValue as AstAttributeColumnNode);                
            }
            else if (e.PropertyName == "ValueColumn")
            {
                VulcanCompositeCollectionChanged(_columns, e.OldValue as AstAttributeColumnNode, e.NewValue as AstAttributeColumnNode);
            }
        }

        private void AstAttributeNode_CollectionPropertyChanged(object sender, VulcanCollectionPropertyChangedEventArgs e)
        {
            VulcanCompositeCollectionChanged(_columns, e);
        }

        [Browsable(false)]
        public VulcanCollection<AstAttributeColumnNode> Columns
        {
            get
            {
                _columns.Clear();

                if (_nameColumn != null)
                {
                    _columns.Add(NameColumn);
                }

                if (_valueColumn != null)
                {
                    _columns.Add(ValueColumn);
                }

                foreach (AstAttributeKeyColumnNode keyColumn in _keyColumns)
                {
                    _columns.Add(keyColumn);
                }

                return _columns;
            }
        }
    }
}
