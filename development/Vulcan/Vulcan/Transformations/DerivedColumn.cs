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
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Vulcan.Common;
using Vulcan.Tasks;
using Vulcan.Common.Templates;
using Vulcan.Emitters;
using Vulcan.Properties;

using DTS = Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;

using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;


namespace Vulcan.Transformations
{
    public class DerivedColumns : Transformation
    {
        private IDTSComponentMetaData90 _dcCom;
        private CManagedComponentWrapper _dci;

        public DerivedColumns(Packages.VulcanPackage vulcanPackage, MainPipe dataFlowTask, IDTSComponentMetaData90 parentComponent, string name, string description )
            :
            base(
            vulcanPackage,
            dataFlowTask,
            parentComponent,
            name,
            description
            )
        {

            _dcCom = dataFlowTask.ComponentMetaDataCollection.New();
            _dcCom.ComponentClassID = "DTSTransform.DerivedColumn";
            
            //IMPORTANT! If you do not Instantiate() first, the component names do not get set... this is bad.

            _dci = _dcCom.Instantiate();
            _dci.ProvideComponentProperties();

            _dcCom.Name = Name;
            _dcCom.Description = Description;

            dataFlowTask.PathCollection.New().AttachPathAndPropagateNotifications(
                                                                                  parentComponent.OutputCollection[0],
                                                                                  _dcCom.InputCollection[0]
                                                                                  );

            _dci.AcquireConnections(null);
            _dci.ReinitializeMetaData();
            _dci.ReleaseConnections();

        }
        
        public void AddOutputColumn(string colName, DataType type, string expression, int length, int precision, int scale, int codepage)
        {
            this.AddOutputColumn(colName, type, expression, length, precision, scale, codepage, DTSUsageType.UT_READONLY);
        }

        public string ExpressionCleanerAndInputMapBuilder(string expression, IDTSVirtualInput90 vi, DTSUsageType inputColumnUsageType)
        {
             foreach (IDTSVirtualInputColumn90 vcol in vi.VirtualInputColumnCollection)
            {
                Regex regex = new Regex(@"\b" + Regex.Escape(vcol.Name) + @"\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                //Slow, speed this up with a proper lookup and parse
                if (regex.IsMatch(expression))
                {
                    SetInputUsageType(vi,vcol, inputColumnUsageType);
                    expression = regex.Replace(expression, "#" + vcol.LineageID);
                }
             }

             return expression;
        }
        public void AddOutputColumn(string colName, DataType type, string expression, int length, int precision, int scale, int codepage, DTSUsageType inputColumnUsageType)
        {
            IDTSOutputColumn90 col = _dcCom.OutputCollection[0].OutputColumnCollection.New();
            col.Name = colName;
            col.Description = colName;
            col.ErrorRowDisposition = DTSRowDisposition.RD_IgnoreFailure;
            col.TruncationRowDisposition = DTSRowDisposition.RD_FailComponent;
            col.SetDataTypeProperties(type, length, precision, scale, codepage);
            col.ExternalMetadataColumnID = 0;

            IDTSCustomProperty90 propExpression = col.CustomPropertyCollection.New();
            propExpression.Name = "Expression";
            propExpression.Value = expression;

            IDTSCustomProperty90 propFriendlyExpression = col.CustomPropertyCollection.New();
            propFriendlyExpression.Name = "FriendlyExpression";
            propFriendlyExpression.Value = expression;

            IDTSVirtualInput90 vi = _dcCom.InputCollection[0].GetVirtualInput();

            propExpression.Value = this.ExpressionCleanerAndInputMapBuilder(expression, vi, inputColumnUsageType);
        }

        public override IDTSComponentMetaData90 Component
        {
            get
            {
                return this._dcCom;
            }
        }

        public override CManagedComponentWrapper ComponentInstance
        {
            get
            {
                return this._dci;
            }
        }
    }
}
