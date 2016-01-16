using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VulcanTests.Ssis2008EmitterTests
{
    [TestClass]
    public class Ssis2008EmitterTemplateTests
    {
        private static readonly SsisComparer DefaultComparer = SsisComparer.DefaultSsisComparer;

        [TestMethod]
        public void Templates_Package()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Templates.Package_PRE.xml", "Templates.Package_POST");
        }
    }
}
