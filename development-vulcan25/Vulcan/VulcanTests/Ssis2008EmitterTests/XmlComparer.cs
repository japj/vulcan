using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VulcanTests.Ssis2008EmitterTests
{
    public static class XmlComparer
    {
        public static void CompareText(string content1, string content2)
        {
            CompareText(content1, content2, true);
        }

        public static void CompareText(string content1, string content2, bool ignoreWhiteSpace)
        {
            if (ignoreWhiteSpace)
            {
                content1 = Regex.Replace(content1, @"><", "> <");
                content2 = Regex.Replace(content2, @"><", "> <");

                content1 = Regex.Replace(content1, @"\s+", " ");
                content2 = Regex.Replace(content2, @"\s+", " ");
            }

            Assert.AreEqual(content1, content2, "Text contents do not match.");
        }
    }
}