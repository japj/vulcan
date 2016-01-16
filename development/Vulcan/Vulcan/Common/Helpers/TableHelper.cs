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
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Vulcan.Common;
using Vulcan.Common.Dimensions;
using Vulcan.Tasks;
using Vulcan.Common.Templates;
using Vulcan.Transformations;
using Vulcan.Emitters;
using Vulcan.Properties;
using DTS = Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;

namespace Vulcan.Common
{
    public enum KeyColumnType
    {
        None,
        Identity,
        PrimaryKey
    }

    public class TableHelper
    {
        private string _name;
        private VulcanConfig _vulcanConfig;
        private XPathNavigator _tableNavigator;
        private Dictionary<string,Column> _columnLookup;
        private List<Constraint> _constraintList;

        private Column _keyColumn;
        private KeyColumnType _keyColumnType;

        public TableHelper(string name, VulcanConfig vulcanConfig, XPathNavigator tableNavigator)
        {
            this._name = name;
            this._vulcanConfig = vulcanConfig;
            this._tableNavigator = tableNavigator;

            this._columnLookup = new Dictionary<string,Column>();
            this._constraintList = new List<Constraint>();
            _ParseColumns();
            _ParseKeyColumn();
            _PopulateConstraints();
            _PopulateDimensions();
        }

        public Column KeyColumn
        {
            get
            {
                return _keyColumn;
            }
        }

        public KeyColumnType KeyColumnType
        {
            get
            {
                return _keyColumnType;
            }
        }

        private void _ParseKeyColumn()
        {
            XPathNavigator keyColumnNav = _tableNavigator.SelectSingleNode("rc:KeyColumn", _vulcanConfig.NamespaceManager);

            if (keyColumnNav != null)
            {
                string name = keyColumnNav.SelectSingleNode("@Name").Value;
                string type = keyColumnNav.SelectSingleNode("@Type").Value;

                if(this._columnLookup.ContainsKey(name))
                {
                   this._keyColumn = this._columnLookup[name];

                    switch(type)
                    {
                        case "PrimaryKey":
                            this._keyColumnType = KeyColumnType.PrimaryKey;
                            break;
                        case "Identity":
                            this._keyColumnType = KeyColumnType.Identity;
                            break;
                        default:
                            Message.Trace(Severity.Error,"Unknown key column type {0}",type);
                            this._keyColumnType = KeyColumnType.None;
                            break;
                    }
                }
                else
                {
                     Message.Trace(Severity.Warning,"Invalid key column {0} in {1}. Skipping",name,this.Name);
                    return;
                }
            }
            else
            {
                _keyColumn = null;
                _keyColumnType = KeyColumnType.None;
            }
        }
        private void _ParseColumns()
        {
            foreach (XPathNavigator colNav in _tableNavigator.Select("rc:Columns/rc:Column", _vulcanConfig.NamespaceManager))
            {
                Column c = new Column();
                foreach (XPathNavigator nav in colNav.Select("@*"))
                {
                    c.Properties.Add(nav.Name, nav.Value);
                }
                if(!_columnLookup.ContainsKey(c.Properties["Name"]))
                {
                _columnLookup.Add(c.Properties["Name"], c);
                }
                else
                {
                    Message.Trace(Severity.Error,"Duplicate column {0} in {1}",c.Properties["Name"],this.Name);
                    return;
                }
            }
        }

