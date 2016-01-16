using System.Linq;
using System.Security.Permissions;
using AstFramework.Engine.Binding;
using AstFramework.Model;

namespace AstFramework
{
    public static class CompileTimeResolver
    {
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static void ResolveAll(SymbolTable symbolTable, UnboundReferences unboundReferences)
        {
            ResolveTemplateInstances(symbolTable, unboundReferences);
            ResolveUnboundReferences(symbolTable, unboundReferences);
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private static void ResolveTemplateInstances(SymbolTable symbolTable, UnboundReferences unboundReferences)
        {
            foreach (ITemplate template in symbolTable[typeof(ITemplate)])
            {
                template.UnboundReferences.ResolveAll(symbolTable);
            }

            // Iteratively pick off templates that are not in a template until they've all been processed
            // This logic is required to enable nesting of template instances within templates
            var templateInstance = symbolTable[typeof(ITemplateInstance)].FirstOrDefault(item => item.FirstThisOrParent<ITemplate>() == null);
            while (templateInstance != null)
            {
                ((ITemplateInstance)templateInstance).Instantiate(symbolTable, unboundReferences);
                templateInstance = symbolTable[typeof(ITemplateInstance)].FirstOrDefault(item => item.FirstThisOrParent<ITemplate>() == null);
            }
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private static void ResolveUnboundReferences(SymbolTable symbolTable, UnboundReferences unboundReferences)
        {
            unboundReferences.ResolveAll(symbolTable);
        }
    }
}
