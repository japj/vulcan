using System;
using System.Collections.Generic;
using System.Text;

using DTS = Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

using Ssis2008Emitter.Utility;
using Ssis2008Emitter.IR.DataFlow;
using Ssis2008Emitter.Emitters.Task;

namespace Ssis2008Emitter.Emitters.DataFlow
{
    [PhysicalIRMapping(typeof(OLEDBCommand))]
    public class SsisOleDbCommandComponent : SsisComponent, ISSISEmitter
    {
        public override string ClassID
        {
            get { return "DTSTransform.OLEDBCommand"; }
        }

        public SsisOleDbCommandComponent(Transformation t, SSISEmitterContext context) : base(t, context)
        {
            InitializeConnection(((OLEDBCommand)_transformation).Connection);
            SetOutputName(_transformation.OutputPath);
            SetupComponentProperties();
        }

        public SSISEmitterContext Emit()
        {
            _dataFlowTask.ChainComponent(this, _transformation.InputPath);
            this.AutoMap();
            return _context;
        }

        private void SetupComponentProperties()
        {
            _instance.SetComponentProperty("SqlCommand", ((OLEDBCommand)_transformation).Command);
        }

        public void AutoMap()
        {
            IDTSVirtualInput100 cvi = _component.InputCollection[0].GetVirtualInput();

            Dictionary<string, string> virtualInputDictionary = new Dictionary<string, string>();
            foreach (IDTSVirtualInputColumn100 vc in cvi.VirtualInputColumnCollection)
            {
                virtualInputDictionary["@" + vc.Name.ToUpperInvariant()] = vc.Name;
            }

            foreach (IDTSExternalMetadataColumn100 extCol in _component.InputCollection[0].ExternalMetadataColumnCollection)
            {
                if (virtualInputDictionary.ContainsKey(extCol.Name.ToUpperInvariant()))
                {
                    Map(virtualInputDictionary[extCol.Name.ToUpperInvariant()], extCol.Name, false);
                }
            }

            foreach (Mapping mapping in ((OLEDBCommand)_transformation).Mappings)
            {
                Map(mapping.Source, mapping.Destination == null ? "@" + mapping.Source : mapping.Destination, false);
            }
        }

        private void Map(string source, string destination, bool unMap)
        {
            SafeMapInputToExternalMetadataColumn(source, destination, unMap);
        }
    }
}
