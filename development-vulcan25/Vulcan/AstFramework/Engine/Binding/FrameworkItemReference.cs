using AstFramework.Model;

namespace AstFramework.Engine.Binding
{
    public class FrameworkItemReference
    {
        private readonly IFrameworkItem _referencingItem;
        private readonly string _propertyName;
        private readonly IReferenceableItem _referencedItem;

        public IFrameworkItem ReferencingItem
        {
            get { return _referencingItem; }
        }

        public string PropertyName
        {
            get { return _propertyName; }
        }

        public IReferenceableItem ReferencedItem
        {
            get { return _referencedItem; }
        }

        public FrameworkItemReference(IFrameworkItem referencingItem, string propertyName, IReferenceableItem referencedItem)
        {
            _referencingItem = referencingItem;
            _propertyName = propertyName;
            _referencedItem = referencedItem;
        }

        public override bool Equals(object obj)
        {
            var reference = obj as FrameworkItemReference;
            if (reference != null)
            {
                return Equals(reference);
            }

            return false;
        }

        public bool Equals(FrameworkItemReference reference)
        {
            bool referencingNode = ReferencingItem == reference.ReferencingItem;
            bool propertyName = PropertyName == reference.PropertyName;
            bool referencedNode = ReferencedItem == reference.ReferencedItem;
            return referencingNode && propertyName && referencedNode;
        }

        public override int GetHashCode()
        {
            int referencingNode = ReferencingItem.GetHashCode();
            int propertyName = PropertyName.GetHashCode();
            int referencedNode = ReferencedItem.GetHashCode();
            return referencingNode + propertyName + referencedNode;
        }
    }
}