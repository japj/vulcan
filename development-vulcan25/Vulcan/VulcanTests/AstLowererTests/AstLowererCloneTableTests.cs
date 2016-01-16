using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VulcanTests
{
    [TestClass]
    public class AstLowererCloneTableTests
    {
        public const string DefaultXmlNamespace = "http://tempuri.org/vulcan2.xsd";
        
        public const AstComparerOptions DefaultComparerOptions = AstComparerOptions.IgnoreExternalReferences | AstComparerOptions.IgnoreNonFrameworkProperties | AstComparerOptions.IgnoreNameDifferences | AstComparerOptions.IgnoreStringWhiteSpaceDifferences;
        
        private static readonly AstComparer DefaultComparer = new AstComparer(DefaultXmlNamespace, DefaultComparerOptions);

        #region BasicTable

        #region IdentityKey

        [TestMethod]
        public void CloneTable_BasicTable_IdentityKey_AddColumnsAndIdentityKey()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.BasicTable.IdentityKey.AddColumnsAndIdentityKey_PRE.xml", "CloneTable.BasicTable.IdentityKey.AddColumnsAndIdentityKey_POST.xml");
        }

        [TestMethod]
        public void CloneTable_BasicTable_IdentityKey_AddIdentityKey()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.BasicTable.IdentityKey.AddIdentityKey_PRE.xml", "CloneTable.BasicTable.IdentityKey.AddIdentityKey_POST.xml");
        }

        #endregion IdentityKey

        #region Indexes

        [TestMethod]
        public void CloneTable_BasicTable_Indexes_AddColumnsAndIndexes()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.BasicTable.Indexes.AddColumnsAndIndexes_PRE.xml", "CloneTable.BasicTable.Indexes.AddColumnsAndIndexes_POST.xml");
        }

        [TestMethod]
        public void CloneTable_BasicTable_Indexes_AddIndexes()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.BasicTable.Indexes.AddIndexes_PRE.xml", "CloneTable.BasicTable.Indexes.AddIndexes_POST.xml");
        }

        #endregion Indexes

        #region MultipleKeys

        [TestMethod]
        public void CloneTable_BasicTable_MultipleKeys_AddColumnsAndMultipleKeys()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.BasicTable.MultipleKeys.AddColumnsAndMultipleKeys_PRE.xml", "CloneTable.BasicTable.MultipleKeys.AddColumnsAndMultipleKeys_POST.xml");
        }

        [TestMethod]
        public void CloneTable_BasicTable_MultipleKeys_AddMultipleKeys()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.BasicTable.MultipleKeys.AddMultipleKeys_PRE.xml", "CloneTable.BasicTable.MultipleKeys.AddMultipleKeys_POST.xml");
        }

        #endregion MultipleKeys

        #region PrimaryKey

        [TestMethod]
        public void CloneTable_BasicTable_PrimaryKey_AddColumnsAndPrimaryKey()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.BasicTable.PrimaryKey.AddColumnsAndPrimaryKey_PRE.xml", "CloneTable.BasicTable.PrimaryKey.AddColumnsAndPrimaryKey_POST.xml");
        }

        [TestMethod]
        public void CloneTable_BasicTable_PrimaryKey_AddPrimaryKey()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.BasicTable.PrimaryKey.AddPrimaryKey_PRE.xml", "CloneTable.BasicTable.PrimaryKey.AddPrimaryKey_POST.xml");
        }

        #endregion PrimaryKey

        #region UniqueKey

        [TestMethod]
        public void CloneTable_BasicTable_UniqueKey_AddColumnsAndUniqueKey()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.BasicTable.UniqueKey.AddColumnsAndUniqueKey_PRE.xml", "CloneTable.BasicTable.UniqueKey.AddColumnsAndUniqueKey_POST.xml");
        }

        [TestMethod]
        public void CloneTable_BasicTable_UniqueKey_AddUniqueKey()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.BasicTable.UniqueKey.AddUniqueKey_PRE.xml", "CloneTable.BasicTable.UniqueKey.AddUniqueKey_POST.xml");
        }

        #endregion UniqueKey

        #region Other

        [TestMethod]
        public void CloneTable_BasicTable_AddColumn()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.BasicTable.AddColumn_PRE.xml", "CloneTable.BasicTable.AddColumn_POST.xml");
        }

        [TestMethod]
        public void CloneTable_BasicTable_AddEverything()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.BasicTable.AddEverything_PRE.xml", "CloneTable.BasicTable.AddEverything_POST.xml");
        }

        #endregion Other

        #endregion BasicTable

        #region FullTable

        #region IdentityKey

        [TestMethod]
        public void CloneTable_FullTable_IdentityKey_CloneAllFalseAndAddIdentityKey()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.FullTable.IdentityKey.CloneAllFalseAndAddIdentityKey_PRE.xml", "CloneTable.FullTable.IdentityKey.CloneAllFalseAndAddIdentityKey_POST.xml");
        }

        [TestMethod]
        public void CloneTable_FullTable_IdentityKey_CloneAllTrueAndAddIdentityKey()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.FullTable.IdentityKey.CloneAllTrueAndAddIdentityKey_PRE.xml", "CloneTable.FullTable.IdentityKey.CloneAllTrueAndAddIdentityKey_POST.xml");
        }

        #endregion IdentityKey

        #region Indexes

        [TestMethod]
        public void CloneTable_FullTable_Indexes_CloneAllFalseAndAddIndexes()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.FullTable.Indexes.CloneAllFalseAndAddIndexes_PRE.xml", "CloneTable.FullTable.Indexes.CloneAllFalseAndAddIndexes_POST.xml");
        }

        [TestMethod]
        public void CloneTable_FullTable_Indexes_CloneAllTrueAndAddIndexes()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.FullTable.Indexes.CloneAllTrueAndAddIndexes_PRE.xml", "CloneTable.FullTable.Indexes.CloneAllTrueAndAddIndexes_POST.xml");
        }

        [TestMethod]
        public void CloneTable_FullTable_Indexes_CloneIndexesFalse()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.FullTable.Indexes.CloneIndexesFalse_PRE.xml", "CloneTable.FullTable.Indexes.CloneIndexesFalse_POST.xml");
        }

        [TestMethod]
        public void CloneTable_FullTable_Indexes_CloneIndexesTrue()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.FullTable.Indexes.CloneIndexesTrue_PRE.xml", "CloneTable.FullTable.Indexes.CloneIndexesTrue_POST.xml");
        }

        #endregion Indexes

        #region MultipleKeys

        [TestMethod]
        public void CloneTable_FullTable_MultipleKeys_CloneAllFalseAndAddMultipleKeys()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.FullTable.MultipleKeys.CloneAllFalseAndAddMultipleKeys_PRE.xml", "CloneTable.FullTable.MultipleKeys.CloneAllFalseAndAddMultipleKeys_POST.xml");
        }

        [TestMethod]
        public void CloneTable_FullTable_MultipleKeys_CloneAllTrueAndAddMultipleKeys()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.FullTable.MultipleKeys.CloneAllTrueAndAddMultipleKeys_PRE.xml", "CloneTable.FullTable.MultipleKeys.CloneAllTrueAndAddMultipleKeys_POST.xml");
        }

        [TestMethod]
        public void CloneTable_FullTable_MultipleKeys_CloneKeysFalse()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.FullTable.MultipleKeys.CloneKeysFalse_PRE.xml", "CloneTable.FullTable.MultipleKeys.CloneKeysFalse_POST.xml");
        }

        [TestMethod]
        public void CloneTable_FullTable_MultipleKeys_CloneKeysTrue()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.FullTable.MultipleKeys.CloneKeysTrue_PRE.xml", "CloneTable.FullTable.MultipleKeys.CloneKeysTrue_POST.xml");
        }

        #endregion MultipleKeys

        #region PrimaryKey

        [TestMethod]
        public void CloneTable_FullTable_PrimaryKey_CloneAllFalseAndAddPrimaryKey()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.FullTable.PrimaryKey.CloneAllFalseAndAddPrimaryKey_PRE.xml", "CloneTable.FullTable.PrimaryKey.CloneAllFalseAndAddPrimaryKey_POST.xml");
        }

        [TestMethod]
        public void CloneTable_FullTable_PrimaryKey_CloneAllTrueAndAddPrimaryKey()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.FullTable.PrimaryKey.CloneAllTrueAndAddPrimaryKey_PRE.xml", "CloneTable.FullTable.PrimaryKey.CloneAllTrueAndAddPrimaryKey_POST.xml");
        }

        #endregion PrimaryKey

        #region UniqueKey

        [TestMethod]
        public void CloneTable_FullTable_UniqueKey_CloneAllFalseAndAddUniqueKey()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.FullTable.UniqueKey.CloneAllFalseAndAddUniqueKey_PRE.xml", "CloneTable.FullTable.UniqueKey.CloneAllFalseAndAddUniqueKey_POST.xml");
        }

        [TestMethod]
        public void CloneTable_FullTable_UniqueKey_CloneAllTrueAndAddUniqueKey()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.FullTable.UniqueKey.CloneAllTrueAndAddUniqueKey_PRE.xml", "CloneTable.FullTable.UniqueKey.CloneAllTrueAndAddUniqueKey_POST.xml");
        }

        #endregion UniqueKey

        #region Other

        [TestMethod]
        public void CloneTable_FullTable_AddColumns()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.FullTable.AddColumns_PRE.xml", "CloneTable.FullTable.AddColumns_POST.xml");
        }

        [TestMethod]
        public void CloneTable_FullTable_CloneAllFalse()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.FullTable.CloneAllFalse_PRE.xml", "CloneTable.FullTable.CloneAllFalse_POST.xml");
        }

        [TestMethod]
        public void CloneTable_FullTable_CloneAllFalseAndAddColumns()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.FullTable.CloneAllFalseAndAddColumns_PRE.xml", "CloneTable.FullTable.CloneAllFalseAndAddColumns_POST.xml");
        }

        [TestMethod]
        public void CloneTable_FullTable_CloneAllFalseAndAddEverything()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.FullTable.CloneAllFalseAndAddEverything_PRE.xml", "CloneTable.FullTable.CloneAllFalseAndAddEverything_POST.xml");
        }
        
        [TestMethod]
        public void CloneTable_FullTable_CloneAllTrue()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.FullTable.CloneAllTrue_PRE.xml", "CloneTable.FullTable.CloneAllTrue_POST.xml");
        }

        [TestMethod]
        public void CloneTable_FullTable_CloneAllTrueAndAddColumns()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.FullTable.CloneAllTrueAndAddColumns_PRE.xml", "CloneTable.FullTable.CloneAllTrueAndAddColumns_POST.xml");
        }

        [TestMethod]
        public void CloneTable_FullTable_CloneAllTrueAndAddEverything()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.FullTable.CloneAllTrueAndAddEverything_PRE.xml", "CloneTable.FullTable.CloneAllTrueAndAddEverything_POST.xml");
        }

        [TestMethod]
        public void CloneTable_FullTable_Default()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.FullTable.Default_PRE.xml", "CloneTable.FullTable.Default_POST.xml");
        }

        #endregion Other

        #endregion FullTable

        #region Other

        [TestMethod]
        public void CloneTable_BasicAndEmpty()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("CloneTable.BasicAndEmpty_PRE.xml", "CloneTable.BasicAndEmpty_POST.xml");
        }

        #endregion Other
    }
}