        private void _PopulateDimensions()
        {
            foreach (XPathNavigator dimNav in _tableNavigator.Select("rc:Columns/rc:Dimension", _vulcanConfig.NamespaceManager))
            {
                string dimName = dimNav.SelectSingleNode("@Name").Value;
                string factTableDimColumnName = dimNav.SelectSingleNode("@OutputName").Value;

                Message.Trace(Severity.Debug,"Mapping {0} to Dimension {1}", _name, dimName);

                if (DimensionHelper.DimDictionary.ContainsKey(dimName))
                {
                    TableHelper dimTableHelper = 
                        new TableHelper(
                        dimName, 
                        DimensionHelper.DimDictionary[dimName], 
                        DimensionHelper.DimNavigatorDictionary[dimName].SelectSingleNode("rc:Table",_vulcanConfig.NamespaceManager));
                    Column dimKeyColumn = dimTableHelper.KeyColumn;

                    if (dimKeyColumn != null)
                    {
                        string constraintName = String.Format(System.Globalization.CultureInfo.InvariantCulture,"{0}_{1}_{2}", _name, dimName, factTableDimColumnName);

                        //Clone the Foreign Key Column and add it into our fact table.
                        Column tableColumn = (Column)dimKeyColumn.Clone();
                        tableColumn.Properties["Name"] = factTableDimColumnName;
                        this.Columns.Add(factTableDimColumnName,tableColumn);

                        ForeignKeyConstraint fkc = new ForeignKeyConstraint(constraintName, tableColumn, dimKeyColumn, dimName);
                        this.Constraints.Add(fkc);
                    }
                    else
                    {
                        dimTableHelper.TraceHelper();
                        Message.Trace(Severity.Error,"Dimension {0} has no key column.\n{1}", dimName,dimTableHelper.TableNavigator.OuterXml);
                    }
                }
                else
                {
                    Message.Trace(Severity.Error,"Dimension {0} cannot be found in the Dimensions Database.", dimName);
                }
            }

        }

        private SimpleConstraint _ParseSimpleConstraint(XPathNavigator simpleConstraintNav, bool isPrimaryKey, int index)
        {
            SimpleConstraint sc;
            List<Column> localColumnList = new List<Column>();

            foreach (XPathNavigator colNav in simpleConstraintNav.Select("rc:Column", _vulcanConfig.NamespaceManager))
            {
                Column c = new Column();
                foreach (XPathNavigator nav in colNav.Select("@*"))
                {
                    c.Properties.Add(nav.Name, nav.Value);
                }
                localColumnList.Add(c);
            }

            if (isPrimaryKey)
            {
                string name = String.Format(System.Globalization.CultureInfo.InvariantCulture, "PK_{0}_{1}", _name, index, localColumnList);
                sc = new PrimaryKeyConstraint(name, localColumnList);
            }
            else
            {
                string name = String.Format(System.Globalization.CultureInfo.InvariantCulture, "CON_{0}_{1}", _name, index, localColumnList);
                sc = new SimpleConstraint(name, localColumnList);
            }

            foreach (XPathNavigator attributeNav in simpleConstraintNav.Select("@*"))
            {
                sc.Properties.Add(attributeNav.Name, attributeNav.Value);
            }
            return sc;
        }

        private void _PopulateConstraints()
        {
            int index = 0;
            foreach (XPathNavigator conNav in _tableNavigator.Select("rc:Constraints/rc:PrimaryKeyConstraint", _vulcanConfig.NamespaceManager))
            {
                _constraintList.Add(_ParseSimpleConstraint(conNav,true,index));
                index++;
            }

            index = 0;
            foreach (XPathNavigator conNav in _tableNavigator.Select("rc:Constraints/rc:Constraint", _vulcanConfig.NamespaceManager))
            {
                _constraintList.Add(_ParseSimpleConstraint(conNav, false, index));
                index++;
            }

            foreach (XPathNavigator fkCon in _tableNavigator.Select("rc:Constraints/rc:ForeignKeyConstraint", _vulcanConfig.NamespaceManager))
            {
                Column localColumn = new Column();
                Column foreignColumn = new Column();
                foreach (XPathNavigator nav in fkCon.Select("rc:LocalColumn/@*", _vulcanConfig.NamespaceManager))
                {
                    localColumn.Properties[nav.Name] = nav.Value;
                }
                foreach (XPathNavigator nav in fkCon.Select("rc:ForeignColumn/@*", _vulcanConfig.NamespaceManager))
                {
                    foreignColumn.Properties[nav.Name] = nav.Value;
                }
                string fkName = String.Format(System.Globalization.CultureInfo.InvariantCulture,"FK_{0}_{1}_{2}_{3}",_name,localColumn.Name,foreignColumn.Properties["Table"],foreignColumn.Name);
                ForeignKeyConstraint fkc = new ForeignKeyConstraint(fkName, localColumn, foreignColumn, foreignColumn.Properties["Table"]);
                _constraintList.Add(fkc);
            }
        }

