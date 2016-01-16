using AstFramework.Engine.Binding;

namespace AstFramework.Model
{
    public interface ISymbolTableProvider : IFrameworkItem
    {
        SymbolTable SymbolTable { get; }
    }
}
