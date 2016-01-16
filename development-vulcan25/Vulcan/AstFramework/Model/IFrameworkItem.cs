using System;
using System.Collections.Generic;
using System.ComponentModel;
using Vulcan.Utility.Collections;
using Vulcan.Utility.Xml;

namespace AstFramework.Model
{
    public interface IFrameworkItem : INotifyPropertyChanged, IXObjectMappingProvider
    {
        IFrameworkItem ParentItem { get; set; }

        ISymbolTableProvider SymbolTableProvider { get; }

        BimlFile BimlFile { get; set; }

        IScopeBoundary ScopeBoundary { get; }

        IEnumerable<IFrameworkItem> AllDefinedSuccessors();

        VulcanCollection<IFrameworkItem> DefinedAstNodes();

        IEnumerable<IScopeBoundary> BindingScopeBoundaries();

        IFrameworkItem Clone();

        IFrameworkItem Clone(IFrameworkItem parentItem);

        IFrameworkItem Clone(Dictionary<IFrameworkItem, IFrameworkItem> cloneMapping);

        IFrameworkItem Clone(IFrameworkItem parentItem, Dictionary<IFrameworkItem, IFrameworkItem> cloneMapping);

        void CloneInto(IFrameworkItem targetItem, Dictionary<IFrameworkItem, IFrameworkItem> cloneMapping);

        IFrameworkItem CloneHusk(IFrameworkItem parentItem);

        void OnPropertyChanged(string propertyName);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Advanced method provided for advanced developers.")]
        TAstNode FirstParent<TAstNode>() where TAstNode : class;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Advanced method provided for advanced developers.")]
        TAstNode FirstParent<TAstNode>(Type attributeType) where TAstNode : class;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Advanced method provided for advanced developers.")]
        TAstNode FirstThisOrParent<TAstNode>() where TAstNode : class;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Advanced method provided for advanced developers.")]
        TAstNode FirstThisOrParent<TAstNode>(Type attributeType) where TAstNode : class;
    }
}
