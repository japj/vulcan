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
    public class OLEDBCommand : Transformation
    {
        private IDTSComponentMetaData90 _oledbCom;
        private CManagedComponentWrapper _oledbComI;

        public OLEDBCommand(
            VulcanPackage vulcanPackage,
            MainPipe dataFlowTask,
            IDTSComponentMetaData90
            parentComponent,
            string name,
            string description,
            Connection connection,
            string command
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

            _oledbCom = dataFlowTask.ComponentMetaDataCollection.New();
            _oledbCom.ComponentClassID = "DTSTransform.OLEDBCommand";
            
            //IMPORTANT! If you do not Instantiate() first, the component names do not get set... this is bad.
            _oledbComI = _oledbCom.Instantiate();
            _oledbComI.ProvideComponentProperties();

            _oledbCom.Name        = name;
            _oledbCom.Description = description;

            _oledbCom.RuntimeConnectionCollection[0].ConnectionManagerID = connection.ConnectionManager.ID;
            _oledbCom.RuntimeConnectionCollection[0].ConnectionManager =
                DTS.DtsConvert.ToConnectionManager90(connection.ConnectionManager);

            _oledbComI.SetComponentProperty("SqlCommand", command);

            dataFlowTask.PathCollection.New().AttachPathAndPropagateNotifications(
                                                                                parentComponent.OutputCollection[0],
                                                                                _oledbCom.InputCollection[0]
                                                                                );

            try
            {
                _oledbComI.AcquireConnections(null);
                _oledbComI.ReinitializeMetaData();
                _oledbComI.ReleaseConnections();
            }
            catch (System.Runtime.InteropServices.COMException ce)
            {
                Message.Trace(Severity.Error,ce,"OLEDBCommand: {3}: {2}: Source {0}: Command {1}", connection.ConnectionManager.Name, command, ce.Message, _oledbCom.GetErrorDescription(ce.ErrorCode));
            }
            catch (Exception e)
            {
                Message.Trace(Severity.Error,e,"OLEDBCommand: {2}: Source {0}: Command {1}", connection.ConnectionManager.Name, command, e.Message);
            }

            AutoMap();
        }

        /// <summary>
        /// Automap must ALWAYS be called BEFORE Map() otherwise bad things will happen to the SSIS Metadata.  
        /// </summary>
        protected void AutoMap()
        {
            IDTSVirtualInput90 cvi = _oledbCom.InputCollection[0].GetVirtualInput();

            Dictionary<string, string> virtualInputDictionary = new Dictionary<string, string>();
            foreach (IDTSVirtualInputColumn90 vc in cvi.VirtualInputColumnCollection)
            {
                virtualInputDictionary["@" + vc.Name.ToUpperInvariant()] = vc.Name;
            }

            foreach (IDTSExternalMetadataColumn90 extCol in _oledbCom.InputCollection[0].ExternalMetadataColumnCollection)
            {
                if (virtualInputDictionary.ContainsKey(extCol.Name.ToUpperInvariant()))
                {
                    this.Map(virtualInputDictionary[extCol.Name.ToUpperInvariant()], extCol.Name,false);
                }
            }
        }

        public void Map(string source, string destination, bool unMap)
        {
            this.SafeMapInputToExternalMetadataColumn(source, destination, unMap);
        }

        public override IDTSComponentMetaData90 Component
        {
            get 
            { 
                return _oledbCom; 
            }
        }

        public override CManagedComponentWrapper ComponentInstance
        {
            get 
            {
                return _oledbComI;
            }
        }
    }
}
