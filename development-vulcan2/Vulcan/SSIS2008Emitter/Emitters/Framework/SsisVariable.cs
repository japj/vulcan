using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using DTS = Microsoft.SqlServer.Dts.Runtime;
using DTSTasks = Microsoft.SqlServer.Dts.Tasks;

using VulcanEngine.Common;
using Ssis2008Emitter.Utility;
using Ssis2008Emitter.IR.Framework;

namespace Ssis2008Emitter.Emitters.Framework
{
    public class SsisVariable
    {
        private DTS.Variable _dtsVariable;
        private DTS.DtsContainer _parentContainer;

        private string _name;
        private string _path;

        public SsisVariable(Variable variable) : this(variable.Name, variable.Value, (DTS.DtsContainer)null) { }

        public SsisVariable(string name, object value) : this(name, value, (DTS.DtsContainer)null) { }

        public SsisVariable(Variable variable, SSISEmitterContext context) : this(variable.Name, variable.Value, (DTS.DtsContainer)context.ParentContainer) { }

        public SsisVariable(string name, object value, SSISEmitterContext context) : this(name, value, (DTS.DtsContainer)context.ParentContainer) { }

        public SsisVariable(string name, object value, DTS.DtsContainer parentContainer)
        {
            if (null == parentContainer)
            {
                this._parentContainer = (DTS.DtsContainer)SsisPackage.CurrentPackage.DTSPackage;
            }
            else
            {
                this._parentContainer = parentContainer;
            }
            _dtsVariable = _addVariable(name, value, _parentContainer);
        }

        public void Delete()
        {
            if (null != _parentContainer && null != _dtsVariable)
            {
                _parentContainer.Variables.Remove(_dtsVariable.ID);
                _dtsVariable = null;
                _parentContainer = null;
            }
        }

        private DTS.Variable _addVariable(string name, object value, DTS.DtsContainer dtsContainer)
        {
            _name = name;

            SetVariablePath(dtsContainer);

            if (!dtsContainer.Variables.Contains(name))
            {
                MessageEngine.Global.Trace(Severity.Alert, "Creating variable {0} with value {1} in Container {2}", name, value, dtsContainer.Name);
                DTS.Variable var = dtsContainer.Variables.Add(name, false, "User", value);
                _dtsVariable = var;
                return var;
            }
            else
            {
                _dtsVariable = dtsContainer.Variables[name];
                return dtsContainer.Variables[name];
            }
        }

        private void SetVariablePath(DTS.DtsContainer dtsContainer)
        {
            StringBuilder sPath = new StringBuilder(dtsContainer.Name);

            while (dtsContainer.Parent != null)
            {
                dtsContainer = dtsContainer.Parent;
                sPath.Insert(0, "/");
                sPath.Insert(0, dtsContainer.Name);
            }

            sPath.Insert(0, "/Root/");
            _path = sPath.ToString();
        }

        public string GetVariablePathXML()
        {
            DTS.DtsContainer dtsContainer = _parentContainer;

            StringBuilder sPath = new StringBuilder();
            sPath.AppendFormat("<{0}></{1}>", dtsContainer.Name, dtsContainer.Name);

            while (dtsContainer.Parent != null)
            {
                dtsContainer = dtsContainer.Parent;
                sPath.Insert(0, ">");
                sPath.Insert(0, dtsContainer.Name);
                sPath.Insert(0, "<");
                sPath.AppendFormat("</{0}>", dtsContainer.Name);
            }

            sPath.Insert(0, "'<Root>");
            sPath.Append("</Root>'");
            return sPath.ToString();
        }

        public DTS.Variable DTSVariable
        {
            get { return _dtsVariable; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Path
        {
            get { return _path; }
        }

    }
}
