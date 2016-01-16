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
    public class IndexEmitter : TableModifierEmitter
    {
        public IndexEmitter(string tableName, XPathNavigator tableNavigator, Packages.VulcanPackage vulcanPackage)
            :base(
            tableName,
            tableNavigator,
            vulcanPackage
            )
        {
        }

        public override void Emit(TextWriter outputWriter)
        {
            TemplateEmitter te = new TemplateEmitter("CreateIndex", VulcanPackage, null);

            foreach (XPathNavigator nav in TableNavigator.Select("rc:Indexes/rc:Index", VulcanPackage.VulcanConfig.NamespaceManager))
            {
                string indexName = nav.SelectSingleNode("@Name") == null ? null : nav.SelectSingleNode("@Name").Value;

                string unique = nav.SelectSingleNode("@Unique", VulcanPackage.VulcanConfig.NamespaceManager).ValueAsBoolean
                                                     ? "UNIQUE"
                                                     : "";

                string clustered =  nav.SelectSingleNode("@Clustered", VulcanPackage.VulcanConfig.NamespaceManager).ValueAsBoolean
                                                     ? "CLUSTERED"
                                                     : "NONCLUSTERED";

                string dropExisting = nav.SelectSingleNode("@DropExisting", VulcanPackage.VulcanConfig.NamespaceManager).ValueAsBoolean
                                                     ? "DROP_EXISTING = ON"
                                                     : "DROP_EXISTING = OFF";

                string ignoreDupKey = nav.SelectSingleNode("@IgnoreDupKey", VulcanPackage.VulcanConfig.NamespaceManager).ValueAsBoolean
                                                     ? "IGNORE_DUP_KEY = ON"
                                                     : "IGNORE_DUP_KEY = OFF";

                string online = nav.SelectSingleNode("@Online", VulcanPackage.VulcanConfig.NamespaceManager).ValueAsBoolean
                                                     ? "ONLINE = ON"
                                                     : "ONLINE = OFF";

                string padIndex = nav.SelectSingleNode("@PadIndex", VulcanPackage.VulcanConfig.NamespaceManager).ValueAsBoolean
                                                     ? "PAD_INDEX = ON"
                                                     : "PAD_INDEX = OFF";

                string sortInTempdb = nav.SelectSingleNode("@SortInTempdb", VulcanPackage.VulcanConfig.NamespaceManager).ValueAsBoolean
                                                     ? "SORT_IN_TEMPDB = ON"
                                                     : "SORT_IN_TEMPDB = OFF";

                StringBuilder columnsBuilder = new StringBuilder();
                StringBuilder indexNameBuilder = new StringBuilder("IX_");
                indexNameBuilder.Append(TableName);

                outputWriter.Write("\n");
                foreach (XPathNavigator columnNav in nav.Select("rc:Column", VulcanPackage.VulcanConfig.NamespaceManager))
                {
                    // This should use a Column Emitter which has callbacks and such...
                    //string componentName = nav.SelectSingleNode("@Name", VulcanPackage.NamespaceManager).Value;
                    string name = columnNav.SelectSingleNode("@Name", VulcanPackage.VulcanConfig.NamespaceManager).Value;
                    string sortOrder = columnNav.SelectSingleNode("@SortOrder", VulcanPackage.VulcanConfig.NamespaceManager).Value;

                    columnsBuilder.AppendFormat(
                        "[{0}] {1},\n",
                        name,
                        sortOrder
                    );
                    indexNameBuilder.AppendFormat("_{0}", name);
                }
                columnsBuilder.Replace(",", "", columnsBuilder.Length - 2, 1);
                // Remove any dots which happen to show up in the name
                indexNameBuilder.Replace(".", "_");

                if (!string.IsNullOrEmpty(indexName))
                {
                    indexNameBuilder = new StringBuilder(indexName);  // Throw out the old name :) We got ourselves a better one.
                }

                te.SetParameters(
                    unique, 
                    clustered,
                    indexNameBuilder.ToString(),
                    TableName,
                    columnsBuilder.ToString(),
                    String.Format(System.Globalization.CultureInfo.InvariantCulture,"{0},\n{1},\n{2},\n{3},\n{4}", padIndex, sortInTempdb, dropExisting, ignoreDupKey, online)
                    );

                te.Emit(outputWriter);
                outputWriter.Flush();

                
            }
        }
    }
}
