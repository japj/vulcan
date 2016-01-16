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
