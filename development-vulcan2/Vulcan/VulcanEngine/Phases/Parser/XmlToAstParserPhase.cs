using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

using VulcanEngine.Properties;
using VulcanEngine.Common;
using VulcanEngine.IR;
using VulcanEngine.IR.Ast;

namespace VulcanEngine.Phases.Parser
{
    [PhaseFriendlyName("XmlToAstParserPhase")]
    public class XmlToAstParserPhase : IPhase
    {
        // TODO: Add XPath binding semantics

        protected string _WorkflowUniqueName;
        PluginLoader<AstNode, AstSchemaTypeBindingAttribute> _Loader;
        Dictionary<XmlQualifiedName, Type> _ASTNodeTypesDictionary;
        Dictionary<Type, Dictionary<XName, PropertyBindingAttributePair>> _PropertyMappingDictionary;
        Dictionary<Type, Dictionary<string, AstNamedNode>> _GlobalASTNamedNodeDictionary;
        Dictionary<XName, List<AstNode>> _UnmappedProperties;
        List<BindingItem> _LateBindingItems;
        List<NewBindingItem> _NewObjectItems;
        Dictionary<Type, string> _DefaultXmlNamespacesByASTNodeType;
        AstParserScopeManager _ScopeManager;
        protected string _DefaultXmlNamespace;
        
        #region IPhase Members

        public string Name
        {
            get { return "XmlToASTParserPhase"; }
        }

        public string WorkflowUniqueName
        {
            get { return this._WorkflowUniqueName; }
        }

        public Type InputIRType
        {
            get { throw new NotImplementedException(); }
        }

        public VulcanEngine.IR.IIR Execute(List<VulcanEngine.IR.IIR> PredecessorIRs)
        {
            return this.Execute(IRUtility.MergeIRList(this.Name, PredecessorIRs));
        }

        public VulcanEngine.IR.IIR Execute(VulcanEngine.IR.IIR PredecessorIR)
        {
            // TODO: Enhance the PluginLoader so we can use it as the type store.  We're currently missing all types that lack a friendly name.
            XmlIR xmlIR = PredecessorIR as XmlIR;
            if (xmlIR == null)
            {
                // TODO: Message.Trace(Severity.Error, Resources.ErrorPhaseWorkflowIncorrectInputIRType, PredecessorIR.GetType().ToString(), this.Name);
            }

            AstIR astIR = new AstIR(xmlIR);
            astIR.AstRootNode = new AstRootNode();

            foreach (XmlIRDocumentType docType in astIR.XDocuments.Keys)
            {
                foreach (XDocument xDocument in astIR.XDocuments[docType])
                {
                    parseElement(xDocument.Root, astIR.AstRootNode,docType);
                }
            }

            HashSet<string> processedIncludeFiles = new HashSet<string>();
            List<string> newIncludeFiles = new List<string>();

            bool bContinue;
            do
            {
                bContinue = false;
                var newFiles = from file in astIR.AstRootNode.IncludedFiles
                               where !processedIncludeFiles.Contains(file.IncludedFile)
                               select file.IncludedFile;
                List<string> newFileList = new List<string>();
                newFileList.AddRange(newFiles);
                foreach (var xmlFile in newFileList)
                {
                    XDocument xDocument = astIR.AddXml(xmlFile, XmlIRDocumentType.INCLUDE);
                    if (xDocument != null)
                    {
                        bContinue = true;
                        parseElement(xDocument.Root, astIR.AstRootNode, XmlIRDocumentType.INCLUDE);
                        processedIncludeFiles.Add(xmlFile);
                    }
                }
            } while (bContinue);

            ProcessLateBindingQueue();

            ProcessNewObjectQueue(xmlIR);

            return astIR;
        }

        #endregion

