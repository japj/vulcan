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
    public abstract class Emitter
    {
        private Packages.VulcanPackage _vulcanPackage;

        protected Emitter(Packages.VulcanPackage vulcanPackage)
        {
            _vulcanPackage = vulcanPackage;
        }

        public abstract void Emit(System.IO.TextWriter outputWriter);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#")]
        public void Emit(out string stringToEmitTo)
        {
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            Emit(sw);

            ms.Position = 0;
            
            StreamReader sr = new StreamReader(ms);
            stringToEmitTo = sr.ReadToEnd();

            sr.Close();
            sw.Close();
        }

        public void Emit(string fileName, bool append)
        {
            StreamWriter sw = new StreamWriter(fileName, append);
            Emit(sw);
            sw.Close();
        }
        public void Emit(Stream outputStream)
        {
            StreamWriter sw = new StreamWriter(outputStream);
            Emit(sw);
            sw.Dispose();
        }

        public Packages.VulcanPackage VulcanPackage
        {
            get
            {
                return _vulcanPackage;
            }
        }
    }

    public abstract class TableModifierEmitter : Emitter
    {
        private string _tableName;
        private XPathNavigator _tableNavigator;

        public TableModifierEmitter(string tableName, XPathNavigator tableNavigator, Packages.VulcanPackage vulcanPackage)
            :
            base(
            vulcanPackage
            )
        {
            this._tableName = tableName;
            this._tableNavigator = tableNavigator;
        }

        public string TableName
        {
            get
            {
                return _tableName;
            }
        }

        public XPathNavigator TableNavigator
        {
            get
            {
                return this._tableNavigator;
            }
        }
    }
}
