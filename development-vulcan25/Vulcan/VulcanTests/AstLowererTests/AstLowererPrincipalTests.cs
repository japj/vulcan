using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VulcanTests
{
    [TestClass]
    public class AstLowererPrincipalTests
    {
        public const string DefaultXmlNamespace = "http://tempuri.org/vulcan2.xsd";
        
        public const AstComparerOptions DefaultComparerOptions = AstComparerOptions.IgnoreExternalReferences | AstComparerOptions.IgnoreNonFrameworkProperties | AstComparerOptions.IgnoreNameDifferences | AstComparerOptions.IgnoreStringWhiteSpaceDifferences;
        
        private static readonly AstComparer DefaultComparer = new AstComparer(DefaultXmlNamespace, DefaultComparerOptions);

        [TestMethod]
        public void Principal_ApplicationRole()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Principal.Principal_ApplicationRole_PRE.xml", "Principal.Principal_ApplicationRole_POST.xml");
        }

        [TestMethod]
        public void Principal_DatabaseRole()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Principal.Principal_DatabaseRole_PRE.xml", "Principal.Principal_DatabaseRole_POST.xml");
        }

        [TestMethod]
        public void Principal_NoEmit()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Principal.Principal_NoEmit_PRE.xml", "Principal.Principal_NoEmit_POST.xml");
        }

        [TestMethod]
        public void Principal_SqlUser()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Principal.Principal_SqlUser_PRE.xml", "Principal.Principal_SqlUser_POST.xml");
        }

        [TestMethod]
        public void Principal_WindowsGroup()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Principal.Principal_WindowsGroup_PRE.xml", "Principal.Principal_WindowsGroup_POST.xml");
        }

        [TestMethod]
        public void Principal_WindowsUser()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Principal.Principal_WindowsUser_PRE.xml", "Principal.Principal_WindowsUser_POST.xml");
        }
    }
}