        #region Initialization
        public XmlToAstParserPhase(string WorkflowUniqueName, string DefaultXmlNamespace)
        {
            this._DefaultXmlNamespace = DefaultXmlNamespace;
            this._DefaultXmlNamespacesByASTNodeType = new Dictionary<Type, string>();

            this._ScopeManager = new AstParserScopeManager();

            this._Loader = new PluginLoader<AstNode, AstSchemaTypeBindingAttribute>(Settings.Default.SubpathAstPluginFolder, 1, 2);
            this._ASTNodeTypesDictionary = new Dictionary<XmlQualifiedName, Type>();
            this.CopyPluginLoaderDataToASTNodeTypes(this._Loader);

            this._PropertyMappingDictionary = new Dictionary<Type, Dictionary<XName, PropertyBindingAttributePair>>();
            this._GlobalASTNamedNodeDictionary = new Dictionary<Type, Dictionary<string, AstNamedNode>>();
            this.PreLoadPropertyMappingsAndGlobalDictionary();

            this._UnmappedProperties = new Dictionary<XName, List<AstNode>>();

            this._LateBindingItems = new List<BindingItem>();
            this._NewObjectItems = new List<NewBindingItem>();
        }

        private void CopyPluginLoaderDataToASTNodeTypes(PluginLoader<AstNode, AstSchemaTypeBindingAttribute> loader)
        {
            // TODO: Can this be done elegantly inline with LINQ?
            foreach (KeyValuePair<AstSchemaTypeBindingAttribute, Type> pair in loader.PluginTypesByAttribute)
            {
                this._ASTNodeTypesDictionary.Add(pair.Key.XmlQualifiedName, pair.Value);
            }
        }

        private string GetDefaultXmlNamespaceCustomAttribute(Type t)
        {
            object[] attributeList = t.GetCustomAttributes(typeof(AstDefaultXmlNamespaceBindingAttribute), false);

            if (attributeList != null && attributeList.Length > 0)
            {
                if (attributeList.Length > 1)
                {
                    // TODO: Patch this up with a more generic warning/error
                    //_message.Trace(Severity.Warning, Resources.WarningMultiplePhaseFriendlyNames, t.AssemblyQualifiedName);
                    return null;
                }
                return attributeList[0].ToString();
            }
            return null;
        }

        private void PreLoadPropertyMappingsAndGlobalDictionary()
        {
            foreach (Type astNodeType in this._ASTNodeTypesDictionary.Values)
            {
                // Same type can be listed with multiple names, so check for existence first
                if (!this._PropertyMappingDictionary.ContainsKey(astNodeType))
                {
                    Dictionary<XName, PropertyBindingAttributePair> propertyBindings = new Dictionary<XName, PropertyBindingAttributePair>();
                    PropertyInfo[] astNodeProperties = astNodeType.GetProperties();

                    var NativeKeyValueTriplets =
                        from property in astNodeProperties
                        where property.CanWrite == true || IsContainerOf(typeof(ICollection<object>), property.PropertyType)
                        select new { xName = XName.Get(property.Name, GetDefaultXMLNamespace(astNodeType)), property };
                    // TODO: This is probably the wrong way to get the DefaultXmLNamspace, plus this doesn't error check lack of property

                    foreach (var triplet in NativeKeyValueTriplets)
                    {
                        propertyBindings.Add(triplet.xName, new PropertyBindingAttributePair(triplet.property, null));
                    }

                    var BindingAttributeKeyValueTriplets =
                        from property in astNodeProperties
                        from bindingAttribute in (AstXNameBindingAttribute[])property.GetCustomAttributes(typeof(AstXNameBindingAttribute), false)
                        where property.CanWrite == true || IsContainerOf(typeof(ICollection<object>), property.PropertyType)
                        select new { xName = bindingAttribute.XName, property, attribute = bindingAttribute };

                    foreach (var triplet in BindingAttributeKeyValueTriplets)
                    {
                        // REVIEW: Permit ASTBindingAttributes to override automatic name based matching? Or should we error on collisions?
                        propertyBindings[triplet.xName] = new PropertyBindingAttributePair(triplet.property, triplet.attribute);
                    }

                    this._PropertyMappingDictionary.Add(astNodeType, propertyBindings);
                    if (typeof(AstNamedNode).IsAssignableFrom(astNodeType))
                    {
                        this._GlobalASTNamedNodeDictionary.Add(astNodeType, new Dictionary<string, AstNamedNode>());
                    }

                    string DefaultXmlNamespace = GetDefaultXmlNamespaceCustomAttribute(astNodeType);
                    if (DefaultXmlNamespace != null)
                    {
                        this._DefaultXmlNamespacesByASTNodeType.Add(astNodeType, DefaultXmlNamespace);
                    }
                }
            }
        }
        #endregion //Initialization

