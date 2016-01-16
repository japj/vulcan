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
    public class InsertDefaultValuesEmitter : TableModifierEmitter
    {
        private TableHelper _tableHelper;
        public InsertDefaultValuesEmitter(string tableName, XPathNavigator tableNavigator, TableHelper tableHelper,Packages.VulcanPackage vulcanPackage)
            : base(
            tableName,
            tableNavigator,
            vulcanPackage
            )
        {
            _tableHelper = tableHelper;
        }

        public override void Emit(TextWriter outputWriter)
        {

            if(TableNavigator.Select("rc:DefaultValues/rc:Value",VulcanPackage.VulcanConfig.NamespaceManager).Count == 0)
            {
                outputWriter.Flush();
                return;
            }

            StringBuilder columnBuilder = new StringBuilder();

            bool containsIdentities = _tableHelper.KeyColumnType == KeyColumnType.Identity;
            foreach (XPathNavigator nav in TableNavigator.Select("rc:Columns/rc:Column", VulcanPackage.VulcanConfig.NamespaceManager))
            {
                /* Build Column List */
                string columnName = nav.SelectSingleNode("@Name", VulcanPackage.VulcanConfig.NamespaceManager).Value;

                columnBuilder.AppendFormat(
                     "[{0}],",
                     columnName
                 );
            }

            columnBuilder.Remove(columnBuilder.Length - 1, 1);

            if (containsIdentities)
            {
                outputWriter.Write("\n");
                outputWriter.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture,"\nSET IDENTITY_INSERT {0} ON\n", TableName));
            }
            TemplateEmitter te = new TemplateEmitter("InsertDefaultValues",VulcanPackage,null);

            outputWriter.Write("\n");
            foreach (XPathNavigator nav in TableNavigator.Select("rc:DefaultValues/rc:Value", VulcanPackage.VulcanConfig.NamespaceManager))
            {
                te.SetParameters(TableName, columnBuilder.ToString(),nav.Value);
                te.Emit(outputWriter);
                outputWriter.Write("\n"); ;
            }

            if (containsIdentities)
            {
                outputWriter.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture,"\nSET IDENTITY_INSERT {0} OFF", TableName));
            }

            outputWriter.Write("\nGO\n");
            outputWriter.Flush();
        }

        public static string GetCheckAndInsertProcedureName(string tableName)
        {
            return
                Resources.SPPrefix+Resources.Seperator
                + Resources.CheckAndInsert
                + tableName;
        }
    }
}
