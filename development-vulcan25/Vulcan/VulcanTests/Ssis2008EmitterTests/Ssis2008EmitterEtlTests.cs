using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VulcanTests.Ssis2008EmitterTests
{
    [TestClass]
    public class Ssis2008EmitterEtlTests
    {
        private static readonly SsisComparer DefaultComparer = SsisComparer.DefaultSsisComparer;

        [TestMethod]
        public void Etl_Basic()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ETL.Basic_PRE.xml", "Tasks.ETL.Basic_POST");
        }

        [TestMethod]
        public void Etl_DelayValidation()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ETL.DelayValidation_PRE.xml", "Tasks.ETL.DelayValidation_POST");
        }

        [TestMethod]
        public void Etl_IsolationLevelChaos()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ETL.IsolationLevelChaos_PRE.xml", "Tasks.ETL.IsolationLevelChaos_POST");
        }

        [TestMethod]
        public void Etl_Events()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ETL.Events_PRE.xml", "Tasks.ETL.Events_POST");
        }

        [TestMethod]
        public void Etl_PrecedenceConstraints()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ETL.PrecedenceConstraints_PRE.xml", "Tasks.ETL.PrecedenceConstraints_POST");
        }

        #region Transformation Tests

        [TestMethod]
        public void Etl_Transformations_QuerySourceBasic()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ETL.Transformations.QuerySourceBasic_PRE.xml", "Tasks.ETL.Transformations.QuerySourceBasic_POST");
        }

        [TestMethod]
        public void Etl_Transformations_DerivedColumnBasic()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ETL.Transformations.DerivedColumnBasic_PRE.xml", "Tasks.ETL.Transformations.DerivedColumnBasic_POST");
        }

        [TestMethod]
        public void RowCount_Basic()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ETL.RowCount_PRE.xml", "Tasks.ETL.RowCount_POST");
        }

        [TestMethod]
        public void Etl_Transformations_DerivedColumnErrorRowDisposition()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ETL.DerivedColumnErrorRowDisposition_PRE.xml", "Tasks.ETL.DerivedColumnErrorRowDisposition_POST");
        }

        [TestMethod]
        public void Etl_Transformations_QuerySourceParameters()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ETL.Transformations.QuerySourceParameters_PRE.xml", "Tasks.ETL.Transformations.QuerySourceParameters_POST");
        }

        [TestMethod]
        public void Etl_Transformations_ConditionalSplitBasic()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ETL.ConditionalSplitBasic_PRE.xml", "Tasks.ETL.ConditionalSplitBasic_POST");
        }

        [TestMethod]
        public void Etl_Transformations_DestinationBasic()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ETL.DestinationBasic_PRE.xml", "Tasks.ETL.DestinationBasic_POST");
        }

        [TestMethod]
        public void Etl_Transformations_DestinationFastLoad()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ETL.DestinationFastLoad_PRE.xml", "Tasks.ETL.DestinationFastLoad_POST");
        }

        [TestMethod]
        public void Etl_Transformations_LookupBasic()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ETL.LookupBasic_PRE.xml", "Tasks.ETL.LookupBasic_POST");
        }

        [TestMethod]
        public void Etl_Transformations_MulticastBasic()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ETL.MulticastBasic_PRE.xml", "Tasks.ETL.MulticastBasic_POST");
        }

        [TestMethod]
        public void Etl_Transformations_OleDBCommandBasic()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ETL.OleDbCommandBasic_PRE.xml", "Tasks.ETL.OleDbCommandBasic_POST");
        }

        [TestMethod]
        public void Etl_Transformations_SortBasic()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ETL.SortBasic_PRE.xml", "Tasks.ETL.SortBasic_POST");
        }

        [TestMethod]
        public void Etl_Transformations_UnionAllBasic()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ETL.UnionAllBasic_PRE.xml", "Tasks.ETL.UnionAllBasic_POST");
        }

        [TestMethod]
        public void Etl_Transformations_ScdBasic()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ETL.ScdBasic_PRE.xml", "Tasks.ETL.ScdBasic_POST");
        }
        
        [TestMethod]
        public void Etl_Transformations_TermLookupBasic()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ETL.TermLookupBasic_PRE.xml", "Tasks.ETL.TermLookupBasic_POST");
        }

        [TestMethod]
        public void Etl_Transformations_TransformationTemplateInstanceBasic()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ETL.TransformationTemplateInstanceBasic_PRE.xml", "Tasks.ETL.TransformationTemplateInstanceBasic_POST");
        }

        [TestMethod]
        public void Etl_Transformations_XmlSourceBasic()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ETL.XmlSourceBasic_PRE.xml", "Tasks.ETL.XmlSourceBasic_POST");
        }

        // Commented out until we decide if ETL Fragements are still in, or if we've replaced them with templates
        ////[TestMethod]
        ////public void Etl_Transformations_EtlFragmentBasic()
        ////{
        ////    DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ETL.EtlFragmentBasic_PRE.xml", "Tasks.ETL.EtlFragmentBasic_POST");
        ////}

        #endregion
    }
}
