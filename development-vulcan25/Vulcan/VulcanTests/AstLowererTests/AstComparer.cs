using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AstFramework;
using AstFramework.Markup;
using AstFramework.Model;
using AstLowerer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vulcan.Utility.Common;
using VulcanEngine.Common;
using VulcanEngine.IR;
using VulcanEngine.Phases;

namespace VulcanTests
{
    [Flags]
    public enum AstComparerOptions
    {
        None = 0x0,
        IgnoreNonFrameworkProperties = 0x1,
        IgnoreExternalReferences = 0x2,
        IgnoreNameDifferences = 0x4,
        IgnoreStringDifferences = 0x8,
        IgnoreStringWhiteSpaceDifferences = 0x10,
    }

    public class AstComparer
    {
        private readonly string _defaultXmlNamespace;
        private string _resourcePrefix = "VulcanTests.AstLowererTests";

        public AstComparerOptions AstComparerOptions { get; set; }

        public AstComparer(string defaultXmlNamespace, AstComparerOptions astComparerOptions)
        {
            _defaultXmlNamespace = defaultXmlNamespace;
            AstComparerOptions = astComparerOptions;
        }

        public AstComparer(string defaultXmlNamespace, AstComparerOptions astComparerOptions, string resourcePrefix)
        {
            _resourcePrefix = resourcePrefix;
            _defaultXmlNamespace = defaultXmlNamespace;
            AstComparerOptions = astComparerOptions;
        }

        private bool HasComparerOption(AstComparerOptions astComparerOption)
        {
            return (AstComparerOptions & astComparerOption) == astComparerOption;
        }

        public void CheckResourceFrameworkItemsNoLowering(string resource)
        {
            string filename = Utility.LoadResourceToTempFile("VulcanTests.AstLowererTests", resource);
            CheckFrameworkItemsNoLowering(filename);
            Utility.CleanTempFile(filename);
        }

        public void CheckFrameworkItemsNoLowering(string file)
        {
            Assert.IsTrue(File.Exists(file), "File was not found");
            
            // TODO: Fix this hack
            MessageEngine.ClearMessages();

            var parser = new XmlToAstParserPhase("TestParser", _defaultXmlNamespace);
            var lowerer = new AstLowererPhase("TestLowerer");

            var unloweredXmlIR = new XmlIR();
            unloweredXmlIR.AddXml(file, XmlIRDocumentType.Source, true);
            var unloweredAstRootNode = ((AstIR)parser.Execute(unloweredXmlIR)).AstRootNode;
            Assert.IsTrue(MessageEngine.ErrorCount == 0, "There were parse errors in the unlowered file pass.");

            // TODO: Fix this hack
            MessageEngine.ClearMessages();

            var loweredXmlIR = new XmlIR();
            loweredXmlIR.AddXml(file, XmlIRDocumentType.Source, true);
            IIR loweredParsed = parser.Execute(loweredXmlIR);
            Assert.IsTrue(MessageEngine.ErrorCount == 0, "There were parse errors in the lowered file pass.");

            // TODO: Fix this hack
            MessageEngine.ClearMessages();

            var loweredAstRootNode = ((AstIR)lowerer.Execute(loweredParsed)).AstRootNode;
            Assert.IsTrue(MessageEngine.ErrorCount == 0, "There were lowering errors in the file.");

            CheckFrameworkItems(unloweredAstRootNode, loweredAstRootNode, false);
        }

        public void CheckResourceFrameworkItemsExactMatch(string parentResource, string subResource)
        {
            CheckResourceFrameworkItems(parentResource, subResource, false);
        }

        public void CheckResourceFrameworkItemsSubsetOf(string parentResource, string subResource)
        {
            CheckResourceFrameworkItems(parentResource, subResource, true);
        }

        private void CheckResourceFrameworkItems(string parentResource, string subResource, bool isSubsetOf)
        {
            string preFilename = Utility.LoadResourceToTempFile(_resourcePrefix, parentResource);
            string postFilename = Utility.LoadResourceToTempFile(_resourcePrefix, subResource);
            CheckFrameworkItems(preFilename, postFilename, isSubsetOf);
            Utility.CleanTempFile(postFilename);
            Utility.CleanTempFile(preFilename);
        }

