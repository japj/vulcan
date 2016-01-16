using AstFramework.Dataflow;
using AstFramework.Engine.Binding;
using AstFramework.Model;
using Vulcan.Utility.Collections;

namespace VulcanEngine.IR.Ast
{
    public partial class AstRootNode : IDataflowItem
    {
        // We need to provide an Items collection so our UI can build a tree view of these items, grouped by type.
        // Although using a CompositeCollection in XAML would be preferable to explicitly creating an Items list, CompositeCollections cannot be grouped.
        [System.ComponentModel.BrowsableAttribute(false)]
        public VulcanCollection<IFrameworkItem> Items { get; private set; }

        public AstRootNode(IFrameworkItem parentItem) : base(parentItem)
        {
            Items = new VulcanCollection<IFrameworkItem>();
            SymbolTable = new SymbolTable(this);

            InitializeAstNode();

            CollectionPropertyChanged += AstRootNode_CollectionPropertyChanged;
        }

        private void AstRootNode_CollectionPropertyChanged(object sender, VulcanCollectionPropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Connections" 
                || e.PropertyName == "Tables" 
                || e.PropertyName == "Dimensions" 
                || e.PropertyName == "Facts" 
                || e.PropertyName == "Packages" 
                || e.PropertyName == "Schemas" 
                || e.PropertyName == "Principals")
            {
                VulcanCompositeCollectionChanged(Items, e);
            }
        }
    }
}
