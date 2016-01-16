using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using DataflowEngine.Statements;
using DataFlowEngine.TupleProviders.Transformations;
using Vulcan.Utility.Collections;
using Vulcan.Utility.Graph;
using VulcanEngine.IR.Ast.AstFramework;
using VulcanEngine.IR.Ast.Transformation;

namespace DataflowEngine
{
    public class TupleDataflowGraph : BasicBlockGraph<TupleSequence>
    {
        private TupleProviderManager _tupleProviderManager;

        private readonly BasicBlock<TupleSequence> _prologueBasicBlock;
        private readonly GraphNode<BasicBlock<TupleSequence>> _prologueBasicBlockNode;

        private readonly BasicBlock<TupleSequence> _epilogueBasicBlock;
        private readonly GraphNode<BasicBlock<TupleSequence>> _epilogueBasicBlockNode;

        private readonly Dictionary<GraphNode<BasicBlock<IDataflowAstNode>>, GraphNode<BasicBlock<TupleSequence>>> _astBasicBlockNodeToTupleBasicBlockNodeMapping;
        private readonly Dictionary<IDataflowAstNode, BasicBlock<TupleSequence>> _astToBasicBlockMapping;

        private Dictionary<GraphEdge<BasicBlock<IDataflowAstNode>>, GraphNode<BasicBlock<TupleSequence>>> _edgeInputMappingNodeMappings;

        public BasicBlockGraph<IDataflowAstNode> AstDataflowGraph { get; private set; }

        public ObservableDictionary<Identifier, ObservableHashSet<Definition>> ExternalDefinitions { get; private set; }

        protected override GraphNode<BasicBlock<TupleSequence>> CreateNode(BasicBlock<TupleSequence> item)
        {
            return new TupleDataflowGraphNode(item);
        }

        private void InitializeAstDataflowGraphEventHandlers()
        {
            AstDataflowGraph.RootNodes.CollectionChanged += AstDataflowGraphRootNodes_CollectionChanged;
            AstDataflowGraph.LeafNodes.CollectionChanged += AstDataflowGraphLeafNodes_CollectionChanged;
            AstDataflowGraph.Nodes.CollectionChanged += AstDataflowGraphNodes_CollectionChanged;
            AstDataflowGraph.Edges.CollectionChanged += AstDataflowGraphEdges_CollectionChanged;
        }

        public TupleDataflowGraph(BasicBlockGraph<IDataflowAstNode> basicBlockGraph)
            : base()
        {
            AstDataflowGraph = basicBlockGraph;

            InitializeAstDataflowGraphEventHandlers();

            _prologueBasicBlock = new BasicBlock<TupleSequence>();
            _prologueBasicBlockNode = AddNode(_prologueBasicBlock);

            _epilogueBasicBlock = new BasicBlock<TupleSequence>();
            _epilogueBasicBlockNode = AddNode(_epilogueBasicBlock);

            _edgeInputMappingNodeMappings = new Dictionary<GraphEdge<BasicBlock<IDataflowAstNode>>, GraphNode<BasicBlock<TupleSequence>>>();

            _astBasicBlockNodeToTupleBasicBlockNodeMapping = new Dictionary<GraphNode<BasicBlock<IDataflowAstNode>>, GraphNode<BasicBlock<TupleSequence>>>();
            _astToBasicBlockMapping = new Dictionary<IDataflowAstNode, BasicBlock<TupleSequence>>();

            _tupleProviderManager = new TupleProviderManager();

            ExternalDefinitions = new ObservableDictionary<Identifier, ObservableHashSet<Definition>>();
            Nodes.CollectionChanged += Nodes_CollectionChanged;

            PermitDuplicateNodeItems = false;
            ErrorOnDuplicateNodeAddAttempt = false;

            foreach (var basicBlockNode in basicBlockGraph.Nodes)
            {
                AddDataflowAstNodeBasicBlock(basicBlockNode);
            }

            foreach (var basicBlockEdge in basicBlockGraph.Edges)
            {
                OnAddAstDataflowGraphEdge(basicBlockEdge);
//                AddEdge(_astBasicBlockNodeToTupleBasicBlockNodeMapping[basicBlockEdge.Source], _astBasicBlockNodeToTupleBasicBlockNodeMapping[basicBlockEdge.Sink]);
            }
            
            var snapshotRootNodes = new List<GraphNode<BasicBlock<TupleSequence>>>(RootNodes);
            foreach (var rootNode in snapshotRootNodes)
            {
                if (rootNode != _prologueBasicBlockNode && rootNode != _epilogueBasicBlockNode)
                {
                    AddEdge(_prologueBasicBlockNode, rootNode);
                }
            }

            var snapshotLeafNodes = new List<GraphNode<BasicBlock<TupleSequence>>>(LeafNodes);
            foreach (var leafNode in snapshotLeafNodes)
            {
                if (leafNode != _epilogueBasicBlockNode && leafNode != _prologueBasicBlockNode)
                {
                    AddEdge(leafNode, _epilogueBasicBlockNode);
                }
            }
        }

