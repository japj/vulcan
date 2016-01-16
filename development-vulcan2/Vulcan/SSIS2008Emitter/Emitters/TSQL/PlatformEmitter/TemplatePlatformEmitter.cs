using System;
using System.Collections.Generic;
using System.Text;

using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.Emitters.TSQL;

namespace Ssis2008Emitter.Emitters.TSQL.PlatformEmitter
{
    public class TemplatePlatformEmitter
    {
        private string _templateName;
        private string[] _templateParameters;
        private static TemplateManager _templateManager = new TemplateManager();

        public TemplatePlatformEmitter(string templateName, params string[] parameters)
        {
            _templateName = templateName;
            _templateParameters = parameters;
        }

        public string Emit(LogicalObject obj)
        {
            Template template = _templateManager[_templateName];

            if (_templateParameters == null)
            {
                return template.Data.Trim();
            }
            else
            {
                return String.Format(System.Globalization.CultureInfo.InvariantCulture,template.Data, _templateParameters).Trim();
            }
        }

        public static TemplateManager TemplateManager
        {
            get { return _templateManager; }
            set { _templateManager = value; }
        }
    }
}