        #region Core Parsing Code

        private void parseElement(XElement element, AstNode astNode, XmlIRDocumentType docType)
        {
            this._ScopeManager.Push(astNode);
            ParseAttributes(element, astNode,docType);
            ParseChildElements(element, astNode,docType);
            this._ScopeManager.Pop();

            AstNamedNode astNamedNode = astNode as AstNamedNode;
            if (astNode is IPackageRootNode)
            {
                switch(docType)
                {
                    case XmlIRDocumentType.SOURCE:
                        ((IPackageRootNode)astNode).Emit = true;
                        break;
                    case XmlIRDocumentType.INCLUDE:
                        ((IPackageRootNode)astNode).Emit = false;
                        break;
                    default:
                        throw new NotImplementedException(String.Format(Properties.Resources.ErrorUnknownDocType, docType.ToString()));
                }
            }
            if (astNamedNode != null && astNamedNode.Name != null)
            {
                // Guaranteed to work since all sub-dictionaries were initialized in PreLoadPropertyMappingsAndGlobalDictionary()
                this._GlobalASTNamedNodeDictionary[astNamedNode.GetType()][_ScopeManager.GetScopedName(astNamedNode.Name)] = astNamedNode;
            }
        }

        private void ParseAttributes(XElement element, AstNode astNode, XmlIRDocumentType docType)
        {
            foreach (XAttribute xAttribute in element.Attributes())
            {
                PropertyBindingAttributePair propertyBinding = GetPropertyBinding(astNode, xAttribute.Name, true);
                ParseChildXObject(astNode, xAttribute, xAttribute.Name, xAttribute.Value, propertyBinding == null ? null : propertyBinding.Property,docType);
            }
        }

        private void ParseChildElements(XElement element, AstNode parentASTNode, XmlIRDocumentType docType)
        {
            foreach (XElement xElement in element.Elements())
            {
                PropertyBindingAttributePair propertyBinding = GetPropertyBinding(parentASTNode, xElement.Name, false);

                AstXNameBindingAttribute BindingAttribute = propertyBinding == null ? null : propertyBinding.BindingAttribute;
                if (BindingAttribute != null && BindingAttribute.HasXPathQuery)
                {
                    BindXPathQuery(parentASTNode, xElement, propertyBinding, BindingAttribute,docType);
                }
                ParseChildXObject(parentASTNode, xElement, xElement.Name, xElement.Value, propertyBinding == null ? null : propertyBinding.Property,docType);
            }
            PropertyBindingAttributePair textPropertyBinding = GetPropertyBinding(parentASTNode, XName.Get("__self", element.Name.NamespaceName));
            AstXNameBindingAttribute textBindingAttribute = textPropertyBinding == null ? null : textPropertyBinding.BindingAttribute;
            if (textBindingAttribute != null && textBindingAttribute.HasXPathQuery)
            {
                BindXPathQuery(parentASTNode, element, textPropertyBinding, textBindingAttribute,docType);
            }
        }

