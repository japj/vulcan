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
    public class TermLookup : Transformation
    {
        public enum InputColumnUsageType
        {
            PassThrough,
            Lookup,
            BothPassThroughAndLookup
        }

        IDTSComponentMetaData90 TermLookupCom;
        CManagedComponentWrapper TermLookupComI;

        public TermLookup(
           VulcanPackage vulcanPackage,
           MainPipe dataFlowTask,
           IDTSComponentMetaData90 parentComponent,
           string name,
           string description,
           Connection connection,
            bool isCaseSensitive,
            string refTermColumn,
            string refTermTable
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
            TermLookupCom = dataFlowTask.ComponentMetaDataCollection.New();
            TermLookupCom.ComponentClassID = "DTSTransform.TermLookup";

            TermLookupComI = TermLookupCom.Instantiate();
            TermLookupComI.ProvideComponentProperties();

            TermLookupCom.Name = name;

            TermLookupCom.RuntimeConnectionCollection[0].ConnectionManagerID = connection.ConnectionManager.ID;
            TermLookupCom.RuntimeConnectionCollection[0].ConnectionManager =
                DTS.DtsConvert.ToConnectionManager90(connection.ConnectionManager);

            TermLookupComI.SetComponentProperty("IsCaseSensitive", isCaseSensitive);
            TermLookupComI.SetComponentProperty("RefTermColumn", refTermColumn);
            TermLookupComI.SetComponentProperty("RefTermTable", refTermTable);
            TermLookupCom.OutputCollection[0].ErrorRowDisposition = DTSRowDisposition.RD_IgnoreFailure;

            dataFlowTask.PathCollection.New().AttachPathAndPropagateNotifications(
                                                                                  parentComponent.OutputCollection[0],
                                                                                  TermLookupCom.InputCollection[0]
                                                                                  );
            TermLookupComI.AcquireConnections(null);
            TermLookupComI.ReinitializeMetaData();
            TermLookupComI.ReleaseConnections();
        }

        public void MapInput(string inputColumnName, TermLookup.InputColumnUsageType inputColumnUsageType)
        {
            this.SetInputUsageType(inputColumnName, DTSUsageType.UT_READONLY);
            
            ComponentInstance.SetInputColumnProperty(
                Component.InputCollection[0].ID,
                Component.InputCollection[0].InputColumnCollection[inputColumnName].ID,
                "InputColumnType",
                (int)inputColumnUsageType
                );
        }

        public override IDTSComponentMetaData90 Component
        {
            get
            {
                return this.TermLookupCom;
            }
        }

        public override CManagedComponentWrapper ComponentInstance
        {
            get
            {
                return this.TermLookupComI;
            }
        }
    }
}
