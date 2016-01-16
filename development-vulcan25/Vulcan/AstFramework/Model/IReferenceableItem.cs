using System.Collections.Generic;
using AstFramework.Engine.Binding;

namespace AstFramework.Model
{
    public interface IReferenceableItem : IFrameworkItem
    {
        string Name { get; set; }

        string ScopedName { get; }

        HashSet<FrameworkItemReference> References { get; }

        bool Reference(IFrameworkItem frameworkItem, string propertyName);

        bool Unreference(IFrameworkItem frameworkItem, string propertyName);

        bool Unreference(IFrameworkItem frameworkItem);

        bool DefineSymbol();

        bool Undefine();
    }
}