        private void BindXPathQuery(AstNode parentASTNode, XElement xElement, PropertyBindingAttributePair propertyBinding, AstXNameBindingAttribute BindingAttribute, XmlIRDocumentType docType)
        {
            object EvaluationResult = xElement.XPathEvaluate(BindingAttribute.XPathQuery);

            if (IsContainerOf(typeof(IEnumerable<object>), EvaluationResult.GetType()))
            {
                foreach (object child in (IEnumerable<object>)EvaluationResult)
                {
                    XAttribute childAttribute = child as XAttribute;
                    XElement childElement = child as XElement;
                    XText childText = child as XText;
                    if (childAttribute != null)
                    {
                        ParseChildXObject(parentASTNode, childAttribute, childAttribute.Name, childAttribute.Value, propertyBinding.Property, docType);
                    }
                    else if (childElement != null)
                    {
                        ParseChildXObject(parentASTNode, childElement, childElement.Name, childElement.Value, propertyBinding.Property, docType);
                    }
                    else if (childText != null)
                    {
                        ParseChildXObject(parentASTNode, childText, null, childText.Value, propertyBinding.Property, docType);
                    }
                }
            }
            else
            {
                XAttribute xResult = new XAttribute(XName.Get("XPathEvaluationResult", this._DefaultXmlNamespace), EvaluationResult);
                ParseChildXObject(parentASTNode, xResult, xResult.Name, xResult.Value, propertyBinding.Property, docType);
            }

        }

        private void ParseChildXObject(AstNode parentASTNode, XObject xObject, XName xName, string xValue, PropertyInfo propertyToBind, XmlIRDocumentType docType)
        {
            bool mapped = false;

            if (propertyToBind != null)
            {
                // TODO: Do we need to check this cast or is it guaranteed safe?
                if (IsContainerOf(typeof(ICollection<AstNode>), propertyToBind.PropertyType) && xObject is XElement && !IsFlattenedListOf(propertyToBind.PropertyType, (XElement)xObject))
                {
                    // REVIEW: We ignore the child attributes of the ICollection-bound xObject and we process its children with the same parentASTNode and a binding preset to the ICollection
                    mapped = true;
                    foreach (XElement xElement in ((XElement)xObject).Elements())
                    {
                        if (xElement.Name != XName.Get("Annotation", GetDefaultXMLNamespace(parentASTNode)))
                        {
                            mapped = mapped && ParseChildXObjectToElement(parentASTNode, xElement, xElement.Name, xElement.Value, propertyToBind, docType);
                        }
                    }
                }
                else
                {
                    mapped = ParseChildXObjectToElement(parentASTNode, xObject, xName, xValue, propertyToBind,docType);
                }
            }

            // Property could not be found or ASTNode instance could not be created
            if (!mapped)
            {
                if (!this._UnmappedProperties.ContainsKey(xName))
                {
                    this._UnmappedProperties.Add(xName, new List<AstNode>());
                }
                this._UnmappedProperties[xName].Add(parentASTNode);
                parentASTNode.UnmappedXObjects.Add(xObject);

                if (parentASTNode is AstNamedNode && xName == XName.Get("New", GetDefaultXMLNamespace(parentASTNode)))
                {
                    NewBindingItem bindingItem = new NewBindingItem(docType, xObject, ((XElement)xObject).Attribute("Class").Value, (AstNamedNode)parentASTNode, _ScopeManager.Clone());
                    this._NewObjectItems.Add(bindingItem);
                }
            }
        }


