using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VulcanTests
{
    [TestClass]
    public class AstLowererSchemaTests
    {
        public const string DefaultXmlNamespace = "http://tempuri.org/vulcan2.xsd";
        
        public const AstComparerOptions DefaultComparerOptions = AstComparerOptions.IgnoreExternalReferences | AstComparerOptions.IgnoreNonFrameworkProperties | AstComparerOptions.IgnoreNameDifferences | AstComparerOptions.IgnoreStringWhiteSpaceDifferences;
        
        private static readonly AstComparer DefaultComparer = new AstComparer(DefaultXmlNamespace, DefaultComparerOptions);

        [TestMethod]
        public void Schema_BasicAndEmpty()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Schema.Schema_BasicAndEmpty_PRE.xml", "Schema.Schema_BasicAndEmpty_POST.xml");
        }

        [TestMethod]
        public void Schema_CustomExtensions()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Schema.Schema_CustomExtensions_PRE.xml", "Schema.Schema_CustomExtensions_POST.xml");
        }

        [TestMethod]
        public void Schema_NoEmit()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Schema.Schema_NoEmit_PRE.xml", "Schema.Schema_NoEmit_POST.xml");
        }

        [TestMethod]
        public void Schema_Permissions()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Schema.Schema_Permissions_PRE.xml", "Schema.Schema_Permissions_POST.xml");
        }
    }
}
