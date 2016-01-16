using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Text.RegularExpressions;

using VulcanEngine.Common;

namespace Ssis2008Emitter.Emitters.TSQL
{
    public class TemplateEmitter : TextEmitter.TextEmitter
    {
        private Template _template;
        private string[] _templateParameters;

        public TemplateEmitter(Template template)
        {
            this._template = template;
            if (template.MapDictionary.Keys.Count > 0)
            {
                this._templateParameters = new string[template.MapDictionary.Count];
            }
        }

        public void SetNamedParameter(string name, string value)
        {
            if (_template.MapDictionary.ContainsKey(name))
            {
                this._templateParameters[_template.MapDictionary[name]] = value;
            }
            else
            {
                MessageEngine.Global.Trace(Severity.Error,"Named parameter {0} does not exist.",name);
            }
        }

        public void SetParameters(params string[] parameters)
        {
            this._templateParameters = parameters;
        }

        /*
        public override void Emit(TextWriter outputWriter)
        {
            if (_templateParameters == null)
            {
                outputWriter.Write(_template.Data.Trim());
            }
            else
            {
                outputWriter.Write(String.Format(_template.Data, _templateParameters).Trim());
            }
            outputWriter.Flush();
        }
        */ 
    }
}
