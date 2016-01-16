using System;
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
    public class StoredProcEmitter : Emitter
    {
        private string _procName;
        private  string _body;

        private List<string> _columnList;

        public StoredProcEmitter(string procName, string body, Packages.VulcanPackage vulcanPackage)
            : base(
       vulcanPackage
       )
        {
            this._procName = procName;
            this._body = body;
            this._columnList = new List<string>();
        }

        public void AddColumn(string columnName, string defaultValue, string type, bool isOutput)
        {
            string output = isOutput
                            ? "OUTPUT"
                            : "";

            if (!String.IsNullOrEmpty(defaultValue))
            {
                defaultValue = "= " + defaultValue;
            }
            _columnList.Add(String.Format(System.Globalization.CultureInfo.InvariantCulture,"@{0} {1} {2} {3},", columnName, type, defaultValue,output));
        }

        public override void Emit(TextWriter outputWriter)
        {
            StringBuilder columnBuilder = new StringBuilder();


            foreach (string s in _columnList)
            {
                columnBuilder.AppendLine(s);
            }
            columnBuilder.Replace(",", "", columnBuilder.Length - 3,3);

            TemplateEmitter te = new TemplateEmitter("StoredProc", VulcanPackage, _procName, columnBuilder.ToString(), _body);
            te.Emit(outputWriter);
        }

    }
}
