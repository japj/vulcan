using System;
using Vulcan.Utility.Collections;

namespace AstFramework.Model
{
    public interface IRootItem : ISymbolTableProvider
    {
        bool RemoveRootNode(IFrameworkItem frameworkItem);

        VulcanCollection<IFrameworkItem> Items { get; }

        bool IsRootCollection(object collection);

        bool IsRootCollectionObjectType(Type type);

        bool IsRootCollectionObject(IFrameworkItem frameworkItem);

        string GetRootCollectionName(IFrameworkItem frameworkItem);

        string RootItemXsdName { get; }
    }
}