        #region Leaf and Root Node Management
        private void AstDataflowGraphRootNodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (GraphNode<BasicBlock<IDataflowAstNode>> oldNode in e.OldItems)
                {
                    RemoveEdge(FindEdge(_prologueBasicBlockNode, _astBasicBlockNodeToTupleBasicBlockNodeMapping[oldNode]));
                }
            }
            if (e.NewItems != null)
            {
                foreach (GraphNode<BasicBlock<IDataflowAstNode>> newNode in e.NewItems)
                {
                    AddEdge(_prologueBasicBlockNode, _astBasicBlockNodeToTupleBasicBlockNodeMapping[newNode]);
                }
            }
        }

        private void AstDataflowGraphLeafNodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (GraphNode<BasicBlock<IDataflowAstNode>> oldNode in e.OldItems)
                {
                    RemoveEdge(FindEdge(_astBasicBlockNodeToTupleBasicBlockNodeMapping[oldNode], _epilogueBasicBlockNode));
                }
            }
            if (e.NewItems != null)
            {
                foreach (GraphNode<BasicBlock<IDataflowAstNode>> newNode in e.NewItems)
                {
                    AddEdge(_astBasicBlockNodeToTupleBasicBlockNodeMapping[newNode], _epilogueBasicBlockNode);
                }
            }
        }
        #endregion Leaf and Root Node Management

        #region AstDataflowGraph BasicBlock Management
        private void AstDataflowGraphNodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (GraphNode<BasicBlock<IDataflowAstNode>> oldNode in e.OldItems)
                {
                    RemoveDataflowAstNodeBasicBlock(oldNode);
                }
            }
            if (e.NewItems != null)
            {
                foreach (GraphNode<BasicBlock<IDataflowAstNode>> newNode in e.NewItems)
                {
                    AddDataflowAstNodeBasicBlock(newNode);
                }
            }
        }

        private void AddDataflowAstNodeBasicBlock(GraphNode<BasicBlock<IDataflowAstNode>> dataflowAstNodeBasicBlockNode)
        {
            var tupleBasicBlock = new BasicBlock<TupleSequence>();
            bool first = true;
            foreach (var dataflowAstNode in dataflowAstNodeBasicBlockNode.Item)
            {
                AddDataflowAstNode(dataflowAstNode, tupleBasicBlock, first);
                first = false;
            }
            _astBasicBlockNodeToTupleBasicBlockNodeMapping[dataflowAstNodeBasicBlockNode] = AddNode(tupleBasicBlock);
            dataflowAstNodeBasicBlockNode.Item.CollectionChanged += AstDataflowGraphBasicBlockItems_CollectionChanged;
            //AddNode(tupleBasicBlock);
        }

        private void RemoveDataflowAstNodeBasicBlock(GraphNode<BasicBlock<IDataflowAstNode>> dataflowAstNodeBasicBlockNode)
        {
            foreach (var dataflowAstNode in dataflowAstNodeBasicBlockNode.Item)
            {
                RemoveDataflowAstNode(dataflowAstNode);
            }
            RemoveNode(_astBasicBlockNodeToTupleBasicBlockNodeMapping[dataflowAstNodeBasicBlockNode]);
            _astBasicBlockNodeToTupleBasicBlockNodeMapping.Remove(dataflowAstNodeBasicBlockNode);
            dataflowAstNodeBasicBlockNode.Item.CollectionChanged -= AstDataflowGraphBasicBlockItems_CollectionChanged;
        }
        #endregion AstDataflowGraph BasicBlock Management

        #region AstDataflowGraph Edge Management
        private void AstDataflowGraphEdges_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (GraphEdge<BasicBlock<IDataflowAstNode>> oldEdge in e.OldItems)
                {
                    OnRemoveAstDataflowGraphEdge(oldEdge);
                }
            }
            if (e.NewItems != null)
            {
                foreach (GraphEdge<BasicBlock<IDataflowAstNode>> newEdge in e.NewItems)
                {
                    OnAddAstDataflowGraphEdge(newEdge);
                }
            }
        }

        protected void OnAddAstDataflowGraphEdge(GraphEdge<BasicBlock<IDataflowAstNode>> edge)
        {
            var mappingTupleBasicBlock = new BasicBlock<TupleSequence>();
            var mappingTupleBasicBlockNode = AddNode(mappingTupleBasicBlock);
            _edgeInputMappingNodeMappings.Add(edge, mappingTupleBasicBlockNode);

            OnChangeAstDataflowGraphEdgeData(edge);
            
            AddEdge(_astBasicBlockNodeToTupleBasicBlockNodeMapping[edge.Source], mappingTupleBasicBlockNode);
            AddEdge(mappingTupleBasicBlockNode, _astBasicBlockNodeToTupleBasicBlockNodeMapping[edge.Sink]);

            edge.PropertyChanged += AstDataflowGraphEdgePropertyChanged;
        }

        protected void OnRemoveAstDataflowGraphEdge(GraphEdge<BasicBlock<IDataflowAstNode>> edge)
        {
            if (_edgeInputMappingNodeMappings.ContainsKey(edge))
            {
                RemoveNode(_edgeInputMappingNodeMappings[edge]);
                edge.PropertyChanged -= AstDataflowGraphEdgePropertyChanged;
            }
        }

        #endregion AstDataflowGraph Edge Management

        #region AstDataflowGraph Edge Data Management

        void AstDataflowGraphEdgePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SinkData")
            {
                var edge = sender as GraphEdge<BasicBlock<IDataflowAstNode>>;
                OnChangeAstDataflowGraphEdgeData(edge);
            }
        }

        protected void OnChangeAstDataflowGraphEdgeData(GraphEdge<BasicBlock<IDataflowAstNode>> edge)
        {
            if (_edgeInputMappingNodeMappings.ContainsKey(edge))
            {
                if (_edgeInputMappingNodeMappings[edge].Item.Count > 0)
                {
                    _edgeInputMappingNodeMappings[edge].Item.RemoveAt(0);
                }

                var astTransformationNode = edge.Sink.Item.Leader as AstTransformationNode;
                var astDataflowMappedInputPathNode = edge.SinkData as AstDataflowMappedInputPathNode;
                if (astDataflowMappedInputPathNode != null && astTransformationNode != null)
                {
                    // TODO: We may want some logic here so that the AstTransformation node only gets pulled in for unionall and other relevant types
                    var mappingTupleProvider = new MappedInputTupleProvider(astTransformationNode, astDataflowMappedInputPathNode);
                    _edgeInputMappingNodeMappings[edge].Item.Add(mappingTupleProvider.InlineTupleSequence);
                }
            }
        }
        #endregion AstDataflowGraph Edge Data Management

        #region DataflowAstNode Management
        void AstDataflowGraphBasicBlockItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var sourceBasicBlock = sender as BasicBlock<IDataflowAstNode>;
            var tupleBasicBlockNode = _astBasicBlockNodeToTupleBasicBlockNodeMapping[AstDataflowGraph.FindNode(sourceBasicBlock)];

            if (e.OldItems != null)
            {
                foreach (IDataflowAstNode oldNode in e.OldItems)
                {
                    RemoveDataflowAstNode(oldNode);
                }
            }
            if (e.NewItems != null)
            {
                bool first = e.NewItems.Count == sourceBasicBlock.Count;
                foreach (IDataflowAstNode newNode in e.NewItems)
                {
                    AddDataflowAstNode(newNode, tupleBasicBlockNode.Item, first);
                    first = false;
                }
            }
        }

        private void AddDataflowAstNode(IDataflowAstNode dataflowAstNode, BasicBlock<TupleSequence> tupleBasicBlock, bool preventMappingEmission)
        {
            _astToBasicBlockMapping[dataflowAstNode] = tupleBasicBlock;
            var tupleProvider = _tupleProviderManager[dataflowAstNode];

            var singleMappedInputTupleProvider = tupleProvider as SingleMappedInputTupleProvider;
            if (singleMappedInputTupleProvider != null)
            {
                singleMappedInputTupleProvider.PreventMappedInputTupleEmission = preventMappingEmission;
            }

            // TODO: Do I have to account for inserts here or can we canonicalize on add-to-the-end?
            tupleBasicBlock.Add(tupleProvider.InlineTupleSequence);
            _prologueBasicBlock.Add(tupleProvider.PrologueTupleSequence);
            _epilogueBasicBlock.Add(tupleProvider.EpilogueTupleSequence);
        }

        private void AddDataflowAstNode(IDataflowAstNode dataflowAstNode, BasicBlock<TupleSequence> tupleBasicBlock)
        {
            AddDataflowAstNode(dataflowAstNode, tupleBasicBlock, false);
        }

        private void RemoveDataflowAstNode(IDataflowAstNode dataflowAstNode)
        {
            var tupleBasicBlock = _astToBasicBlockMapping[dataflowAstNode];
            _astToBasicBlockMapping.Remove(dataflowAstNode);

            var tupleProvider = _tupleProviderManager[dataflowAstNode];

            tupleBasicBlock.Remove(tupleProvider.InlineTupleSequence);
            _prologueBasicBlock.Remove(tupleProvider.PrologueTupleSequence);
            _epilogueBasicBlock.Remove(tupleProvider.EpilogueTupleSequence);

        }
        #endregion DataflowAstNode Management

        #region External Definition Management


        void Nodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (TupleDataflowGraphNode oldNode in e.OldItems)
                {
                    oldNode.ExternalDefinitions.CollectionChanged -= ExternalDefinitions_CollectionChanged;
                    foreach (var externalDefinitonPair in oldNode.ExternalDefinitions)
                    {
                        OnRemoveExternalDefinitions(externalDefinitonPair.Key, externalDefinitonPair.Value);
                    }
                }
            }
            if (e.NewItems != null)
            {
                foreach (TupleDataflowGraphNode newNode in e.NewItems)
                {
                    foreach (var externalDefinitonPair in newNode.ExternalDefinitions)
                    {
                        OnAddExternalDefinitions(externalDefinitonPair.Key, externalDefinitonPair.Value);
                    }
                    newNode.ExternalDefinitions.CollectionChanged += ExternalDefinitions_CollectionChanged;
                }
            }
        }

        void ExternalDefinitions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (KeyValuePair<Identifier, ObservableHashSet<Definition>> oldPair in e.OldItems)
                {
                    OnRemoveExternalDefinitions(oldPair.Key, oldPair.Value);
                }
            }
            if (e.NewItems != null)
            {
                foreach (KeyValuePair<Identifier, ObservableHashSet<Definition>> newPair in e.NewItems)
                {
                    OnAddExternalDefinitions(newPair.Key, newPair.Value);
                }
            }
        }

        protected void OnAddExternalDefinitions(Identifier identifier, ObservableHashSet<Definition> definitions)
        {
            if (!ExternalDefinitions.ContainsKey(identifier))
            {
                ExternalDefinitions.Add(identifier, new ObservableHashSet<Definition>());
            }
            ExternalDefinitions[identifier].UnionWith(definitions);
        }

        protected void OnRemoveExternalDefinitions(Identifier identifier, ObservableHashSet<Definition> definitions)
        {
            if (ExternalDefinitions.ContainsKey(identifier))
            {
                // TODO: If definitions somehow got on the list from multiple vectors, then this is too aggressive.  I don't think that's a scenario, though.
                ExternalDefinitions[identifier].ExceptWith(definitions);
            }
        }

        #endregion External Definition Management
    }
}
