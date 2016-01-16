using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using Vulcan.Utility.Collections;
using VulcanEngine.IR.Ast;

namespace DataflowEngine.Statements
{
    public class Tuple : INotifyPropertyChanged, ITupleBase
    {
        public ObservableHashSet<Identifier> DefinedIdentifiers { get; private set; }
        public ObservableHashSet<Identifier> UsedIdentifiers { get; private set; }
        public ObservableDictionary<Identifier, ObservableHashSet<Definition>> ExternalDefinitions { get; private set; }
        public ObservableDictionary<Identifier, Definition> LastIdentifierDefinition { get; private set; }
        
        private Opcode _opcode;
        private AstNode _originAstNode;

        public Opcode Opcode
        {
            get { return _opcode; }
            set
            {
                if (_opcode != value)
                {
                    _opcode = value;
                    OnPropertyChanged("Opcode");
                }
            }
        }

        public AstNode OriginAstNode
        {
            get { return _originAstNode; }
            set
            {
                if (_originAstNode != value)
                {
                    _originAstNode = value;
                    OnPropertyChanged("OriginAstNode");
                }
            }
        }

        public VulcanCollection<Definition> LeftHandSide { get; private set; }
        public VulcanCollection<Use> RightHandSide { get; private set; }

        public Tuple()
        {
            LeftHandSide = new VulcanCollection<Definition>();
            RightHandSide = new VulcanCollection<Use>();

            DefinedIdentifiers = new ObservableHashSet<Identifier>();
            UsedIdentifiers = new ObservableHashSet<Identifier>();
            ExternalDefinitions = new ObservableDictionary<Identifier, ObservableHashSet<Definition>>();
            LastIdentifierDefinition = new ObservableDictionary<Identifier, Definition>();
        }

        public Tuple(Opcode opcode) : this()
        {
            Opcode = opcode;
            LeftHandSide.CollectionChanged += LeftHandSideCollectionChanged;
            RightHandSide.CollectionChanged += RightHandSideCollectionChanged;
        }

        public Tuple(AstNode originAstNode, Opcode opcode) : this(opcode)
        {
            OriginAstNode = originAstNode;
        }


        void LeftHandSideCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Definition definition in e.NewItems)
                {
                    definition.Tuple = this;
                }
            }
        }

        void RightHandSideCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Use use in e.NewItems)
                {
                    use.Tuple = this;
                }
            }
        }

        public override string ToString()
        {
            return String.Format("{0}: ({1}) <- ({2})", Opcode, FlattenStringList(LeftHandSide), FlattenStringList(RightHandSide));
        }

        private static string FlattenStringList(IEnumerable items)
        {
            return FlattenStringList(items, ",");
        }

        private static string FlattenStringList(IEnumerable items, string separator)
        {
            var flattenedList = new StringBuilder();
            bool isFirst = true;
            foreach (object item in items)
            {
                if (!isFirst)
                {
                    flattenedList.Append(separator);
                }
                isFirst = false;
                flattenedList.Append(item.ToString());
            }
            return flattenedList.ToString();
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
