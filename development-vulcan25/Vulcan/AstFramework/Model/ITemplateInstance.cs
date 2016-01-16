using System.Collections.Generic;
using AstFramework.Engine.Binding;

namespace AstFramework.Model
{
    public interface ITemplateInstance
    {
        ITemplate Template { get; }

        void Instantiate(SymbolTable symbolTable, UnboundReferences unboundReferences);

        Dictionary<string, string> ArgumentDictionary { get; }
    }
}