        // TODO: Should we add a global XObject dictionary keyed by object so that we can cross-refernce back to non-ASTNode literals?
        private bool ParseChildXObjectToElement(AstNode parentASTNode, XObject xObject, XName xName, string xValue, PropertyInfo propertyToBind, XmlIRDocumentType docType)
        {
            bool mapped = false;

            object convertedLiteralValue;
            if (IfLiteralTypeTryConvert(propertyToBind.PropertyType, xValue, out convertedLiteralValue))  // GOTO: Literal Type Handling Code
            {
                BindProperty(propertyToBind, parentASTNode, convertedLiteralValue, xObject, false);
                return true;
            }
            else if (IsXObjectValueTextOnly(xObject))  // GOTO: ASTNode Reference (Lookup) Handling Code
            {
                BindingItem bindingItem = new BindingItem(propertyToBind, xObject, xValue, parentASTNode, _ScopeManager.Clone());
                if (!BindASTNodeReference(bindingItem))
                {
                    this._LateBindingItems.Add(bindingItem);
                }
                return true;
            }
            else // GOTO: ASTNode Child Element (recursive descent) Handling Code  // xObject maps to a list of known IASTNode types
            {
                // TODO: Do we need to check this cast or is it guaranteed safe?
                AstNode astNode = CreateASTNodeInstance((XElement)xObject, parentASTNode);
                if (astNode != null) // xObject maps directly to a known IASTNode type
                {
                    BindProperty(propertyToBind, parentASTNode, astNode, xObject, false);
                    // TODO: Do we need to check this cast or is it guaranteed safe?
                    parseElement((XElement)xObject, astNode, docType);
                    return true;
                }
            }
            return mapped;
        }
        #endregion // Core Parsing Code

        #region Literal Type Handling Code

        public static bool IfLiteralTypeTryConvert(Type type, string value, out object convertedValue)
        {
            if (IsContainerOf(typeof(ICollection<object>), type))
            {
                type = type.GetGenericArguments()[0];
            }

            // REVIEW: Should I be using different converters here?  XLinq doesn't appear to have them accessible.
            bool success;
            bool cBool; byte cByte; char cChar; DateTime cDateTime; decimal cDecimal; double cDouble; short cShort; int cInt;
            switch (type.FullName)
            {
                case "System.Boolean": success = Boolean.TryParse(value, out cBool); convertedValue = cBool; return success;
                case "System.Byte": success = Byte.TryParse(value, out cByte); convertedValue = cByte; return success;
                case "System.Char": success = Char.TryParse(value, out cChar); convertedValue = cChar; return success;
                case "System.DateTime": success = DateTime.TryParse(value, out cDateTime); convertedValue = cDateTime; return success;
                case "System.Decimal": success = Decimal.TryParse(value, out cDecimal); convertedValue = cDecimal; return success;
                case "System.Double": success = Double.TryParse(value, out cDouble); convertedValue = cDouble; return success;
                case "System.Int16": success = Int16.TryParse(value, out cShort); convertedValue = cShort; return success;
                case "System.Int32": success = Int32.TryParse(value, out cInt); convertedValue = cInt; return success;
                case "System.String": convertedValue = value.Clone(); return true;
                default:
                    if (type.IsEnum)
                    {
                        // TODO: Add exception handling code with friendly error
                        convertedValue = Enum.Parse(type, value, true); return true;
                    }
                    /* TODO: Message.Error("No converter available for that type */
                    convertedValue = null; return false;
            }
        }
        #endregion // Literal Type Handling Code

        #region ASTNode Reference (Lookup) Handling Code

        private bool IsXObjectValueTextOnly(XObject xObject)
        {
            XElement xElement = xObject as XElement;
            if (xElement != null)
            {
                return !(xElement.HasElements || xElement.Value.Equals(string.Empty) || xElement.HasAttributes);  // REVIEW: Without child elements, the xElement must be text only, is this true?  Should we check for empty text?
            }
            return true;  // REVIEW: Assumes that the XObject must be an XAttribute, hence text only value
        }

        private AstNode FindASTNode(Type astNodeType, string ReferenceableName)
        {
            // We do this both because it is a common case and because it provides correct binding order semantics, even though this conditional block is repeated in the foreach
            if (this._GlobalASTNamedNodeDictionary.ContainsKey(astNodeType))
            {
                Dictionary<string, AstNamedNode> astNodeBinding = this._GlobalASTNamedNodeDictionary[astNodeType];
                if (astNodeBinding.ContainsKey(ReferenceableName))
                {
                    return astNodeBinding[ReferenceableName];
                }
            }
            foreach (Type t in this._GlobalASTNamedNodeDictionary.Keys)
            {
                if (astNodeType.IsAssignableFrom(t))
                {
                    Dictionary<string, AstNamedNode> astNodeBinding = this._GlobalASTNamedNodeDictionary[t];
                    if (astNodeBinding.ContainsKey(ReferenceableName))
                    {
                        return astNodeBinding[ReferenceableName];
                    }
                }
            }
            return null;
        }

