using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AstFramework.Model;

namespace AstFramework.Engine.Binding
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Name follows the naming convention of AstFramework.")]
    public class SymbolTable : IEnumerable<IReferenceableItem>
    {
        private readonly HashSet<IReferenceableItem> _store;

        public IFrameworkItem ParentItem { get; set; }

        private readonly Dictionary<string, HashSet<IReferenceableItem>> _lookupDictionary;
        
        private readonly Dictionary<Type, HashSet<IReferenceableItem>> _typeDictionary;

        private readonly Dictionary<IReferenceableItem, string> _lastKnownScopedName;

        public SymbolTable(IFrameworkItem parentItem)
        {
            _store = new HashSet<IReferenceableItem>();
            ParentItem = parentItem;
            _lookupDictionary = new Dictionary<string, HashSet<IReferenceableItem>>();
            _typeDictionary = new Dictionary<Type, HashSet<IReferenceableItem>>();
            _lastKnownScopedName = new Dictionary<IReferenceableItem, string>();
        }

        public static void GetSourceToCloneDefinitionMappings(IFrameworkItem sourceFrameworkItem, IFrameworkItem cloneParentItem, Dictionary<IFrameworkItem, IFrameworkItem> cloneMapping)
        {
            var remainingDefinedNodes = new Stack<IFrameworkItem>();
            var parentMapping = new Dictionary<IFrameworkItem, IFrameworkItem>();

            remainingDefinedNodes.Push(sourceFrameworkItem);
            parentMapping.Add(sourceFrameworkItem, cloneParentItem);

            while (remainingDefinedNodes.Count > 0)
            {
                var currentDefinedNode = remainingDefinedNodes.Pop();
                IFrameworkItem clonedHusk = currentDefinedNode.CloneHusk(parentMapping[currentDefinedNode]);
                cloneMapping.Add(currentDefinedNode, clonedHusk);

                // TODO: DefinedAstNodes should return a HashSet or a List - VulcanCollection is too much overhead
                foreach (var childDefinedNode in currentDefinedNode.DefinedAstNodes())
                {
                    parentMapping.Add(childDefinedNode, clonedHusk);
                    remainingDefinedNodes.Push(childDefinedNode);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1023:IndexersShouldNotBeMultidimensional", Justification = "Advanced method provided for advanced developers.")]
        public IReferenceableItem this[Type type, string name]
        {
            get
            {
                HashSet<IReferenceableItem> typeLookup;
                if (_lookupDictionary.TryGetValue(name, out typeLookup))
                {
                    return typeLookup.FirstOrDefault(item => type.IsAssignableFrom(item.GetType()));
                }

                return null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers", Justification = "Advanced method provided for advanced developers.")]
        public IEnumerable<IReferenceableItem> this[Type type]
        {
            get
            {
                var set = new HashSet<IReferenceableItem>();
                foreach (var referenceableItemType in _typeDictionary.Keys)
                {
                    if (type.IsAssignableFrom(referenceableItemType))
                    {
                        foreach (var referenceableItem in _typeDictionary[referenceableItemType])
                        {
                            set.Add(referenceableItem);
                        }
                    }
                }

                return set;
            }
        }

        public bool Remove(IReferenceableItem oldAstNamedNode)
        {
            bool success = _store.Remove(oldAstNamedNode);
            OnRemoveNamedNode(oldAstNamedNode);
            oldAstNamedNode.PropertyChanged -= NewAstNamedNode_PropertyChanged;
            return success;
        }

        public void Add(IReferenceableItem newAstNamedNode)
        {
            _store.Add(newAstNamedNode);
            OnAddNamedNode(newAstNamedNode);
            newAstNamedNode.PropertyChanged += NewAstNamedNode_PropertyChanged;
        }

        // TODO: This does not work if parent nodes name changes or if parent is reassigned
        private void NewAstNamedNode_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var astNamedNode = sender as IReferenceableItem;
            if (astNamedNode != null && e.PropertyName == "ScopedName")
            {
                OnScopedNameChanged(astNamedNode);
            }
        }

        protected void OnScopedNameChanged(IReferenceableItem astNamedNode)
        {
            OnRemoveNamedNode(astNamedNode);
            OnAddNamedNode(astNamedNode);
        }

        protected void OnAddNamedNode(IReferenceableItem astNamedNode)
        {
            if (astNamedNode.Name != null)
            {
                if (!_lookupDictionary.ContainsKey(astNamedNode.ScopedName))
                {
                    _lookupDictionary.Add(astNamedNode.ScopedName, new HashSet<IReferenceableItem>());
                }

                Type astNamedNodeType = astNamedNode.GetType();
                if (!_typeDictionary.ContainsKey(astNamedNodeType))
                {
                    _typeDictionary.Add(astNamedNodeType, new HashSet<IReferenceableItem>());
                }

                _lookupDictionary[astNamedNode.ScopedName].Add(astNamedNode);
                _typeDictionary[astNamedNodeType].Add(astNamedNode);
                _lastKnownScopedName[astNamedNode] = astNamedNode.ScopedName;
            }
        }

        protected void OnRemoveNamedNode(IReferenceableItem astNamedNode)
        {
            string lastScopedName;
            HashSet<IReferenceableItem> nameLookup;
            HashSet<IReferenceableItem> typeLookup;
            Type astNamedNodeType = astNamedNode.GetType();
            if (_lastKnownScopedName.TryGetValue(astNamedNode, out lastScopedName) 
                && _lookupDictionary.TryGetValue(lastScopedName, out nameLookup) 
                && _typeDictionary.TryGetValue(astNamedNodeType, out typeLookup))
            {
                typeLookup.Remove(astNamedNode);
                if (typeLookup.Count == 0)
                {
                    _typeDictionary.Remove(astNamedNodeType);
                }

                nameLookup.Remove(astNamedNode);
                if (nameLookup.Count == 0)
                {
                    _lookupDictionary.Remove(lastScopedName);
                }

                _lastKnownScopedName.Remove(astNamedNode);
            }
        }

        public IEnumerator<IReferenceableItem> GetEnumerator()
        {
            return _store.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}