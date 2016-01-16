/*
 * Microsoft Public License (Ms-PL)
 * 
 * This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.
 * 
 * 1. Definitions
 * The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
 * A "contribution" is the original software, or any additions or changes to the software.
 * A "contributor" is any person that distributes its contribution under this license.
 * "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 * 
 * 2. Grant of Rights
 * (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 * (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 * 
 * 3. Conditions and Limitations
 * (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
 * (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
 * (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
 * (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
 * (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement. 
 */

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
