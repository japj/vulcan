using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Text.RegularExpressions;


using Vulcan.Common;
using Vulcan.Common.Templates;
using Vulcan.Properties;

using DTS = Microsoft.SqlServer.Dts.Runtime;

namespace Vulcan.Emitters
{

    public class TemplateEmitter : Emitter
    {
        private Template _template;
        private string[] _templateParameters;

        public TemplateEmitter(Template template)
            :
            base(
            null
            )
        {
            this._template = template;
            if (template.MapDictionary.Keys.Count > 0)
            {
                this._templateParameters = new string[template.MapDictionary.Count];
            }
        }

        public TemplateEmitter(String templateName, Packages.VulcanPackage vulcanPackage, params string[] parameters)
            :base(
            vulcanPackage
            )
        {
            this._template = vulcanPackage.TemplateManager[templateName];
            this.SetParameters(parameters);
        }

        public void SetNamedParameter(string name, string value)
        {
            if (_template.MapDictionary.ContainsKey(name))
            {
                this._templateParameters[_template.MapDictionary[name]] = value;
            }
            else
            {
                Message.Trace(Severity.Error,"Named parameter {0} does not exist.",name);
            }
        }

        public void SetParameters(params string[] parameters)
        {
            this._templateParameters = parameters;
        }

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
    }
}
