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
    public class TableEmitterEx : Emitter
    {
        private TableHelper _tableHelper;
        public TableEmitterEx(TableHelper tableHelper, Packages.VulcanPackage vulcanPackage)
            :
            base(vulcanPackage)
        {
            this._tableHelper = tableHelper;
        }

        public override void Emit(System.IO.TextWriter outputWriter)
        {
            StringBuilder columnsBuilder   = new StringBuilder();
            StringBuilder constraintBuilder = new StringBuilder();

            foreach (Column c in _tableHelper.Columns.Values)
            {
                string columnName = c.Properties["Name"];
                string columnType = c.Properties["Type"];
                bool isNullable = c.Properties.ContainsKey("IsNullable")
                                        ? Convert.ToBoolean(c.Properties["IsNullable"])
                                        : false;

                bool isIdentity = _tableHelper.IsIdentityColumn(columnName);

                columnsBuilder.AppendFormat(
                    "\t[{0}] {1}{2}{3},\n",
                    columnName,
                    columnType,
                    isIdentity ? " IDENTITY(1,1)" : "",
                    isNullable ? "" : " NOT NULL"
                );
            }

            StringBuilder simpleConstraintBuilder = new StringBuilder();

            // Primary Key Constraints
            int constraintCount = 0;
            foreach (Constraint c in _tableHelper.Constraints)
            {
                if (c is SimpleConstraint)
                {
                    string cString;
                    Message.Trace(Severity.Debug,"Found Constraint {0}", c.Name);
                    ConstraintEmitter ce = new ConstraintEmitter(VulcanPackage, c, _tableHelper);
                    ce.Emit(out cString);
                    simpleConstraintBuilder.AppendFormat("{0},\n", cString);
                    constraintCount++;
                }
            }

            if (constraintCount > 0)
            {
                simpleConstraintBuilder.Replace(",", "", simpleConstraintBuilder.Length - 2, 2);
            }

            TemplateEmitter te = new TemplateEmitter(
                "CreateTable", 
                VulcanPackage,
                _tableHelper.Name,
                columnsBuilder.ToString(),
                simpleConstraintBuilder.ToString());
            te.Emit(outputWriter);
            outputWriter.Flush();


            //Remove the extra Comma, Leave it in if a _constraint is there...
            if (constraintCount == 0)
            {
                columnsBuilder.Replace(",", "", columnsBuilder.Length - 2, 2);
            }

            //Foreign Key Constraints
            foreach (Constraint c in _tableHelper.Constraints)
            {
                Message.Trace(Severity.Debug,"Found constraint {0}", c.Name);
                if(c is ForeignKeyConstraint)
                {
                    Message.Trace(Severity.Debug,"Found FKC {0}", c.Name);
                    ConstraintEmitter ce = new ConstraintEmitter(VulcanPackage, c, _tableHelper);
                    outputWriter.Write("\n");
                    ce.Emit(outputWriter);
                }
            }

            //Indexes
            IndexEmitter ie = new IndexEmitter(_tableHelper.Name, _tableHelper.TableNavigator, VulcanPackage);
            ie.Emit(outputWriter);

            InsertDefaultValuesEmitter ide = new InsertDefaultValuesEmitter(_tableHelper.Name, _tableHelper.TableNavigator, _tableHelper, VulcanPackage);
            ide.Emit(outputWriter);

            outputWriter.Write("\n");
        }
    }
}