        private bool BindASTNodeReference(BindingItem item)
        {
            Type t = item.BoundProperty.PropertyType;
            if (IsContainerOf(typeof(ICollection<object>), t))
            {
                t = t.GetGenericArguments()[0];
            }

            string value = item.XValue;
            AstNode boundASTNode = null;
            AstParserScopeManager scopeManager = item.ScopeManager.Clone();

            // Walk scopes
            while (boundASTNode == null && !scopeManager.IsEmpty)
            {
                boundASTNode = FindASTNode(t, scopeManager.GetScopedName(value));
                scopeManager.Pop();
            }

            // Attempt scopeless binding
            if (boundASTNode == null)
            {
                boundASTNode = FindASTNode(t, value);
            }

            if (boundASTNode != null)
            {
                BindProperty(item.BoundProperty, item.ParentASTNode, boundASTNode, item.XObject, true);
                return true;
            }
            return false;
        }
        #endregion // ASTNode Reference (Lookup) Handling Code

        #region ASTNode Child Element (recursive descent) Handling Code

        private AstNode CreateASTNodeInstance(XElement element, AstNode parentASTNode)
        {
            Type boundASTNodeType = null;
            ConstructorInfo boundASTNodeConstructor = null;
            AstNode boundASTNode = null;
            if (this._ASTNodeTypesDictionary.TryGetValue(element.GetSchemaInfo().SchemaElement.SchemaTypeName, out boundASTNodeType))
            {
                // ASTNodes must support a default constructor to be valid
                boundASTNodeConstructor = boundASTNodeType.GetConstructor(Type.EmptyTypes);
            }

            if (boundASTNodeConstructor != null)
            {
                boundASTNode = (AstNode)boundASTNodeConstructor.Invoke(null);
                boundASTNode.ParentASTNode = parentASTNode;
            }
            return boundASTNode;
        }
        #endregion // ASTNode Child Element (recursive descent) Handling Code

        #region Utility Code
        public static bool IsContainerOf(Type tContainerBase, Type t)
        {
            Type[] baseArgs = tContainerBase.GetGenericArguments();
            Type[] checkArgs = t.GetGenericArguments();

            if (baseArgs.Length != checkArgs.Length) { return false; }
            
            for (int i = 0; i < baseArgs.Length; i++)
            {
                if (!baseArgs[i].IsAssignableFrom(checkArgs[i])) { return false; }
            }
            // We've established that the type parameter lists are compatible, so now we check to see if the container type is compatible
            // Due to the way .NET reflection handles generic types, we need to construct a candidate type with the contained parameter list
            return tContainerBase.IsAssignableFrom(t.GetGenericTypeDefinition().MakeGenericType(baseArgs));
        }

        private bool IsFlattenedListOf(Type listType, XElement xElement)
        {
            Type boundASTNodeType = null;
            this._ASTNodeTypesDictionary.TryGetValue(xElement.GetSchemaInfo().SchemaElement.SchemaTypeName, out boundASTNodeType);

            Type[] genericArgs = listType.GetGenericArguments();
            return (genericArgs.Length > 0 && genericArgs[0].IsAssignableFrom(boundASTNodeType));
        }

        private string GetDefaultXMLNamespace(AstNode astNode)
        {
            return GetDefaultXMLNamespace(astNode.GetType());
        }

