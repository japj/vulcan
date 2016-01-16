using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VulcanTests
{
    [TestClass]
    public class AstLowererTableTests
    {
        public const string DefaultXmlNamespace = "http://tempuri.org/vulcan2.xsd";
        
        public const AstComparerOptions DefaultComparerOptions = AstComparerOptions.IgnoreExternalReferences | AstComparerOptions.IgnoreNonFrameworkProperties | AstComparerOptions.IgnoreNameDifferences | AstComparerOptions.IgnoreStringWhiteSpaceDifferences;
        
        private static readonly AstComparer DefaultComparer = new AstComparer(DefaultXmlNamespace, DefaultComparerOptions);

        [TestMethod]
        public void Table_BasicAndEmpty()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Table.BasicAndEmpty_PRE.xml", "Table.BasicAndEmpty_POST.xml");
        }

        [TestMethod]
        public void Table_SpacedTable()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Table.SpacedTable_PRE.xml", "Table.SpacedTable_POST.xml");
        }

        [TestMethod]
        public void Table_Schema()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Table.Schema_PRE.xml", "Table.Schema_POST.xml");
        }

        [TestMethod]
        public void Table_LateArriving()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Table.LateArriving_PRE.xml", "Table.LateArriving_POST.xml");
        }

        [TestMethod]
        public void Table_CustomExtensions()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Table.CustomExtensions_PRE.xml", "Table.CustomExtensions_POST.xml");
        }

        [TestMethod]
        public void Table_Column_Types()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Table.Column.Types_PRE.xml", "Table.Column.Types_POST.xml");
        }

        [TestMethod]
        public void Table_Column_Properties()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Table.Column.Properties_PRE.xml", "Table.Column.Properties_POST.xml");
        }

        [TestMethod]
        public void Table_Column_Scd()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Table.Column.Scd_PRE.xml", "Table.Column.Scd_POST.xml");
        }

        [TestMethod]
        public void Table_IdentityKey_Basic()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Table.IdentityKey.Basic_PRE.xml", "Table.IdentityKey.Basic_POST.xml");
        }

        [TestMethod]
        public void Table_PrimaryKey_Basic()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Table.PrimaryKey.Basic_PRE.xml", "Table.PrimaryKey.Basic_POST.xml");
        }

        [TestMethod]
        public void Table_PrimaryKey_Properties()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Table.PrimaryKey.Properties_PRE.xml", "Table.PrimaryKey.Properties_POST.xml");
        }

        [TestMethod]
        public void Table_PrimaryKey_ColumnProperties()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Table.PrimaryKey.ColumnProperties_PRE.xml", "Table.PrimaryKey.ColumnProperties_POST.xml");
        }

        [TestMethod]
        public void Table_UniqueKey_Basic()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Table.UniqueKey.Basic_PRE.xml", "Table.UniqueKey.Basic_POST.xml");
        }

        [TestMethod]
        public void Table_UniqueKey_Properties()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Table.UniqueKey.Properties_PRE.xml", "Table.UniqueKey.Properties_POST.xml");
        }

        [TestMethod]
        public void Table_UniqueKey_ColumnProperties()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Table.UniqueKey.ColumnProperties_PRE.xml", "Table.UniqueKey.ColumnProperties_POST.xml");
        }

        [TestMethod]
        public void Table_Index_Basic()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Table.Index.Basic_PRE.xml", "Table.Index.Basic_POST.xml");
        }

        [TestMethod]
        public void Table_Index_Properties()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Table.Index.Properties_PRE.xml", "Table.Index.Properties_POST.xml");
        }

        [TestMethod]
        public void Table_Index_ColumnProperties()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Table.Index.ColumnProperties_PRE.xml", "Table.Index.ColumnProperties_POST.xml");
        }

        [TestMethod]
        public void Table_PermissionBasic()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Table.PermissionBasic_PRE.xml", "Table.PermissionBasic_POST.xml");
        }

        [TestMethod]
        public void Table_Column_PermissionBasic()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Table.Column.PermissionBasic_PRE.xml", "Table.Column.PermissionBasic_POST.xml");
        }

        [TestMethod]
        public void Table_StaticSource_Basic()
        {
            ////DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Table.StaticSource.Basic_PRE.xml", "Table.StaticSource.Basic_POST.xml");
        }

        [TestMethod]
        public void Table_StaticSource_NoEmitMergePackage()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Table.StaticSource.NoEmitMergePackage_PRE.xml", "Table.StaticSource.NoEmitMergePackage_POST.xml");
        }

        [TestMethod]
        public void Table_StaticSource_DimReferences()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Table.StaticSource.DimReferences_PRE.xml", "Table.StaticSource.DimReferences_POST.xml");
        }
    }
}
