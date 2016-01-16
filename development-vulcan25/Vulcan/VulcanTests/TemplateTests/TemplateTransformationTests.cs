using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VulcanTests
{
    [TestClass]
    public class TemplateTransformationTests
    {
        public const string DefaultXmlNamespace = "http://tempuri.org/vulcan2.xsd";

        public const AstComparerOptions DefaultComparerOptions = AstComparerOptions.IgnoreExternalReferences | AstComparerOptions.IgnoreNonFrameworkProperties | AstComparerOptions.IgnoreNameDifferences | AstComparerOptions.IgnoreStringWhiteSpaceDifferences;

        private static readonly AstComparer DefaultComparer = new AstComparer(DefaultXmlNamespace, DefaultComparerOptions, "VulcanTests.TemplateTests");

        [TestMethod]
        public void Transformation_DerivedColumnBasic()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Transformation.DerivedColumn_PRE.xml", "Transformation.DerivedColumn_POST.xml");
        }

        /// <summary>
        /// Note! Template_PackageTemplate cannot test Emit versus Includes.  For that see the equivalent SSIS2008Emitter test.
        /// </summary>
        [TestMethod]
        public void Template_PackageTemplate()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.PackageTemplate_PRE.xml", "Package.PackageTemplate_POST.xml");
        }
    }
}