        private string GetDefaultXMLNamespace(Type astNodeType)
        {
            if (typeof(AstNode).IsAssignableFrom(astNodeType))
            {
                if(this._DefaultXmlNamespacesByASTNodeType.ContainsKey(astNodeType))
                {
                    return this._DefaultXmlNamespacesByASTNodeType[astNodeType];
                }
                return this._DefaultXmlNamespace;
            }
            return  null;
        }

        private PropertyBindingAttributePair GetPropertyBinding(AstNode astNode, XName xObjectName)
        {
            return GetPropertyBinding(astNode, xObjectName, false);
        }

        private PropertyBindingAttributePair GetPropertyBinding(AstNode astNode, XName xObjectName, bool patchDefaultNamespace)
        {
            Type astNodeType = astNode.GetType();
            if (this._PropertyMappingDictionary.ContainsKey(astNodeType))
            {
                Dictionary<XName, PropertyBindingAttributePair> propertyBinding = this._PropertyMappingDictionary[astNodeType];
                if (propertyBinding.ContainsKey(xObjectName))
                {
                    return propertyBinding[xObjectName];
                }
                else if (patchDefaultNamespace && xObjectName.NamespaceName.Equals(String.Empty))
                {
                    // targetNamespace is not applied to attributes, so this code will use the DefaultXMLNamespace to perform XName matching when specified (i.e. for attributes)
                    return GetPropertyBinding(astNode, XName.Get(xObjectName.LocalName, GetDefaultXMLNamespace(astNode)), false);
                }
            }
            return null;
        }

        private void BindProperty(PropertyInfo property, AstNode parentASTNode, object value, XObject sourceXObject, bool bLateBinding)
        {
            sourceXObject.AddAnnotation(value);

            AstNode astNode = value as AstNode;
            if (astNode != null && !bLateBinding)
            {
                astNode.BoundXElement = sourceXObject;
            }

            if (property.PropertyType.IsAssignableFrom(value.GetType()))
            {
                property.SetValue(parentASTNode, value, null);
            }
            else if (IsContainerOf(typeof(ICollection<object>), property.PropertyType) && property.PropertyType.GetGenericArguments()[0].IsAssignableFrom(value.GetType()))
            {
                object collection = property.GetValue(parentASTNode, null);
                MethodInfo addMethod = collection.GetType().GetMethod("Add");
                addMethod.Invoke(collection, new object[] { value });
            }
            else
            {
                // TODO: Message.Error("No Binding Mechanism Worked");
            }
        }

        private void ProcessLateBindingQueue()
        {
            while (this._LateBindingItems.Count > 0)
            {
                List<BindingItem> newLateBindingItems = new List<BindingItem>();

                foreach (BindingItem item in this._LateBindingItems)
                {
                    if (!this.BindASTNodeReference(item))
                    {
                        newLateBindingItems.Add(item);
                    }
                }

                if (this._LateBindingItems.Count == newLateBindingItems.Count)
                {
                    // TODO: Message.Error("Late Binding Queue Stalled without Progress");
                    return;
                }
                this._LateBindingItems = newLateBindingItems;
            }
        }

        private void ProcessNewObjectQueue(XmlIR xmlIR)
        {
            foreach (NewBindingItem item in this._NewObjectItems)
            {
                FindASTNodeReferenceForNewObject(xmlIR, item);
            }
        }

