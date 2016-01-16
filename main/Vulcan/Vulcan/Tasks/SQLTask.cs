/*
 * Microsoft Public License (Ms-PL)
 * 
 * This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.
 * 
 * 1. Definitions
 * The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
 * A "contribution" is the original software, or any additions or changes to the software.
 * A "contributor" is any person that distributes its contribution under this license.
 * "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 * 
 * 2. Grant of Rights
 * (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 * (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 * 
 * 3. Conditions and Limitations
 * (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
 * (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
 * (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
 * (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
 * (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement. 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Reflection;

using Vulcan.Common;
using Vulcan.Common.Templates;
using Vulcan.Properties;

using DTS = Microsoft.SqlServer.Dts.Runtime;
using DTSTasks = Microsoft.SqlServer.Dts.Tasks;

namespace Vulcan.Tasks
{
    public class SQLTask : Task
    {
        private DTS.TaskHost _sqlTask;
        private Vulcan.Common.Helpers.ExpressionPathBuilder _expressionBuilder;
        private int _parameterIndex = 0;
        public SQLTask(Packages.VulcanPackage package, string name, string description, DTS.IDTSSequence parentContainer,Connection destinationConnection)
            :
            this(
            package,
            name, 
            description, 
            parentContainer, 
            destinationConnection,
            null
            )
        {
        }

        public SQLTask(Packages.VulcanPackage package, string name, string description, DTS.IDTSSequence parentContainer, Connection destinationConnection, Dictionary<string,object> propertiesDictionary)
            :
            base(
            package,
            name,
            description,
            parentContainer
            )
        {
            this._expressionBuilder = new Vulcan.Common.Helpers.ExpressionPathBuilder(package);

            _sqlTask = (DTS.TaskHost)parentContainer.Executables.Add("STOCK:SQLTask");
            _sqlTask.Name = name;
            _sqlTask.Description = description;

            SetProperties(propertiesDictionary);
            if (destinationConnection != null)
            {
                SetProperty("Connection", destinationConnection.ConnectionManager.Name);
            }
        }

        public void TransmuteToFileTask(string sqlFileName)
        {
            string expression = _expressionBuilder.BuildRelativeExpression("varRootDir", VulcanPackage.ProjectSubpath,VulcanPackage.Name, sqlFileName);
            Connection c = new Connection(VulcanPackage,sqlFileName,sqlFileName,"File",expression);
            ExecuteSQLTask.SqlStatementSourceType = Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.SqlStatementSourceType.FileConnection;
            ExecuteSQLTask.SqlStatementSource = c.ConnectionManager.Name;
        }

        public void TransmuteToExpressionTask(string expression)
        {
            ExecuteSQLTask.SqlStatementSourceType = Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.SqlStatementSourceType.DirectInput;
            ExecuteSQLTask.SqlStatementSource = "PLACEHOLDER";
            SQLTaskHost.SetExpression("SqlStatementSource", expression);
        }

        public void BindResult(string resultName, string variableName)
        {
            DTSTasks.ExecuteSQLTask.IDTSResultBindings resultBindings = this.ExecuteSQLTask.ResultSetBindings;
            DTSTasks.ExecuteSQLTask.IDTSResultBinding result = resultBindings.Add();
            result.ResultName = resultName;

            if (VulcanPackage.DTSPackage.Variables.Contains(variableName))
            {
                result.DtsVariableName = VulcanPackage.DTSPackage.Variables[variableName].QualifiedName;
            }
            else
            {
                Message.Trace(Severity.Error, "Task {0}: Could not Bind ResultSet: Variable {1} does not exist",this.Name, variableName);
            }
        }


        public void BindParameter(DTS.Variable variable, Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections direction, int dataType)
        {
            BindParameter(variable, direction,dataType, -1);
        }
        public void BindParameter(DTS.Variable variable, Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections direction, int dataType, int size)
        {
            BindParameter(variable, direction, _parameterIndex.ToString(), dataType, size);
            _parameterIndex++;
        }
        public void BindParameter(DTS.Variable variable, Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ParameterDirections direction, string parameterName, int dataType, int size)
        {
            DTSTasks.ExecuteSQLTask.IDTSParameterBinding binding = ExecuteSQLTask.ParameterBindings.Add();
            binding.DtsVariableName = variable.QualifiedName;
            binding.ParameterDirection = direction;
            binding.ParameterName = parameterName;
            binding.ParameterSize = size;
            binding.DataType = dataType;

        }

        public override void SetProperty(string name, object value)
        {
            _sqlTask.Properties[name].SetValue(_sqlTask, value);
        }
        
        public DTS.TaskHost SQLTaskHost
        {
            get
            {
                return this._sqlTask;
            }
        }

        public DTSTasks.ExecuteSQLTask.ExecuteSQLTask ExecuteSQLTask
        {
            get
            {
                return (DTSTasks.ExecuteSQLTask.ExecuteSQLTask)_sqlTask.InnerObject;
            }
        }

        public override void SetExpression(string name, string expression)
        {
            throw new NotSupportedException("This SQLTask does not support Expressions");
        }
    }
}
