using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VulcanTests
{
    [TestClass]
    public class AstLowererPackageTests
    {
        public const string DefaultXmlNamespace = "http://tempuri.org/vulcan2.xsd";

        public const AstComparerOptions DefaultComparerOptions = AstComparerOptions.IgnoreExternalReferences | AstComparerOptions.IgnoreNonFrameworkProperties | AstComparerOptions.IgnoreNameDifferences | AstComparerOptions.IgnoreStringWhiteSpaceDifferences;

        private static readonly AstComparer DefaultComparer = new AstComparer(DefaultXmlNamespace, DefaultComparerOptions);

        [TestMethod]
        public void Package_BasicAndEmpty()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.Package_BasicAndEmpty_PRE.xml", "Package.Package_BasicAndEmpty_POST.xml");
        }

        [TestMethod]
        public void Package_NoEmit()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.Package_NoEmit_PRE.xml", "Package.Package_NoEmit_POST.xml");
        }
        
        [TestMethod]
        public void Package_TransformationTemplateInstance()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.TransformationTemplateInstanceBasic_PRE.xml", "Package.TransformationTemplateInstanceBasic_POST.xml");
        }

        [TestMethod]
        public void IsNullPatcher_Basic()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.IsNullPatcher.Basic_PRE.xml", "Package.IsNullPatcher.Basic_POST.xml");
        }

        [TestMethod]
        public void IsNullPatcher_MultipleColumns()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.IsNullPatcher.MultipleColumns_PRE.xml", "Package.IsNullPatcher.MultipleColumns_POST.xml");
        }

        [TestMethod]
        public void IsNullPatcher_ValidateExternalMetadata()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.IsNullPatcher.ValidateExternalMetadata_PRE.xml", "Package.IsNullPatcher.ValidateExternalMetadata_POST.xml");
        }

        [TestMethod]
        public void LateArriving_Basic()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.LateArriving.Basic_PRE.xml", "Package.LateArriving.Basic_POST.xml");
        }

        // TODO: Add Support to Compare Different Dlasses from the Same Base Class. 
        /*********************************************************
         * DOES NOT WORK YET!
        [TestMethod]
        public void Staging_Basic()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.Staging.Basic_PRE.xml", "Package.Staging.Basic_POST.xml");
        }

        [TestMethod]
        public void StagingTable()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.Staging.StagingTable_PRE.xml", "Package.Staging.StagingTable_POST.xml");
        }
        ************************************************************/

        [TestMethod]
        public void LateArriving_InputsAndOutputs()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.LateArriving.InputsAndOutputs_PRE.xml", "Package.LateArriving.InputsAndOutputs_POST.xml");
        }

        [TestMethod]
        public void Scd_BasicDestination()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.Scd.BasicDestination_PRE.xml", "Package.Scd.BasicDestination_POST.xml");
        }

        [TestMethod]
        public void Scd_BasicMerge()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.Scd.BasicMerge_PRE.xml", "Package.Scd.BasicMerge_POST.xml");
        }

        [TestMethod]
        public void StoredProcedure_Basic()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.StoredProcedure.Basic_PRE.xml", "Package.StoredProcedure.Basic_POST.xml");
        }

        [TestMethod]
        public void StoredProcedure_ParametersBasic()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.StoredProcedure.ParametersBasic_PRE.xml", "Package.StoredProcedure.ParametersBasic_POST.xml");
        }

        [TestMethod]
        public void StoredProcedure_ParametersDefault()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.StoredProcedure.ParametersDefault_PRE.xml", "Package.StoredProcedure.ParametersDefault_POST.xml");
        }

        [TestMethod]
        public void StoredProcedure_ParametersOutput()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.StoredProcedure.ParametersOutput_PRE.xml", "Package.StoredProcedure.ParametersOutput_POST.xml");
        }

        [TestMethod]
        public void StoredProcedure_ParametersReadOnly()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.StoredProcedure.ParametersReadOnly_PRE.xml", "Package.StoredProcedure.ParametersReadOnly_POST.xml");
        }

        [TestMethod]
        public void StoredProcedure_Permission()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.StoredProcedure.Permission_PRE.xml", "Package.StoredProcedure.Permission_POST.xml");
        }

        [TestMethod]
        public void StoredProcedure_Event()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.StoredProcedure.Event_PRE.xml", "Package.StoredProcedure.Event_POST.xml");
        }

        [TestMethod]
        public void StoredProcedure_PrecedenceConstraint()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.StoredProcedure.PrecedenceConstraint_PRE.xml", "Package.StoredProcedure.PrecedenceConstraint_POST.xml");
        }

        [TestMethod]
        public void Merge_Basic()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.Merge.Basic_PRE.xml", "Package.Merge.Basic_POST.xml");
        }

        [TestMethod]
        public void Merge_Columns()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.Merge.Columns_PRE.xml", "Package.Merge.Columns_POST.xml");
        }

        [TestMethod]
        public void Merge_DelayValidation()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.Merge.DelayValidation_PRE.xml", "Package.Merge.DelayValidation_POST.xml");
        }

        [TestMethod]
        public void Merge_DisableScd()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.Merge.DisableSCD_PRE.xml", "Package.Merge.DisableSCD_POST.xml");
        }

        [TestMethod]
        public void Merge_WithEvent()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.Merge.Event_PRE.xml", "Package.Merge.Event_POST.xml");
        }

        [TestMethod]
        public void Merge_IsolationLevel()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.Merge.IsolationLevel_PRE.xml", "Package.Merge.IsolationLevel_POST.xml");
        }

        [TestMethod]
        public void Merge_NoUpdate()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.Merge.NoUpdate_PRE.xml", "Package.Merge.NoUpdate_POST.xml");
        }

        [TestMethod]
        public void Merge_PrecedenceConstraint()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.Merge.PrecedenceConstraint_PRE.xml", "Package.Merge.PrecedenceConstraint_POST.xml");
        }

        [TestMethod]
        public void Merge_Scd()
        {
            // TODO: Not sure this one is working right - historical handling seems to be off.
            Assert.Fail("Needs a design review to ensure correct behavior");
            ////DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.Merge.SCD_PRE.xml", "Package.Merge.SCD_POST.xml");
        }

        [TestMethod]
        public void Merge_UnspecifiedColumnDefaultUsageType()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.Merge.UnspecifiedColumnDefaultUsageType_PRE.xml", "Package.Merge.UnspecifiedColumnDefaultUsageType_POST.xml");
        }

        [TestMethod]
        public void Merge_UpdateTargetTable()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.Merge.UpdateTargetTable_PRE.xml", "Package.Merge.UpdateTargetTable_POST.xml");
        }

        [TestMethod]
        public void RetryContainer_Basic()
        {
            DefaultComparer.CheckResourceFrameworkItemsSubsetOf("Package.RetryContainer.Basic_PRE.xml", "Package.RetryContainer.Basic_POST.xml");
        }
    }
}