        private void CheckFrameworkItems(string parentFile, string subFile, bool isSubsetOf)
        {
            Assert.IsTrue(File.Exists(parentFile), "PRE file was not found");
            Assert.IsTrue(File.Exists(subFile), "POST file was not found");

            // TODO: Fix this hack
            MessageEngine.ClearMessages();

            var parser = new XmlToAstParserPhase("TestParser", _defaultXmlNamespace);
            var lowerer = new AstLowererPhase("TestLowerer");

            var parentXmlIR = new XmlIR();
            parentXmlIR.AddXml(parentFile, XmlIRDocumentType.Source, true);
            IIR parentParsed = parser.Execute(parentXmlIR);
            Assert.IsTrue(MessageEngine.ErrorCount == 0, "There were parse errors in the PRE file.");

            // TODO: Fix this hack
            MessageEngine.ClearMessages();

            var parentAstRootNode = ((AstIR)lowerer.Execute(parentParsed)).AstRootNode;
            Assert.IsTrue(MessageEngine.ErrorCount == 0, "There were lowering errors in the PRE file.");

            // TODO: Fix this hack
            MessageEngine.ClearMessages();

            var subXmlIR = new XmlIR();
            subXmlIR.AddXml(subFile, XmlIRDocumentType.Source, true);
            IIR subParsed = parser.Execute(subXmlIR);
            Assert.IsTrue(MessageEngine.ErrorCount == 0, "There were parse errors in the POST file.");

            // TODO: Fix this hack
            MessageEngine.ClearMessages();

            var subAstRootNode = ((AstIR)lowerer.Execute(subParsed)).AstRootNode;
            Assert.IsTrue(MessageEngine.ErrorCount == 0, "There were lowering errors in the POST file.");
            
            CheckFrameworkItems(parentAstRootNode, subAstRootNode, isSubsetOf);
        }

        private void CheckFrameworkItems(IFrameworkItem parentItem, IFrameworkItem subitem, bool isSubsetOf)
        {
            Dictionary<IFrameworkItem, IFrameworkItem> definitionMappings = LoadDefinitionMappings(parentItem, subitem);
            CheckFrameworkItems(parentItem, subitem, isSubsetOf, definitionMappings);
        }

        private void CheckFrameworkItems(IFrameworkItem parentItem, IFrameworkItem subitem, bool isSubsetOf, Dictionary<IFrameworkItem, IFrameworkItem> definitionMappings)
        {
            var parentItemType = parentItem.GetType();
            var subItemType = subitem.GetType();
            Assert.AreEqual(parentItemType, subItemType, String.Format(CultureInfo.InvariantCulture, "FrameworkItem type {0} != {1}", parentItemType, subItemType));

            foreach (var property in parentItemType.GetProperties())
            {
                // TODO: Need to exclude indexed properties
                if (HasComparerOption(AstComparerOptions.IgnoreNonFrameworkProperties))
                {
                    var bindingAttributes = CustomAttributeProvider.Global.GetAttribute<AstXNameBindingAttribute>(property, true);
                    var mergeableAttributes = CustomAttributeProvider.Global.GetAttribute<AstMergeablePropertyAttribute>(property, true);
                    if (bindingAttributes.Count > 0 || mergeableAttributes.Count > 0)
                    {
                        CompareProperties(property, parentItem, subitem, isSubsetOf, definitionMappings);
                    }
                }
                else
                {
                    CompareProperties(property, parentItem, subitem, isSubsetOf, definitionMappings);
                }
            }
        }

        private void CompareProperties(PropertyInfo property, IFrameworkItem parentItem, IFrameworkItem subitem, bool isSubsetOf, Dictionary<IFrameworkItem, IFrameworkItem> definitionMappings)
        {
            bool isSubsetMatch = isSubsetOf && parentItem is IRootItem;
            ComparePropertiesSubsetMatch(property, parentItem, subitem, definitionMappings, isSubsetMatch);
        }

