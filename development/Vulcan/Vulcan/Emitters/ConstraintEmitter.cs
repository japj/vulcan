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
    public class ConstraintEmitter : Emitter
    {
        private  Constraint _constraint;
        private TableHelper _tableHelper;

        public ConstraintEmitter(Packages.VulcanPackage vulcanPackage, Constraint constraint, TableHelper tableHelper)
            : base(vulcanPackage)
        {
            this._tableHelper = tableHelper;
            this._constraint = constraint;
            this._tableHelper = tableHelper;
        }

        public override void Emit(TextWriter outputWriter)
        {

            if (_constraint is PrimaryKeyConstraint || _constraint is SimpleConstraint)
            {

                SimpleConstraint sc = (SimpleConstraint)_constraint;
                
                string clustered = !sc.Properties.ContainsKey("Clustered")
                                                     ? ""
                                                     :
                                                     (Convert.ToBoolean(sc.Properties["Clustered"],System.Globalization.CultureInfo.InvariantCulture)
                                                        ? "CLUSTERED"
                                                        : "NONCLUSTERED"
                                                        );


                string ignoreDupKey = !sc.Properties.ContainsKey("IgnoreDupKey")
                                                     ? ""
                                                     : (Convert.ToBoolean(sc.Properties["IgnoreDupKey"],System.Globalization.CultureInfo.InvariantCulture)
                                                        ? "IGNORE_DUP_KEY = ON"
                                                        : "IGNORE_DUP_KEY = OFF"
                                                     );

                string padIndex = !sc.Properties.ContainsKey("PadIndex")
                                                     ? ""
                                                     :
                                                     (Convert.ToBoolean(sc.Properties["PadIndex"],System.Globalization.CultureInfo.InvariantCulture)
                                                        ? "PAD_INDEX = ON"
                                                        : "PAD_INDEX = OFF"
                                                     );


                StringBuilder columnsBuilder = new StringBuilder();
                string pkName = sc.Properties.ContainsKey("Name") ? sc.Properties["Name"] : sc.Name;
                foreach (Column col in sc.Columns)
                {
                    string name = col.Name;
                    string sortOrder = !col.Properties.ContainsKey("SortOrder")
                                                     ? " ASC"
                                                     : " "+col.Properties["SortOrder"];
                    columnsBuilder.AppendFormat(
                        "[{0}]{1},\n",
                        name,
                        sortOrder
                    );
                    pkName = pkName + "_" + name;
                }
                columnsBuilder.Replace(",", "", columnsBuilder.Length - 2, 2);

                StringBuilder optionsBuilder = new StringBuilder();

                if (! String.IsNullOrEmpty(padIndex))
                    optionsBuilder.AppendFormat("{0},", padIndex);
                if(!String.IsNullOrEmpty(ignoreDupKey))
                    optionsBuilder.AppendFormat("{0},", ignoreDupKey);

                if (optionsBuilder.Length > 0)
                {
                    optionsBuilder.Replace(",", "",optionsBuilder.Length-1,1);
                    optionsBuilder.Insert(0, "WITH(");
                    optionsBuilder.Append(")");
                }

                string primaryKeyString = sc is PrimaryKeyConstraint ? "PRIMARY KEY" : "";
                string unique = sc is PrimaryKeyConstraint ? "" : "UNIQUE ";

                TemplateEmitter te = new TemplateEmitter("ConstraintTemplate", VulcanPackage, null);
                te.SetParameters(
                    pkName,
                    unique + clustered,
                    columnsBuilder.ToString(),
                    optionsBuilder.ToString(),
                    primaryKeyString
                    );

                te.Emit(outputWriter);
                outputWriter.Flush();
            } // end if Primary Key Constraint
            else if (_constraint is ForeignKeyConstraint)
            {
                ForeignKeyConstraint fkc = (ForeignKeyConstraint)_constraint;
                TemplateEmitter te = new TemplateEmitter(
                    "ForeignKeyConstraintTemplate", 
                    VulcanPackage, 
                    _tableHelper.Name, 
                    fkc.Name, 
                    fkc.LocalColumn.Name, 
                    fkc.ForeignTable, 
                    fkc.ForeignColumn.Name
                    );
                te.Emit(outputWriter);
            }
        }
    }
}
