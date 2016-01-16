using System;
using System.Collections.Generic;
using System.Text;

using DTS = Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

using VulcanEngine.Common;
using Ssis2008Emitter.Properties;
using Ssis2008Emitter.Utility;
using Ssis2008Emitter.IR.Task;
using Ssis2008Emitter.IR.DataFlow;
using Ssis2008Emitter.Emitters;
using Ssis2008Emitter.Emitters.Framework;
using Ssis2008Emitter.Emitters.Task;
using Ssis2008Emitter.IR.Framework;

namespace Ssis2008Emitter.Emitters.DataFlow
{
    [PhysicalIRMapping(typeof(OLEDBSource))]
    public class SsisSourceComponent : SsisComponent, ISSISEmitter
    {
        public override string ClassID
        {
            get { return "DTSAdapter.OleDbSource"; }
        }

        public SsisSourceComponent(Transformation t, SSISEmitterContext context)
            : base(t, context)
        {
            OLEDBSource source = (OLEDBSource)t;

            InitializeConnection(source.Connection);
            SetOutputName(source.OutputPath);

            SetComponentProperties(source);
            SetParameterMapping(source.ParameterMappings, SsisPackage.CurrentPackage.DTSPackage.Variables);

            Validate();
        }

        public SSISEmitterContext Emit()
        {
            _dataFlowTask.NewDataFlow();
            _dataFlowTask.ChainComponent(this);
            return _context;
        }

        private void SetComponentProperties(string sQuery, string sConnectionManagerName)
        {
            _instance.SetComponentProperty("AccessMode", 2);
            _instance.SetComponentProperty("SqlCommand", sQuery);

            Flush();
        }

        private void SetComponentProperties(OLEDBSource source)
        {
            switch (source.AccessMode)
            {
                case DataFlowSourceQueryAccessMode.SQLCOMMAND:
                    _instance.SetComponentProperty("AccessMode", 2);
                    _instance.SetComponentProperty("SqlCommand", source.Body);
                    Flush();
                    break;
                case DataFlowSourceQueryAccessMode.SQLCOMMANDFROMVARIABLE:
                    _instance.SetComponentProperty("AccessMode", 3);

                    DTS.Variable sqlCommandVariable = SsisPackage.CurrentPackage.DTSPackage.Variables.Add(
                            ("var"+Guid.NewGuid().ToString()).Replace("-", string.Empty)
                          , false
                          , "User"
                          , string.Empty);
                    sqlCommandVariable.EvaluateAsExpression = true;
                    sqlCommandVariable.Expression = source.Body;
                    _instance.SetComponentProperty("SqlCommandVariable", sqlCommandVariable.Name);
                    Flush();
                    break;
                default:
                    MessageEngine.Global.Trace(Severity.Error, "Unknown Source Data Access Mode Type of {0}", source.AccessMode);
                    break;
            }
        }

        private void SetParameterMapping(IList<DataFlowSourceQueryParameter> parameters, DTS.Variables variables)
        {
            StringBuilder parameterBuilder = new StringBuilder();
            foreach (DataFlowSourceQueryParameter param in parameters)
            {
                if (variables.Contains(param.VariableName))
                {
                    DTS.Variable variable = variables[param.VariableName];
                    parameterBuilder.AppendFormat("\"{0}\",{1};", param.Name, variable.ID);
                }
                else
                {
                    MessageEngine.Global.Trace(Severity.Error, Resources.DTSVariableNotExists, param.VariableName);
                }
            }

            _instance.SetComponentProperty("ParameterMapping", parameterBuilder.ToString());
        }
    }
}
