using System.Collections.Generic;
using System.Security.Permissions;
using AstFramework.Engine.Binding;
using Vulcan.Utility.Collections;

namespace AstFramework.Engine.Binding
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Name matches naming convention of AstFramework.")]
    public class UnboundReferences : ObservableHashSet<BindingItem>
    {
        private Dictionary<object, HashSet<BindingItem>> _itemCache;
        private Dictionary<BimlFile, HashSet<BindingItem>> _bimlFileCache;

        public UnboundReferences()
        {
            _itemCache = new Dictionary<object, HashSet<BindingItem>>();
            _bimlFileCache = new Dictionary<BimlFile, HashSet<BindingItem>>();
            CollectionChanged += UnboundReferences_CollectionChanged;
        }

        // TODO: Do we need to check ParentAstNode and BimlFile for null?
        private void UnboundReferences_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (BindingItem oldItem in e.OldItems)
                {
                    if (_itemCache.ContainsKey(oldItem.ParentItem))
                    {
                        _itemCache[oldItem.ParentItem].Remove(oldItem);
                        if (_itemCache[oldItem.ParentItem].Count == 0)
                        {
                            _itemCache.Remove(oldItem.ParentItem);
                        }
                    }

                    if (_bimlFileCache.ContainsKey(oldItem.BimlFile))
                    {
                        _bimlFileCache[oldItem.BimlFile].Remove(oldItem);
                        if (_bimlFileCache[oldItem.BimlFile].Count == 0)
                        {
                            _bimlFileCache.Remove(oldItem.BimlFile);
                        }
                    }
                }
            }

            if (e.NewItems != null)
            {
                foreach (BindingItem newItem in e.NewItems)
                {
                    if (!_itemCache.ContainsKey(newItem.ParentItem))
                    {
                        _itemCache.Add(newItem.ParentItem, new HashSet<BindingItem>());
                    }

                    _itemCache[newItem.ParentItem].Add(newItem);

                    if (!_bimlFileCache.ContainsKey(newItem.BimlFile))
                    {
                        _bimlFileCache.Add(newItem.BimlFile, new HashSet<BindingItem>());
                    }

                    _bimlFileCache[newItem.BimlFile].Add(newItem);
                }
            }
        }

        public IEnumerable<BindingItem> this[object item]
        {
            get
            {
                if (_itemCache.ContainsKey(item))
                {
                    return _itemCache[item];
                }

                return new List<BindingItem>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers", Justification = "Advanced method provided for advanced developers.")]
        public IEnumerable<BindingItem> this[BimlFile bimlFile]
        {
            get
            {
                if (_bimlFileCache.ContainsKey(bimlFile))
                {
                    return _bimlFileCache[bimlFile];
                }

                return new List<BindingItem>();
            }

            set
            {
                var snapshot = new List<BindingItem>(this[bimlFile]);
                foreach (var oldItem in snapshot)
                {
                    Remove(oldItem);
                }

                foreach (var newItem in value)
                {
                    Add(newItem);
                }
            }
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public bool ResolveAll(SymbolTable index)
        {
            bool progressMade = true;
            while (progressMade && Count > 0)
            {
                progressMade = false;
                var itemsIterator = new List<BindingItem>(this);
                foreach (var item in itemsIterator)
                {
                    if (item.Bind(index))
                    {
                        progressMade = true;
                        Remove(item);
                    }
                }
            }

            return Count == 0;
        }
    }
}
