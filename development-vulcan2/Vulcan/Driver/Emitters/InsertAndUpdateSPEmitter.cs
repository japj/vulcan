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
    public class InsertAndUpdateSPEmitter : Emitter
    {
        private string _tableName;
        private XPathNavigator _tableNavigator;
        private TableHelper _tableHelper;

        public InsertAndUpdateSPEmitter(string tableName, XPathNavigator tableNavigator, TableHelper tableHelper, Packages.VulcanPackage vulcanPackage)
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


                foreach (XPathNavigator nav in _tableNavigator.Select("rc:InsertOrUpdateUniqueColumn", VulcanPackage.VulcanConfig.NamespaceManager))
                {
                    uniqueColumnsBuilder.AppendFormat(
                        "{0} = @{0} AND ",
                        nav.Value
                    );
                }

                StringBuilder updateParameterBuilder = new StringBuilder();
                foreach (Column c in _tableHelper.Columns.Values)
                {
                    if (!c.Name.Equals(_tableHelper.KeyColumn.Name))
                    {
                        updateParameterBuilder.AppendFormat(
                            "{0} = @{0}, ",
                            c.Name
                        );
                    }
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
                updateParameterBuilder.Replace(",", "", updateParameterBuilder.Length - 2, 1);

                outputWriter.Write("\n");
                TemplateEmitter te =
                        new TemplateEmitter(
                        "InsertOrUpdateSP",
                        VulcanPackage,
                        InsertAndUpdateSPEmitter.GetInsertAndUpdateProcedureName(_tableName),
                        _tableName,
                        spParametersBuilder.ToString(),
                        identityColumnName,
                        uniqueColumnsBuilder.ToString(),
                        InsertSPEmitter.GetInsertProcedureName(_tableName),
                        execArgumentsBuilder.ToString(),
                        updateParameterBuilder.ToString(),
                        _tableHelper.KeyColumn.Properties["Type"]
                        );

                te.Emit(outputWriter);
                outputWriter.Write("\n");
                outputWriter.Flush();
            } // end if outputWriter != null
        }

        public static string GetInsertAndUpdateProcedureName(string tableName)
        {
            return
                Resources.SPPrefix + Resources.Seperator
                + Resources.CheckAndInsert 
                + Resources.Update
                + tableName;
        }
    }
}