        private bool FindASTNodeReferenceForNewObject(XmlIR xmlIR, NewBindingItem item)
        {
            Type t = item.node.GetType();
            if (IsContainerOf(typeof(ICollection<object>), t))
            {
                t = t.GetGenericArguments()[0];
            }

            string value = item.XValue;
            AstNode boundASTNode = null;
            AstParserScopeManager scopeManager = item.ScopeManager.Clone();

            // Walk scopes
            while (boundASTNode == null && !scopeManager.IsEmpty)
            {
                boundASTNode = FindASTNode(t, scopeManager.GetScopedName(value));
                scopeManager.Pop();
            }

            // Attempt scopeless binding
            if (boundASTNode == null)
            {
                boundASTNode = FindASTNode(t, value);
            }

            if (boundASTNode != null)
            {
                XElement xElement = new XElement((XElement)boundASTNode.BoundXElement);
                XmlSchemaObject schemaObject = ((XElement)boundASTNode.BoundXElement).GetSchemaInfo().SchemaElement;
                xElement.SetAttributeValue("AsClassOnly", false);
                xElement.SetAttributeValue("Name", item.node.Name);
                Dictionary<string, string> dictParams = new Dictionary<string, string>();
                foreach (XElement param in xElement.Elements(XName.Get("Argument", GetDefaultXMLNamespace(boundASTNode))))
                {
                    string sParam = param.Attribute("Name").Value;
                    XAttribute xAttribute = (item.node.BoundXElement as XElement).
                        Element(XName.Get("New", GetDefaultXMLNamespace(item.node))).Attribute(sParam);
                    if (xAttribute != null)
                    {
                        dictParams.Add("{argument(" + sParam + ")}", xAttribute.Value);
                    }
                    else
                    {
                        dictParams.Add("{argument(" + sParam + ")}", param.Attribute("DefaultValue").Value);
                    }
                }
                ReplaceParameters(xElement, dictParams);
                xmlIR.ValidateXElement(schemaObject, xElement);
                parseElement(xElement, item.node, item.docType);
                return true;
            }
            return false;
        }

        private void ReplaceParameters(XElement xElement, Dictionary<string, string> dictParams)
        {
            foreach (XNode node in xElement.Nodes())
            {
                if (node.NodeType == XmlNodeType.Text)
                {
                    XText text = (XText)node;
                    foreach (KeyValuePair<string, string> keyValue in dictParams)
                    {
                        if (text.Value.Contains(keyValue.Key))
                        {
                            text.Value = text.Value.Replace(keyValue.Key, keyValue.Value);
                        }
                    }
                }

            }
            foreach (XAttribute attribute in xElement.Attributes())
            {
                foreach (KeyValuePair<string, string> keyValue in dictParams)
                {
                    if (attribute.Value.Contains(keyValue.Key))
                    {
                        attribute.SetValue(attribute.Value.Replace(keyValue.Key, keyValue.Value));
                    }
                }
            }
            foreach (XElement element in xElement.Elements())
            {
                ReplaceParameters(element, dictParams);
            }
        }
        #endregion // Utility Code
    }


    class BindingItem
    {
        public readonly PropertyInfo BoundProperty;
        public readonly XObject XObject;
        public readonly string XValue;
        public readonly AstNode ParentASTNode;
        public readonly AstParserScopeManager ScopeManager;

        public BindingItem(PropertyInfo BoundProperty, XObject XObject, string XValue, AstNode ParentASTNode, AstParserScopeManager scopeManager)
        {
            this.BoundProperty = BoundProperty;
            this.XObject = XObject;
            this.XValue = XValue;
            this.ParentASTNode = ParentASTNode;
            this.ScopeManager = scopeManager;
        }
    }

    class NewBindingItem
    {
        public readonly XmlIRDocumentType docType;
        public readonly XObject XObject;
        public readonly string XValue;
        public readonly AstNamedNode node;
        public readonly AstParserScopeManager ScopeManager;

        public NewBindingItem(XmlIRDocumentType docType, XObject XObject, string XValue, AstNamedNode ParentASTNode, AstParserScopeManager scopeManager)
        {
            this.docType = docType;
            this.XObject = XObject;
            this.XValue = XValue;
            this.node = ParentASTNode;
            this.ScopeManager = scopeManager;
        }
    }

    class PropertyBindingAttributePair
    {
        public readonly PropertyInfo Property;
        public readonly AstXNameBindingAttribute BindingAttribute;

        public PropertyBindingAttributePair(PropertyInfo Property, AstXNameBindingAttribute BindingAttribute)
        {
            this.Property = Property;
            this.BindingAttribute = BindingAttribute;
        }
    }
}