        public bool IsIdentityColumn(string name)
        {
            bool isIdentity = (
                (KeyColumnType == KeyColumnType.Identity)
                &&
                (KeyColumn.Name.ToUpperInvariant() == name.ToUpperInvariant())
                );
            return isIdentity;
        }
        public void TraceHelper()
        {
            Message.Trace(Severity.Debug, "TableHelper for table {0}", _name);
            foreach (Column c in _columnLookup.Values)
            {
                Message.Trace(Severity.Debug, "Table {0}: Column: {1}: Type {2}", _name, c.Properties["Name"], c.Properties["Type"]);
            }

            foreach (Constraint c in this._constraintList)
            {
                Message.Trace(Severity.Debug, "Constraint {0}", c.Name);
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public Dictionary<string,Column> Columns
        {
            get
            {
                return this._columnLookup;
            }
        }

        public List<Constraint> Constraints
        {
            get
            {
                return this._constraintList;
            }
        }

        public XPathNavigator TableNavigator
        {
            get
            {
                return this._tableNavigator;
            }
        }
    } // End TableHelper

    public class PrimaryKeyConstraint : SimpleConstraint
    {
        public PrimaryKeyConstraint(string name, Column c)
            :
            base(name,c)
        {
        }
        public PrimaryKeyConstraint(string name, List<Column> columnList)
            :
            base(name,columnList)
        {
        }
    }

    public class SimpleConstraint : Constraint
    {
        private List<Column> _columnList;

        public SimpleConstraint(string name, Column c)
            :
            base(name)
        {
            _columnList = new List<Column>();
            _columnList.Add(c);
        }

        public SimpleConstraint(string name, List<Column> columnList)
            :
            base(name)
        {
            _columnList = columnList;
        }
        public List<Column> Columns
        {
            get
            {
                return this._columnList;
            }
        }
    }

    public class ForeignKeyConstraint : Constraint
    {
        private Column _localColumn;
        private Column _foreignColumn;
        private string _foreignTable;

        public ForeignKeyConstraint(string name, Column localColumn, Column foreignColumn, string foreignTable)
            :
            base(name)
        {
            _localColumn = localColumn;
            _foreignColumn = foreignColumn;
            _foreignTable = foreignTable;
        }

        public Column LocalColumn
        {
            get
            {
                return _localColumn;
            }
        }

        public Column ForeignColumn
        {
            get
            {
                return _foreignColumn;
            }
        }

        public string ForeignTable
        {
            get
            {
                return _foreignTable;
            }
        }
    }

    public abstract class Constraint
    {
        private string _name;
        private  Dictionary<string, string> _properties;

        protected Constraint(string name)
        {
            _name= name;
            _properties = new Dictionary<string, string>();
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public Dictionary<string, string> Properties
        {
            get
            {
                return this._properties;
            }
        }
    }

    public class Column : ICloneable
    {
        private  Dictionary<string, string> _properties;
        public Column()
        {
            _properties = new Dictionary<string, string>();
        }

        public Dictionary<string, string> Properties
        {
            get
            {
                return this._properties;
            }
        }

        public string Name
        {
            get
            {
                return _properties["Name"];
            }
            set
            {
                _properties["Name"] = value;
            }
        }

        /// <summary>
        /// Performs a deep clone of the Column
        /// </summary>
        /// <returns>Column which was cloned.</returns>
        public Object Clone()
        {
            Column c = new Column();
            foreach (string s in this._properties.Keys)
            {
                c.Properties[s] = _properties[s];
            }
            return c;
        }
    }
} // End Namespace