        private void ComparePropertiesSubsetMatch(PropertyInfo property, IFrameworkItem parentItem, IFrameworkItem subitem, Dictionary<IFrameworkItem, IFrameworkItem> definitionMappings, bool isSubsetMatch)
        {
            object parentValue = property.GetValue(parentItem, null);
            object subValue = property.GetValue(subitem, null);

            var parentFrameworkItem = parentValue as IFrameworkItem;
            var subFrameworkItem = subValue as IFrameworkItem;

            bool parentDefinition = parentFrameworkItem != null && definitionMappings.ContainsKey(parentFrameworkItem);
            bool subDefinition = parentDefinition && definitionMappings[parentFrameworkItem] == subFrameworkItem;

            Assert.AreEqual(parentDefinition, subDefinition, String.Format(CultureInfo.InvariantCulture, "Both property values for {0}.{1} must be present or absent together in the defined node list.", subitem, property.Name));

            bool bothFrameworkItems = parentFrameworkItem != null && subFrameworkItem != null;
            if (bothFrameworkItems && (parentDefinition || !HasComparerOption(AstComparerOptions.IgnoreExternalReferences)))
            {
                CheckFrameworkItems(parentFrameworkItem, subFrameworkItem, false, definitionMappings);
            }
            else if (subValue != null && CommonUtility.IsContainerOf(typeof(ICollection<IFrameworkItem>), subValue.GetType()))
            {
                var subCollection = subValue as IList;
                var parentCollection = parentValue as IList;
                if (isSubsetMatch)
                {
                    Assert.IsTrue(parentCollection.Count >= subCollection.Count, String.Format(CultureInfo.InvariantCulture, "Defined FrameworkItem Child Count not subset in {0}.{1} - {2} < {3}", subitem, property.Name, parentCollection.Count, subCollection.Count));
                }
                else
                {
                    Assert.IsTrue(parentCollection.Count == subCollection.Count, String.Format(CultureInfo.InvariantCulture, "Defined FrameworkItem Child Count Mismatch in {0}.{1} - {2} != {3}", subitem, property.Name, parentCollection.Count, subCollection.Count));
                }

                for (int i = 0; i < subCollection.Count; i++)
                {
                    CheckFrameworkItems((IFrameworkItem)parentCollection[i], (IFrameworkItem)subCollection[i], false, definitionMappings);
                }
            }
            else if (!bothFrameworkItems)
            {
                var originalParentValue = parentValue;
                var originalSubValue = subValue;
                var stringValue1 = parentValue as string;
                var stringValue2 = subValue as string;
                bool stringDifferenceExcluded = stringValue1 != null && HasComparerOption(AstComparerOptions.IgnoreStringDifferences);
                bool nameDifferenceExcluded = property.Name == "Name" && HasComparerOption(AstComparerOptions.IgnoreNameDifferences);
                if (stringValue1 != null && HasComparerOption(AstComparerOptions.IgnoreStringWhiteSpaceDifferences))
                {
                    parentValue = Regex.Replace(stringValue1, @"\s", string.Empty);
                    subValue = Regex.Replace(stringValue2, @"\s", string.Empty);
                }

                if (!stringDifferenceExcluded && !nameDifferenceExcluded)
                {
                    Assert.IsTrue((parentValue == null && subValue == null) || parentValue.Equals(subValue), String.Format(CultureInfo.InvariantCulture, "In property '{0}' of parent item '{1}', values do not match: '{2}' != '{3}'", property.Name, parentItem, originalParentValue, originalSubValue));                    
                }
            }
        }

        public static Dictionary<IFrameworkItem, IFrameworkItem> LoadDefinitionMappings(IFrameworkItem parentItem, IFrameworkItem subitem)
        {
            var definitionMappings = new Dictionary<IFrameworkItem, IFrameworkItem>();

            var remainingParentItemDefinedNodes = new Stack<IFrameworkItem>();
            var remainingSubItemDefinedNodes = new Stack<IFrameworkItem>();

            remainingParentItemDefinedNodes.Push(parentItem);
            remainingSubItemDefinedNodes.Push(subitem);

            while (remainingSubItemDefinedNodes.Count > 0)
            {
                var currentParentItemDefinedNode = remainingParentItemDefinedNodes.Pop();
                var currentSubItemDefinedNode = remainingSubItemDefinedNodes.Pop();

                definitionMappings[currentParentItemDefinedNode] = currentSubItemDefinedNode;

                var parentItemChildDefinedNodes = currentParentItemDefinedNode.DefinedAstNodes();
                var subItemChildDefinedNodes = currentSubItemDefinedNode.DefinedAstNodes();

                foreach (var childType in new HashSet<Type>(subItemChildDefinedNodes.Select(item => item.GetType())))
                {
                    var typedSubItemChildDefinedNodes = subItemChildDefinedNodes.Where(item => item.GetType() == childType).ToList();
                    var typedParentItemChildDefinedNodes = parentItemChildDefinedNodes.Where(item => item.GetType() == childType).ToList();
                    Assert.AreEqual(typedParentItemChildDefinedNodes.Count, typedSubItemChildDefinedNodes.Count, String.Format(CultureInfo.InvariantCulture, "Mismatch in {0} count of children of type {1} - {2} != {3}", currentSubItemDefinedNode, childType.Name, typedParentItemChildDefinedNodes.Count, typedSubItemChildDefinedNodes.Count));

                    for (int i = 0; i < typedSubItemChildDefinedNodes.Count; i++)
                    {
                        remainingParentItemDefinedNodes.Push(typedParentItemChildDefinedNodes[i]);
                        remainingSubItemDefinedNodes.Push(typedSubItemChildDefinedNodes[i]);
                    }
                }
            }

            return definitionMappings;
        }
    }
}
