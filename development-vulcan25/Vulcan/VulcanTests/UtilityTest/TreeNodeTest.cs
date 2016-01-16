using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vulcan.Utility.Tree;

namespace UtilityTest
{
    /// <summary>
    /// Tests the Utility Tree functions
    /// </summary>
    [TestClass]
    public class TreeNodeTest
    {
        public TreeNodeTest()
        {
        }

        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }

            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        #endregion

        private static void GenerateTestTree(out SimpleTreeObjectCollection root, out List<SimpleTreeObjectCollection> testObjectList)
        {
            testObjectList = new List<SimpleTreeObjectCollection>();
            root = new SimpleTreeObjectCollection() { Name = "Root" };
            testObjectList.Add(root);

            var a0 = new SimpleTreeObjectCollection() { Name = "A0", Parent = root };
            testObjectList.Add(a0);

            var a1 = new SimpleTreeObjectCollection() { Name = "A1", Parent = a0 };
            testObjectList.Add(a1);

            var a2 = new SimpleTreeObjectCollection() { Name = "A2", Parent = a1 };
            testObjectList.Add(a2);

            var b1 = new SimpleTreeObjectCollection() { Name = "B1", Parent = a0 };
            testObjectList.Add(b1);

            var b2 = new SimpleTreeObjectCollection() { Name = "B2", Parent = b1 };
            testObjectList.Add(b2);

            ////int matchCount = testObjectList.Count;
        }

        [TestMethod]
        public void TestCount()
        {
            List<SimpleTreeObjectCollection> testObjectList;
            SimpleTreeObjectCollection root;

            GenerateTestTree(out root, out testObjectList);
            testContextInstance.WriteLine("TestCount: Simple Object Count = {0}, RootNode.Count = {1}", testObjectList.Count, root.Count);

            if (root.Count != testObjectList.Count)
            {
                throw new AssertFailedException(
                    String.Format(
                                  CultureInfo.InvariantCulture,
                                  "TestCount: Root.Count != Test Object Count. {0} expected, {1} returned",
                                  testObjectList.Count,
                                  root.Count));
            }
        }

        [TestMethod]
        public void TestAdd()
        {
            List<SimpleTreeObjectCollection> testObjectList;
            SimpleTreeObjectCollection root;

            GenerateTestTree(out root, out testObjectList);

            // Add method #1
            var addObject0 = new SimpleTreeObjectCollection() { Name = "Added Object 0", Parent = root };
            testContextInstance.WriteLine("TestAdd: Added: {0}", addObject0);
            testObjectList.AddRange(addObject0);

            // Add method #2
            var addObject1 = new SimpleTreeObjectCollection() { Name = "Added Object 1", Parent = null };
            addObject0.Children.Add(addObject1);
            testContextInstance.WriteLine("TestAdd: Added: {0}", addObject1);
            testObjectList.AddRange(addObject1);

            int matchCount = testObjectList.Count;
            foreach (var z in root)
            {
                if (testObjectList.Contains(z))
                {
                    testContextInstance.WriteLine("TestEnumerate: Found {0}", z);
                    matchCount--;
                }
            }

            if (matchCount != 0)
            {
                throw new AssertFailedException(
                    String.Format(
                        CultureInfo.InvariantCulture,
                        "TestEnumerate: Test was not able to enumerate the entire tree. {0} expected, {1} returned",
                        testObjectList.Count,
                        testObjectList.Count - matchCount));
            }
        }

        [TestMethod]
        public void TestRemoveWithParentNullMethodology()
        {
            List<SimpleTreeObjectCollection> testObjectList;
            SimpleTreeObjectCollection root;
            GenerateTestTree(out root, out testObjectList);

            List<SimpleTreeObjectCollection> removalList = new List<SimpleTreeObjectCollection>();
            testContextInstance.WriteLine("TestRemoveWithParentNullMethodology: Removing all objects Depth-First");

            removalList.AddRange(root);
            removalList.Reverse();
            testContextInstance.WriteLine("TestRemoveWithParentNullMethodology: Removal List: Count={0}", removalList.Count);

            testContextInstance.WriteLine("TestRemoveWithParentNullMethodology: Attempting Remove with Parent=Null methodology");
            int totalDepthCount = 0;
            foreach (var node in removalList)
            {
                node.Parent = null;
                testContextInstance.WriteLine("TestRemoveWithParentNullMethodology: Removed {0}", node);
                totalDepthCount += Math.Abs(node.Depth);
            }

            if (totalDepthCount != 0)
            {
                throw new AssertFailedException(
                    String.Format(
                        CultureInfo.InvariantCulture,
                        "TestRemoveWithParentNullMethodology: Test was not able to remove the entire tree. {0} nodes, Sum(DepthCount) = {1}",
                        testObjectList.Count,
                        totalDepthCount));
            }
        }

