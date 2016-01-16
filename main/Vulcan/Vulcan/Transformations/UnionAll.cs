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
using Vulcan.Packages;
using Vulcan.Emitters;
using Vulcan.Properties;

using DTS = Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;

using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace Vulcan.Transformations
{
    public class UnionAll : Transformation
    {
        IDTSComponentMetaData90 unionAllCom;
        CManagedComponentWrapper unionAllComI;

        public UnionAll(
           VulcanPackage vulcanPackage,
           MainPipe dataFlowTask,
           IDTSComponentMetaData90
           parentComponent,
           string name,
           string description
           )
            :
           base(
           vulcanPackage,
           dataFlowTask,
           parentComponent,
           name,
           description
           )
        {
            unionAllCom = dataFlowTask.ComponentMetaDataCollection.New();
            unionAllCom.ComponentClassID = "DTSTransform.UnionAll";

            unionAllComI = unionAllCom.Instantiate();
            unionAllComI.ProvideComponentProperties();

            unionAllCom.Name = name;

            unionAllCom.OutputCollection[0].ErrorRowDisposition = DTSRowDisposition.RD_NotUsed;
        }

        public void InitializeAndMapDestination()
        {
            unionAllComI.AcquireConnections(null);
            unionAllComI.ReinitializeMetaData();
            unionAllComI.ReleaseConnections();
        }

        internal void MapInput(string sourceComponentName, MainPipe dataFlowTask)
        {
            IDTSComponentMetaData90 sourceComponent = DataFlowTask.ComponentMetaDataCollection[sourceComponentName];

            dataFlowTask.PathCollection.New().AttachPathAndPropagateNotifications(
                                                                               sourceComponent.OutputCollection[0],
                                                                               unionAllCom.InputCollection["Input for " + sourceComponentName]
                                                                               );
            InitializeAndMapDestination();
        }

        internal void MapInputColumn(string sourceComponentName, string sourceColumnName, string destinationColumnName, bool unMap)
        {
            IDTSVirtualInput90 cvi = unionAllCom.InputCollection["Input for " + sourceComponentName].GetVirtualInput();
            Dictionary<string, IDTSVirtualInputColumn90> virtualInputColumnsDictionary = new Dictionary<string, IDTSVirtualInputColumn90>();
            foreach (IDTSVirtualInputColumn90 inputColumn in cvi.VirtualInputColumnCollection)
            {
                virtualInputColumnsDictionary[inputColumn.Name] = inputColumn;
            }

            IDTSInput90 input = unionAllCom.InputCollection["Input for " + sourceComponentName];
            Dictionary<string, IDTSInputColumn90> inputColumnsDictionary = new Dictionary<string, IDTSInputColumn90>();
            foreach (IDTSInputColumn90 inputColumn in input.InputColumnCollection)
            {
                inputColumnsDictionary[inputColumn.Name] = inputColumn;
            }

            if (!inputColumnsDictionary.ContainsKey(sourceColumnName))
            {
                IDTSInputColumn90 newInputColumn = input.InputColumnCollection.New();
                newInputColumn.Name = sourceColumnName;

                IDTSCustomProperty90 newInputColumnCustomProperty = newInputColumn.CustomPropertyCollection.New();
                newInputColumnCustomProperty.Name = "OutputColumnLineageID";
                newInputColumnCustomProperty.Value = unionAllCom.OutputCollection[0].OutputColumnCollection[destinationColumnName].LineageID;
            }

        }

        public override IDTSComponentMetaData90 Component
        {
            get
            {
                return this.unionAllCom;
            }
        }

        public override CManagedComponentWrapper ComponentInstance
        {
            get
            {
                return this.unionAllComI;
            }
        }


    }
}
