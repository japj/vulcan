using System.Xml.Linq;
using AstFramework.Engine.Binding;
using AstFramework.Model;

namespace AstFramework.Engine
{
    public class ParserContext
    {
        public ITemplate Template { get; private set; }

        public XElement XElement { get; private set; }

        public IFrameworkItem FrameworkItem { get; private set; }

        public BimlFile BimlFile { get; private set; }

        public UnboundReferences UnboundReferences { get; private set; }

        public LanguageSettings LanguageSettings { get; private set; }

        public bool IsInTemplate 
        {
            get { return Template != null; }
        }

        public ParserContext(XElement element, IFrameworkItem parentFrameworkItem, ITemplate currentTemplate, BimlFile bimlFile, UnboundReferences unboundReferences, LanguageSettings languageSettings)
        {
            XElement = element;
            FrameworkItem = parentFrameworkItem;
            Template = currentTemplate;
            BimlFile = bimlFile;
            UnboundReferences = unboundReferences;
            LanguageSettings = languageSettings;
        }
    }
}
