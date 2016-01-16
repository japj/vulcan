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
    public class CheckAndInsertSPEmitter : Emitter
    {
        private string _tableName;
        private XPathNavigator _tableNavigator;
        private TableHelper _tableHelper;

        public CheckAndInsertSPEmitter(string tableName, XPathNavigator tableNavigator, TableHelper tableHelper, Packages.VulcanPackage vulcanPackage)
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
            if (outputWriter != null)
            {
                string identityColumnName = _tableHelper.KeyColumn.Name;
                StringBuilder spParametersBuilder = new StringBuilder();
                StringBuilder execArgumentsBuilder = new StringBuilder();
                StringBuilder uniqueColumnsBuilder = new StringBuilder();

                outputWriter.Write("\n");
                foreach (XPathNavigator nav in _tableNavigator.Select("rc:Columns/rc:Column", VulcanPackage.VulcanConfig.NamespaceManager))
                {
                    /* Build Argument List */
                    string columnName = nav.SelectSingleNode("@Name", VulcanPackage.VulcanConfig.NamespaceManager).Value;
                    string columnType = nav.SelectSingleNode("@Type", VulcanPackage.VulcanConfig.NamespaceManager).Value;
                    bool isKey = false;
                    
                    if (
                        String.Compare(_tableHelper.KeyColumn.Name, columnName, StringComparison.InvariantCultureIgnoreCase) == 0
                        )
                    {
                        isKey = true;
                    }

                    spParametersBuilder.AppendFormat(
                        "\t@{0} {1} {2},\n",
                        columnName,
                        columnType,
                        isKey ? " OUTPUT" : ""
                    );

                    /* Re-Use this loop to build the INSERT statement */

                    execArgumentsBuilder.AppendFormat(
                    "\t\t@{0}{1},\n",
                    columnName,
                    isKey ? " OUTPUT" : ""
                    );
                }


                foreach (XPathNavigator nav in _tableNavigator.Select("rc:CheckAndInsertUniqueColumn", VulcanPackage.VulcanConfig.NamespaceManager))
                {
                    uniqueColumnsBuilder.AppendFormat(
                        "{0} = @{0} AND ",
                        nav.Value
                    );
                }

                // If its not > 0 then we have a problem, this stored proc should never get created.
                if (uniqueColumnsBuilder.Length <= 0)
                {
                    outputWriter.Flush();
                    return;
                }
                //remove trailing commas or newlines or ANDS
                spParametersBuilder.Replace(",", "", spParametersBuilder.Length - 2, 1);
                execArgumentsBuilder.Replace(",", "", execArgumentsBuilder.Length - 2, 1);
                uniqueColumnsBuilder.Replace("AND", "", uniqueColumnsBuilder.Length - 4, 3);

                outputWriter.Write("\n");
                TemplateEmitter te =
                        new TemplateEmitter(
                        "CheckAndInsertSP",
                        VulcanPackage,
                        _tableName,
                        CheckAndInsertSPEmitter.GetCheckAndInsertProcedureName(_tableName),
                        spParametersBuilder.ToString(),
                        execArgumentsBuilder.ToString(),
                        identityColumnName,
                        uniqueColumnsBuilder.ToString(),
                        InsertSPEmitter.GetInsertProcedureName(_tableName),
                        _tableHelper.KeyColumn.Properties["Type"]
                        );

                te.Emit(outputWriter);
                outputWriter.Write("\n");
                outputWriter.Flush();
            } // end if outputWriter != null
        }

        public static string GetCheckAndInsertProcedureName(string tableName)
        {
            return
                Resources.SPPrefix + Resources.Seperator
                + Resources.CheckAndInsert
                + tableName;
        }
    }
}
