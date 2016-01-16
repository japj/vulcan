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
