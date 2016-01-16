using AstFramework.Model;

namespace VulcanEngine.IR.Ast.Transformation
{
    public partial class AstXmlSourceNode
    {
        public AstXmlSourceNode(IFrameworkItem parentAstNode) : base(parentAstNode)
        {
            InitializeAstNode();
        }

        /*
        public override VulcanCollection<AstNamedNode> RegisterFullyInitialized()
        {
            var newAstNamedNodes = new VulcanCollection<AstNamedNode>(base.RegisterFullyInitialized());

            foreach (string outputPathName in GetOutputPathNames())
            {
                var outputPath = new AstDataflowOutputPathNode { Name = outputPathName, SsisName = outputPathName, ParentAstNode = this };
                outputPath.SetCalculatedScopedName();
                newAstNamedNodes.Add(outputPath);
            }
            return newAstNamedNodes;
        }

        private List<string> GetOutputPathNames()
        {
            List<string> outputPathNames = new List<string>();
            
            var nameStore = new Dictionary<string, List<XmlSchemaElement>>();
            using (XmlReader reader = XmlReader.Create(XmlSchemaDefinition))
            {
                XmlSchema schema = XmlSchema.Read(reader, ValidationEventHandler);
                XmlSchemaSet schemaSet = new XmlSchemaSet();
                schemaSet.Add(schema);
                schemaSet.Compile();
                foreach (XmlSchemaObject item in schema.Items)
                {
                    var complexType = item as XmlSchemaComplexType;
                    if (complexType != null)
                    {
                        var particleGroup = complexType.Particle as XmlSchemaGroupBase;
                        if (particleGroup != null)
                        {
                            foreach (XmlSchemaObject groupItem in particleGroup.Items)
                            {
                                XmlSchemaElement element = groupItem as XmlSchemaElement;
                                if (element != null)
                                {
                                    if (!nameStore.ContainsKey(element.Name))
                                    {
                                        nameStore.Add(element.Name, new List<XmlSchemaElement>());
                                    }
                                    nameStore[element.Name].Add(element);
                                }
                            }
                        }
                    }
                }
                foreach (var store in nameStore)
                {
                    if (store.Value.Count == 1)
                    {
                        outputPathNames.Add(store.Value[0].Name);
                    }
                    else
                    {
                        foreach (var element in store.Value)
                        {
                            outputPathNames.Add(GetHierchicalSchemaElementName(element));
                        }
                    }
                }
            }
            return outputPathNames;
        }

        private string GetHierchicalSchemaElementName(XmlSchemaElement element)
        {
            var builder = new StringBuilder(element.Name);

            XmlSchemaObject currentAncestor = element.Parent;
            while (currentAncestor != null)
            {
                var currentAncestorElement = currentAncestor as XmlSchemaElement;
                if (currentAncestorElement != null)
                {
                    builder.Insert(0, "_");
                    builder.Insert(0, currentAncestorElement.Name);
                }
                currentAncestor = currentAncestor.Parent;
            }
            return builder.ToString();
        }

        protected void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    //_message.Trace(Severity.Warning, Resources.ErrorXmlValidation, e.Message);
                    //_message.Trace(Severity.Error, Resources.ErrorXmlValidation, e.Message);
                    break;
                case XmlSeverityType.Warning:
                    //_message.Trace(Severity.Warning, Resources.WarningXmlValidation, e.Message);
                    break;
            }
        }
        */
    }
}
