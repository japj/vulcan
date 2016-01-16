using System;
using System.Collections.Generic;
using AstFramework;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.Framework.Connections;
using Ssis2008Emitter.Phases.Lowering.Framework;
using VulcanEngine.Common;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Transformation;
using DTS = Microsoft.SqlServer.Dts.Runtime;

namespace Ssis2008Emitter.IR.Tasks.Transformations
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public class OleDBCommand : Transformation
    {
        private readonly AstOleDBCommandNode _astOleDBCommandNode;
        private OleDBConnection _oleDBConnection;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        public static void CreateAndRegister(AstNode astNode, LoweringContext context)
        {
            var astOleDBCommandNode = astNode as AstOleDBCommandNode;
            if (astOleDBCommandNode != null)
            {
                var oleDBCommand = new OleDBCommand(context, astNode) { _oleDBConnection = new OleDBConnection(astOleDBCommandNode.Connection) };
                context.ParentObject.Children.Add(oleDBCommand._oleDBConnection);
                context.ParentObject.Children.Add(oleDBCommand);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Required for Emitter pattern.")]
        public OleDBCommand(LoweringContext context, AstNode astNode) : base(context, astNode as AstTransformationNode)
        {
            _astOleDBCommandNode = astNode as AstOleDBCommandNode;
            RegisterInputBinding(_astOleDBCommandNode);
        }

        public override string Moniker
        {
            get { return "DTSTransform.OleDBCommand"; }
        }

        public override void Initialize(SsisEmitterContext context)
        {
            base.Initialize(context);
            TransformationUtility.RegisterOleDBConnection(context, _oleDBConnection, Component);

            Instance.SetComponentProperty("SqlCommand", _astOleDBCommandNode.Query.Body);
            ProcessBindings(context);
            Flush();
        }

        public override void Emit(SsisEmitterContext context)
        {
            ProcessMappings();
        }

        protected void ProcessMappings()
        {
            IDTSVirtualInput100 cvi = Component.InputCollection[0].GetVirtualInput();

            var virtualInputDictionary = new Dictionary<string, IDTSVirtualInputColumn100>();
            foreach (IDTSVirtualInputColumn100 vc in cvi.VirtualInputColumnCollection)
            {
                virtualInputDictionary["@" + vc.Name.ToUpperInvariant()] = vc;
            }

            // Automatically map columns
            foreach (IDTSExternalMetadataColumn100 extCol in Component.InputCollection[0].ExternalMetadataColumnCollection)
            {
                if (virtualInputDictionary.ContainsKey(extCol.Name.ToUpperInvariant()))
                {
                    IDTSVirtualInputColumn100 vc = virtualInputDictionary[extCol.Name.ToUpperInvariant()];
                    Instance.SetUsageType(Component.InputCollection[0].ID, cvi, vc.LineageID, DTSUsageType.UT_READONLY);
                    Component.InputCollection[0].InputColumnCollection[vc.Name].ExternalMetadataColumnID = extCol.ID;
                }
            }

            // Map any overrides
            foreach (AstDataflowColumnMappingNode mapping in _astOleDBCommandNode.Query.Mappings)
            {
                if (String.IsNullOrEmpty(mapping.TargetName))
                {
                    SetInputColumnUsage(0, mapping.SourceName, DTSUsageType.UT_IGNORED, true);
                }
                else
                {
                    IDTSExternalMetadataColumn100 ecol = TransformationUtility.FindExternalColumnByName(mapping.TargetName, Component.InputCollection[0].ExternalMetadataColumnCollection, true);

                    // Unmap anything else that maps to this external metadata column)
                    foreach (IDTSInputColumn100 inputColumn in Component.InputCollection[0].InputColumnCollection)
                    {
                        if (inputColumn.ExternalMetadataColumnID == ecol.ID)
                        {
                            MessageEngine.Trace(_astOleDBCommandNode, Severity.Debug, "V0401", "{0}: {1} Unmapping Input {2}", GetType(), Component.Name, inputColumn.Name);
                            SetInputColumnUsage(0, inputColumn.Name, DTSUsageType.UT_IGNORED, true);
                            break;
                        }
                    }

                    IDTSInputColumn100 icol = SetInputColumnUsage(0, mapping.SourceName, DTSUsageType.UT_READONLY, false);
                    if (ecol != null)
                    {
                        if (icol != null)
                        {
                            icol.ExternalMetadataColumnID = ecol.ID;
                        }
                        else
                        {
                            MessageEngine.Trace(_astOleDBCommandNode, Severity.Error, "V0105", "Could not find source column {0}", mapping.SourceName);
                        }
                    }
                    else
                    {
                        MessageEngine.Trace(_astOleDBCommandNode, Severity.Error, "V0106", "Could not find destination column {0}", mapping.TargetName);
                    }
                }
            }
        }
    }
}

