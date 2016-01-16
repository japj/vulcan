using System.Reflection;
using AstFramework.Markup;

namespace AstFramework.Engine.Binding
{
    public class PropertyBindingAttributePair
    {
        public PropertyInfo Property { get; private set; }

        public AstXNameBindingAttribute BindingAttribute { get; private set; }

        public PropertyBindingAttributePair(PropertyInfo property, AstXNameBindingAttribute bindingAttribute)
        {
            Property = property;
            BindingAttribute = bindingAttribute;
        }
    }
}