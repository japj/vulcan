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
    public class SelectEmitter : Emitter
    {
        string tableName;
        string columns;
        string where;

        public SelectEmitter(Packages.VulcanPackage vulcanPackage, string tableName, string columns)
            :
         this(vulcanPackage,tableName,columns,null)
        {
        }

        public SelectEmitter(Packages.VulcanPackage vulcanPackage, string tableName, string columns, string where)
            :
        base(
            vulcanPackage
            )
        {
            this.tableName = tableName;
            this.columns = columns;
            this.where = where;
        }

        public override void Emit(TextWriter tw)
        {
            string sqlStatement;

            TemplateEmitter sqlTemplate = new TemplateEmitter("SimpleSelect", VulcanPackage, columns, tableName);
            sqlTemplate.Emit(out sqlStatement);

            if (where != null)
            {
                string whereStatement;
                TemplateEmitter whereTemplate = new TemplateEmitter("SimpleWhere",VulcanPackage,where);

                whereTemplate.Emit(out whereStatement);
                sqlStatement += whereStatement;
            }

            tw.WriteLine(sqlStatement);
            tw.Flush();
        }

    }
}
