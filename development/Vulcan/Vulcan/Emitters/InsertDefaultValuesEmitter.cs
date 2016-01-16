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
