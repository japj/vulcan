using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VulcanTests.Ssis2008EmitterTests
{
    [TestClass]
    public class Ssis2008EmitterPackageTests
    {
        private static readonly SsisComparer DefaultComparer = SsisComparer.DefaultSsisComparer;

        [TestMethod]
        public void Package_PrecedenceConstraints()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Package.PrecedenceConstraints_PRE.xml", "Package.PrecedenceConstraints_POST");
        }

        [TestMethod]
        public void Package_Events()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Package.Events_PRE.xml", "Package.Events_POST");
        }

        [TestMethod]
        public void Package_Variables()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Package.Variables_PRE.xml", "Package.Variables_POST");
        }

        [TestMethod]
        public void Package_VariableExpression()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Package.VariableExpression_PRE.xml", "Package.VariableExpression_POST");
        }
        
        [TestMethod]
        public void Package_Log()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Package.Log_PRE.xml", "Package.Log_POST");
        }

        [TestMethod]
        public void Package_ConstraintModeLinear()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Package.ConstraintModeLinear_PRE.xml", "Package.ConstraintModeLinear_POST");
        }

        [TestMethod]
        public void Package_ConstraintModeParallel()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Package.ConstraintModeParallel_PRE.xml", "Package.ConstraintModeParallel_POST");
        }

        [TestMethod]
        public void Package_DelayValidation()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Package.DelayValidation_PRE.xml", "Package.DelayValidation_POST");
        }

        [TestMethod]
        public void Package_IsolationLevelChaos()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Package.IsolationLevelChaos_PRE.xml", "Package.IsolationLevelChaos_POST");
        }

        [TestMethod]
        public void Package_TransactionModeNoTransaction()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Package.TransactionModeNoTransaction_PRE.xml", "Package.TransactionModeNoTransaction_POST");
        }

        [TestMethod]
        public void Package_TransactionModeRequiredTransaction()
        {
            DefaultComparer.CompareResourceBimlWithDtsx("Package.TransactionModeRequiredTransaction_PRE.xml", "Package.TransactionModeRequiredTransaction_POST");
        }
    }
}
