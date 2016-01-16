using System;
using System.Collections.Generic;
using System.Text;

using DTS = Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

using VulcanEngine.Common;
using Ssis2008Emitter.Properties;
using Ssis2008Emitter.Utility;
using Ssis2008Emitter.IR.DataFlow;
using Ssis2008Emitter.Emitters.Task;

namespace Ssis2008Emitter.Emitters.DataFlow
{
    [PhysicalIRMapping(typeof(Destination))]
    public class SsisOleDbDestination : SsisComponent, ISSISEmitter
    {
        public override string ClassID
        {
            get { return "DTSAdapter.OleDbDestination"; }
        }

        public SsisOleDbDestination(Transformation t, SSISEmitterContext context) : base(t, context)
        {
            Destination destination = (Destination)_transformation;
            _component.Name = t.Name;
            SetupComponentProperties();
            InitializeConnection(destination.Connection);
        }

        public SSISEmitterContext Emit()
        {
            MessageEngine.Global.Trace(Severity.Notification, Resources.EmittingDestination, _transformation.Name);
            _dataFlowTask.ChainComponent(this, _transformation.InputPath);
            this.AutoMap();
            return _context;
        }

        public void AutoMap()
        {
            IDTSVirtualInput100 cvi = _component.InputCollection[0].GetVirtualInput();

            Dictionary<string, IDTSVirtualInputColumn100> virtualInputDictionary = new Dictionary<string, IDTSVirtualInputColumn100>();
            foreach (IDTSVirtualInputColumn100 vc in cvi.VirtualInputColumnCollection)
            {
                virtualInputDictionary[vc.Name.ToUpperInvariant()] = vc;
            }

            // Automatically map columns
            foreach (IDTSExternalMetadataColumn100 extCol in _component.InputCollection[0].ExternalMetadataColumnCollection)
            {
                if (virtualInputDictionary.ContainsKey(extCol.Name.ToUpperInvariant()))
                {
                    IDTSVirtualInputColumn100 vc = virtualInputDictionary[extCol.Name.ToUpperInvariant()];
                    _instance.SetUsageType(_component.InputCollection[0].ID, cvi, vc.LineageID, DTSUsageType.UT_READONLY);
                    _component.InputCollection[0].InputColumnCollection[vc.Name].ExternalMetadataColumnID = extCol.ID;
                }
            }

            // Map any overrides
            foreach (Mapping mapping in ((Destination)_transformation).Mappings)
            {
                string dest;
                bool unMap = false;

                if (mapping.Destination == null)
                {
                    unMap = true;
                    dest = mapping.Source;
                }
                else
                {
                    dest = mapping.Destination;
                }

                Map(mapping.Source, dest, unMap);
            }
        }

        private void Map(string source, string destination, bool unMap)
        {
            SafeMapInputToExternalMetadataColumn(source, destination, unMap);
        }

        private void SetupComponentProperties()
        {
            Destination destination = (Destination)_transformation;

            switch (destination.AccessMode.ToUpperInvariant())
            {
                case "TABLE":
                    _instance.SetComponentProperty("AccessMode", 0);
                    _instance.SetComponentProperty("OpenRowset", destination.Table);
                    break;
                case "TABLEFASTLOAD":
                    _instance.SetComponentProperty("AccessMode", 3);
                    _instance.SetComponentProperty("OpenRowset", destination.Table);

                    _instance.SetComponentProperty("FastLoadKeepIdentity", destination.KeepIdentity);
                    _instance.SetComponentProperty("FastLoadKeepNulls", destination.KeepNulls);
                    _instance.SetComponentProperty("FastLoadMaxInsertCommitSize", destination.MaximumInsertCommitSize);

                    StringBuilder fastLoadOptions = new StringBuilder();
                    if (destination.TableLock)
                    {
                        fastLoadOptions.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "TABLOCK,");
                    }
                    if (destination.CheckConstraints)
                    {
                        fastLoadOptions.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "CHECK_CONSTRAINTS,");
                    }
                    if (destination.RowsPerBatch > 0)
                    {
                        fastLoadOptions.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "ROWS_PER_BATCH = {0}", destination.RowsPerBatch);
                    }
                    fastLoadOptions = fastLoadOptions.Replace(",", "", fastLoadOptions.Length - 5, 5);

                    _instance.SetComponentProperty("FastLoadOptions", fastLoadOptions.ToString());
                    break;
                default:
                    MessageEngine.Global.Trace(Severity.Error, "Unknown Destination Load Type of {0}", destination.AccessMode);
                    break;
            }
        }
    }
}