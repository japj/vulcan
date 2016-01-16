using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VulcanTests.Ssis2008EmitterTests
{
    [TestClass]
    public class Ssis2008EmitterTaskTests
    {
        private static readonly SsisComparer DefaultComparer = SsisComparer.DefaultSsisComparer;

        [TestMethod]
        public void ExecuteSql_Basic()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ExecuteSql.Basic_PRE.xml", "Tasks.ExecuteSql.Basic_POST");
        }

        [TestMethod]
        public void ExecuteSql_CoreProperties()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ExecuteSql.CoreProperties_PRE.xml", "Tasks.ExecuteSql.CoreProperties_POST");
        }

        [TestMethod]
        public void ExecutePackage_Basic()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ExecutePackage.Basic_PRE.xml", "Tasks.ExecutePackage.Basic_POST");
        }

        [TestMethod]
        public void CommitPersistentVariable_Basic()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.CommitPersistentVariable.Basic_PRE.xml", "Tasks.CommitPersistentVariable.Basic_POST");
        }

        [TestMethod]
        public void Container_Basic()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.Container.Basic_PRE.xml", "Tasks.Container.Basic_POST");
        }

        [TestMethod]
        public void Container_ConstraintModeLinear()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.Container.ConstraintModeLinear_PRE.xml", "Tasks.Container.ConstraintModeLinear_POST");
        }

        [TestMethod]
        public void Container_ConstraintModeParallel()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.Container.ConstraintModeParallel_PRE.xml", "Tasks.Container.ConstraintModeParallel_POST");
        }

        [TestMethod]
        public void Container_DelayValidation()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.Container.DelayValidation_PRE.xml", "Tasks.Container.DelayValidation_POST");
        }

        [TestMethod]
        public void Container_IsolationLevelChaos()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.Container.IsolationLevelChaos_PRE.xml", "Tasks.Container.IsolationLevelChaos_POST");
        }

        [TestMethod]
        public void Container_Log()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.Container.Log_PRE.xml", "Tasks.Container.Log_POST");
        }

        [TestMethod]
        public void Container_TransactionModeNoTransaction()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.Container.TransactionModeNoTransaction_PRE.xml", "Tasks.Container.TransactionModeNoTransaction_POST");
        }

        [TestMethod]
        public void Container_TransactionModeRequiredTransaction()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.Container.TransactionModeRequiredTransaction_PRE.xml", "Tasks.Container.TransactionModeRequiredTransaction_POST");
        }

        [TestMethod]
        public void Container_PrecedenceConstraints()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.Container.PrecedenceConstraints_PRE.xml", "Tasks.Container.PrecedenceConstraints_POST");
        }

        [TestMethod]
        public void Container_Events()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.Container.Events_PRE.xml", "Tasks.Container.Events_POST");
        }

        [TestMethod]
        public void Container_Variables()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.Container.Variables_PRE.xml", "Tasks.Container.Variables_POST");
        }

        [TestMethod]
        public void Container_VariableExpression()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.Container.VariableExpression_PRE.xml", "Tasks.Container.VariableExpression_POST");
        }

        [TestMethod]
        public void ForLoop_Basic()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Tasks.ForLoop.Basic_PRE.xml", "Tasks.ForLoop.Basic_POST");
        }
    }
}
