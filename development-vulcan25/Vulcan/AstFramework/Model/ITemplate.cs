using System.Collections.Generic;
using AstFramework.Engine.Binding;

namespace AstFramework.Model
{
    public interface ITemplate : IReferenceableItem
    {
        UnboundReferences UnboundReferences { get; }
    }
}