        [TestMethod]
        public void TestRemoveWithChildrenRemoveMethodology()
        {
            List<SimpleTreeObjectCollection> testObjectList;
            SimpleTreeObjectCollection root;
            GenerateTestTree(out root, out testObjectList);

            List<SimpleTreeObjectCollection> removalList = new List<SimpleTreeObjectCollection>();
            testContextInstance.WriteLine("TestRemoveWithChildrenRemoveMethodology: Removing all objects Depth-First");

            removalList.AddRange(root);
            removalList.Reverse();
            testContextInstance.WriteLine("TestRemoveWithChildrenRemoveMethodology: Removal List: Count={0}", removalList.Count);

            testContextInstance.WriteLine("TestRemoveWithChildrenRemoveMethodology: Attempting Remove with Remove Children methodology");
            foreach (var node in removalList)
            {
                List<SimpleTreeObjectCollection> subnodeList = new List<SimpleTreeObjectCollection>(node.Children);
                foreach (var subnode in subnodeList)
                {
                    node.Children.Remove(subnode);
                    subnode.Children.Remove(subnode);
                    testContextInstance.WriteLine("TestRemoveWithChildrenRemoveMethodology: Removed {0}", subnode);
                }
            }

            // Enumerate all possible trees and subtrees
            int totalDepthCount = 0;
            foreach (var node in removalList)
            {
                foreach (var subnode in node)
                {
                    totalDepthCount += Math.Abs(node.Depth);
                    testContextInstance.WriteLine("TestRemoveWithChildrenRemoveMethodology: Enumerated {0}", subnode);
                }
            }

            if (totalDepthCount != 0)
            {
                throw new AssertFailedException(
                    String.Format(
                        CultureInfo.InvariantCulture,
                        "TestRemoveWithChildrenRemoveMethodology: Test was not able to remove the entire tree. {0} nodes, Sum(DepthCount) = {1}",
                        testObjectList.Count,
                        totalDepthCount));
            }
        }

        [TestMethod]
        public void TestRemoveWithChildrenClearMethodology()
        {
            List<SimpleTreeObjectCollection> testObjectList;
            SimpleTreeObjectCollection root;
            GenerateTestTree(out root, out testObjectList);

            List<SimpleTreeObjectCollection> removalList = new List<SimpleTreeObjectCollection>();
            testContextInstance.WriteLine("TestRemoveWithChildrenClearMethodology: Removing all objects Depth-First");

            removalList.AddRange(root);
            removalList.Reverse();
            testContextInstance.WriteLine("TestRemoveWithChildrenClearMethodology: Removal List: Count={0}", removalList.Count);

            testContextInstance.WriteLine("TestRemoveWithChildrenClearMethodology: Attempting Remove with Children.Clear() methodology");
            foreach (var node in removalList)
            {
                node.Children.Clear();
                testContextInstance.WriteLine("TestRemoveWithChildrenClearMethodology: Cleared {0}", node);
            }

            int totalDepthCount = 0;
            foreach (var node in removalList)
            {
                foreach (var subnode in node)
                {
                    totalDepthCount += node.Depth;
                    testContextInstance.WriteLine("TestRemoveWithChildrenClearMethodology: Enumerated {0}", subnode);
                }
            }

            if (totalDepthCount != 0)
            {
                throw new AssertFailedException(
                    String.Format(
                        CultureInfo.InvariantCulture,
                        "TestRemoveWithChildrenClearMethodology was not able to remove the entire tree. {0} nodes, Sum(DepthCount) = {1}",
                        testObjectList.Count,
                        totalDepthCount));
            }
        }

        [TestMethod]
        public void TestEnumerate()
        {
            List<SimpleTreeObjectCollection> testObjectList;
            SimpleTreeObjectCollection root;

            GenerateTestTree(out root, out testObjectList);
            int matchCount = testObjectList.Count;

            foreach (var z in root)
            {
                if (testObjectList.Contains(z))
                {
                    testContextInstance.WriteLine("TestEnumerate: Found {0}", z);
                    matchCount--;
                }
            }

            if (matchCount != 0)
            {
                throw new AssertFailedException(
                    String.Format(
                        CultureInfo.InvariantCulture,
                        "TestEnumerate: Test was not able to enumerate the entire tree. {0} expected, {1} returned",
                        testObjectList.Count,
                        testObjectList.Count - matchCount));
            }
        }
    }
}
