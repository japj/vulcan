using System.Collections.Generic;

namespace AstLowerer.TSqlEmitter
{
    public class Template
    {
        public Dictionary<string, int> MapDictionary { get; private set; }

        public string Name { get; private set; }

        public string TemplateType { get; private set; }

        public string Data { get; private set; }

        public Template(string name, string type, string data)
        {
            MapDictionary = new Dictionary<string, int>();
            Name = name;
            TemplateType = type;
            Data = data;
        }
    }
}