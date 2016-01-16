using System;
using System.Globalization;
using System.Text;
using Ssis2008Emitter.IR.Common;
using AST = VulcanEngine.IR.Ast;
using DTS = Microsoft.SqlServer.Dts.Runtime;

namespace Ssis2008Emitter.IR.Framework
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public class Variable : PhysicalObject, IPhysicalPropertiesProvider
    {
        #region Private Storage

        private DTS.Variable _dtsVariable;
        private DTS.DtsContainer _parentContainer;

        #endregion  // Private Storage

        public bool IsSystemVariable { get; set; }

        public bool EvaluateAsExpression { get; set; }

        public string InheritFromPackageParentConfigurationString { get; set; }

        public TypeCode TypeCode { get; set; }

        public object Value
        {
            get 
            { 
                switch(TypeCode)
                {
                    case TypeCode.DBNull:
                        return DBNull.Value;
                    case TypeCode.Object:
                        return new Object();
                    default:
                        return Convert.ChangeType(ValueString, TypeCode, CultureInfo.InvariantCulture);
                }
            }
        }

        public string ValueString { get; set; }

        public Variable(AST.Task.AstVariableNode astNode) : base(astNode.Name)
        {
            TypeCode = (TypeCode)Enum.Parse(typeof(TypeCode), astNode.TypeCode.ToString());
            ValueString = astNode.Value;
            EvaluateAsExpression = astNode.EvaluateAsExpression;
            InheritFromPackageParentConfigurationString = astNode.InheritFromPackageParentConfigurationString;
            IsSystemVariable = astNode.IsSystemVariable;
        }

        public Variable(string name) : base(name)
        {
            TypeCode = TypeCode.Object;
            ValueString = null;
            IsSystemVariable = false;
        }

        public override void Initialize(SsisEmitterContext context)
        {
            _parentContainer = context.ParentContainer.DtsContainer;
            SetVariablePath(_parentContainer);

            if (_parentContainer.Variables.Contains(Name))
            {
                _dtsVariable = _parentContainer.Variables[Name];
            }
            else
            {
                if (EvaluateAsExpression)
                {
                    _dtsVariable = _parentContainer.Variables.Add(Name, false, "User", null);
                    _dtsVariable.EvaluateAsExpression = true;
                    _dtsVariable.Expression = ValueString;
                }
                else
                {
                    _dtsVariable = _parentContainer.Variables.Add(Name, false, "User", Value);
                }

                if (!String.IsNullOrEmpty(InheritFromPackageParentConfigurationString))
                {
                    var parentConfig = context.Package.DtsPackage.Configurations.Add();
                    parentConfig.ConfigurationType = Microsoft.SqlServer.Dts.Runtime.DTSConfigurationType.ParentVariable;
                    parentConfig.Name = _dtsVariable.Name;
                    parentConfig.PackagePath = String.Format(CultureInfo.InvariantCulture, @"\Package.Variables[{0}].Properties[Value]", _dtsVariable.QualifiedName);
                    parentConfig.ConfigurationString = InheritFromPackageParentConfigurationString;
                }
            }
        }

        public override void Emit(SsisEmitterContext context)
        {
        }

        public DTS.Variable DtsVariable
        {
            get { return _dtsVariable; }
        }

        public DTS.IDTSPropertiesProvider PropertyProvider
        {
            get { return _dtsVariable; }
        }

        public void SetProperty(string name, object value)
        {
            PropertyProvider.Properties[name].SetValue(PropertyProvider, value);
        }

        public void SetExpression(string name, string expression)
        {
            // Validation: Expression length must be <= 4000 characters - SSIS 2008 CU#1 limitation
            PropertyProvider.SetExpression(name, expression);
        }

        public string VariablePathXml
        {
            get
            {
                DTS.DtsContainer dtsContainer = _parentContainer;

                var path = new StringBuilder();
                path.AppendFormat("<{0}></{1}>", dtsContainer.Name, dtsContainer.Name);

                while (dtsContainer.Parent != null)
                {
                    dtsContainer = dtsContainer.Parent;
                    path.Insert(0, ">");
                    path.Insert(0, dtsContainer.Name);
                    path.Insert(0, "<");
                    path.AppendFormat("</{0}>", dtsContainer.Name);
                }

                path.Insert(0, "'<Root>");
                path.Append("</Root>'");
                return path.ToString();
            }
        }

        private static void SetVariablePath(DTS.DtsContainer dtsContainer)
        {
            var path = new StringBuilder(dtsContainer.Name);

            while (dtsContainer.Parent != null)
            {
                dtsContainer = dtsContainer.Parent;
                path.Insert(0, "/");
                path.Insert(0, dtsContainer.Name);
            }

            path.Insert(0, "/Root/");
        }
    }
}
