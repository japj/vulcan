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
