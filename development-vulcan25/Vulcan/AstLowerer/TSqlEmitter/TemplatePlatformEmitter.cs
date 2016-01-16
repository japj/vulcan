using System;
using System.Collections.Generic;
using System.Globalization;

namespace AstLowerer.TSqlEmitter
{
    public class TemplatePlatformEmitter
    {
        private readonly string _templateName;
        private string[] _templateParameters;
        private static TemplateManager _templateManager = new TemplateManager();
        private readonly Template _template;

        public TemplatePlatformEmitter(string templateName, params string[] parameters)
        {
            _templateName = templateName;
            _templateParameters = parameters;
        }

        public TemplatePlatformEmitter(string templateName)
        {
            _templateName = templateName;
            _template = _templateManager[_templateName];
            if (_template.MapDictionary != null && _template.MapDictionary.Count > 0)
            {
                _templateParameters = new string[_template.MapDictionary.Count];
            }
        }

        public void Map(string key, string value)
        {
            if (_template.MapDictionary != null && _template.MapDictionary.ContainsKey(key))
            {
                _templateParameters[_template.MapDictionary[key]] = value;
            }
            else
            {
                throw new KeyNotFoundException(String.Format(CultureInfo.InvariantCulture, "Key {0} not found in Template {1}", key, _template.Name));
            }
        }

        public string Emit()
        {
            Template template = _templateManager[_templateName];

            if (_templateParameters == null)
            {
                return template.Data.Trim();
            }

            return String.Format(System.Globalization.CultureInfo.InvariantCulture, template.Data, _templateParameters).Trim();
        }

        public static TemplateManager TemplateManager
        {
            get { return _templateManager; }
            set { _templateManager = value; }
        }
    }
}