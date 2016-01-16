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
    public class InsertSPEmitter : Emitter
    {
        private string _tableName;
        private XPathNavigator _tableNavigator;
        private TableHelper _tableHelper;

        public InsertSPEmitter(string tableName, XPathNavigator tableNavigator, TableHelper tableHelper, Packages.VulcanPackage vulcanPackage)
            : base(
            vulcanPackage
            )
        {
            this._tableName = tableName;
            this._tableNavigator = tableNavigator;
            this._tableHelper = tableHelper;
        }

        public override void Emit(TextWriter outputWriter)
        {

            string identityColumnName = "";
            StringBuilder spParametersBuilder = new StringBuilder();
            StringBuilder insertIntoBuilder = new StringBuilder();
            StringBuilder valuesBuilder = new StringBuilder();
            outputWriter.Write("\n");
            foreach (XPathNavigator nav in _tableNavigator.Select("rc:Columns/rc:Column", VulcanPackage.VulcanConfig.NamespaceManager))
            {
                /* Build Argument List */
                string columnName = nav.SelectSingleNode("@Name", VulcanPackage.VulcanConfig.NamespaceManager).Value;
                string columnType = nav.SelectSingleNode("@Type", VulcanPackage.VulcanConfig.NamespaceManager).Value;
                bool isIdentity = _tableHelper.IsIdentityColumn(columnName);
                bool isKey = false;

                if (
                    String.Compare(_tableHelper.KeyColumn.Name, columnName, StringComparison.InvariantCultureIgnoreCase) == 0
                    )
                {
                    isKey = true;
                }


                if(isIdentity)
                {
                    identityColumnName = columnName;
                }

                spParametersBuilder.AppendFormat(
                    "\t@{0} {1} {2},\n",
                    columnName,
                    columnType,
                    isKey ? "OUTPUT" : ""
                );

                /* Re-Use this loop to build the INSERT statement */

                if (!isIdentity)
                {
                    insertIntoBuilder.AppendFormat(
                    "\t[{0}],\n",
                    columnName
                    );

                    valuesBuilder.AppendFormat(
                    "\t@{0},\n",
                    columnName
                    );
                }
            }


            spParametersBuilder.Replace(",", "", spParametersBuilder.Length - 2, 1);
            insertIntoBuilder.Replace(",", "", insertIntoBuilder.Length - 2, 1);
            valuesBuilder.Replace(",", "", valuesBuilder.Length - 2, 1);


            string scopeIdentity = String.Empty;
            
            if(!String.IsNullOrEmpty(identityColumnName))
            {
             scopeIdentity =  String.Format("IF @Exception = 0\nSET @{0} = SCOPE_IDENTITY()", identityColumnName);
            }

            TemplateEmitter te =
                    new TemplateEmitter(
                    "InsertSP",
                    VulcanPackage,
                    _tableName,
                    GetInsertProcedureName(_tableName),
                    spParametersBuilder.ToString(),
                    insertIntoBuilder.ToString(),
                    valuesBuilder.ToString(),
                    scopeIdentity 
                    );

            te.Emit(outputWriter);
            outputWriter.Flush();
        }

        public static string GetInsertProcedureName(string tableName)
        {
                return
                    Resources.SPPrefix 
                    + Resources.Seperator
                    + Resources.Insert
                    + tableName;
        }
    }
}
