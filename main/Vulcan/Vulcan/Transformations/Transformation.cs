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
    public abstract class Transformation
    {
        private VulcanPackage _vulcanPackage;
        private MainPipe _dataFlowTask;
        private IDTSComponentMetaData90 _parentComponent;
        private string _name;
        private string _description;

        protected Transformation(
            Packages.VulcanPackage vulcanPackage,
            MainPipe dataFlowTask,
            IDTSComponentMetaData90 parentComponent,
            string name,
            string description
            )
        {
            this._vulcanPackage = vulcanPackage;
            this._dataFlowTask = dataFlowTask;
            this._parentComponent = parentComponent;
            this._name = name;
            this._description = description;
        }

        public virtual void SetInputUsageType(string name, DTSUsageType usageType)
        {
            IDTSVirtualInput90 vi = Component.InputCollection[0].GetVirtualInput();
            IDTSVirtualInputColumn90 vcol = vi.VirtualInputColumnCollection[name];
            SetInputUsageType(vi, vcol, usageType);

        }

        public virtual void SetInputUsageType(IDTSVirtualInput90 vi, IDTSVirtualInputColumn90 vcol, DTSUsageType usageType)
        {
            this.SetInputUsageType(vi, vcol, usageType, false);
        }

        public virtual void SetInputUsageType(IDTSVirtualInput90 vi, IDTSVirtualInputColumn90 vcol, DTSUsageType usageType, bool forceOverwrite)
        {
            // This will not make a DT_READWRITE column more restrctive :)  Only less restrictive
            if (vcol.UsageType != DTSUsageType.UT_READWRITE || forceOverwrite)
            {
                ComponentInstance.SetUsageType(
                    Component.InputCollection[0].ID,
                    vi,
                    vcol.LineageID,
                    usageType
                    );
            }
        }

        protected virtual void SafeMapInputToExternalMetadataColumn(string inputColumnName, string externalMetadataColumnName, bool unMap)
        {
            try
            {
                IDTSVirtualInput90 cvi = Component.InputCollection[0].GetVirtualInput();
                IDTSExternalMetadataColumn90 eCol = Component.InputCollection[0].ExternalMetadataColumnCollection[externalMetadataColumnName];

                foreach (IDTSInputColumn90 inCol in Component.InputCollection[0].InputColumnCollection)
                {
                    //Unmap anything else that maps to this external metadata column)
                    if (inCol.ExternalMetadataColumnID == eCol.ID)
                    {
                        Message.Trace(Severity.Debug, "{0}: {1} Unmapping Input {2}", this.GetType(), this._name, inCol.Name);
                        this.SetInputUsageType(cvi, cvi.VirtualInputColumnCollection[inCol.Name], DTSUsageType.UT_IGNORED, true);
                        break;
                    }
                }
                if (!unMap)
                {
                    this.SetInputUsageType(cvi, cvi.VirtualInputColumnCollection[inputColumnName], DTSUsageType.UT_READONLY);
                    Component.InputCollection[0].InputColumnCollection[inputColumnName].ExternalMetadataColumnID = eCol.ID;
                }
            }
            catch (System.Runtime.InteropServices.COMException ce)
            {
                Message.Trace(Severity.Warning,Resources.WarningMapColumnsDoNotExist, inputColumnName, externalMetadataColumnName, Name,ce.Message);
            }
        }

        public virtual string Name
        {
            get
            {
                return this._name;
            }
        }

        public virtual string Description
        {
            get
            {
                return this._description;
            }
        }

        public IDTSComponentMetaData90 ParentComponent
        {
            get
            {
                return _parentComponent;
            }
        }

        public MainPipe DataFlowTask
        {
            get
            {
                return _dataFlowTask;
            }
        }

        public VulcanPackage VulcanPackage
        {
            get
            {
                return _vulcanPackage;
            }
        }

        public abstract IDTSComponentMetaData90 Component
        {
            get;
        }

        public abstract CManagedComponentWrapper ComponentInstance
        {
            get;
        }

    }
}
