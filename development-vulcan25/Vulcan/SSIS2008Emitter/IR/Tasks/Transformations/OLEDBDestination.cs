using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class OleDBDestination : Transformation
    {
        private readonly AstDestinationNode _astDestination;

        private OleDBConnection _oleDBConnection;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        public static void CreateAndRegister(AstNode astNode, LoweringContext context)
        {
            var astDestination = astNode as AstDestinationNode;
            if (astDestination != null)
            {
                var oleDBDestination = new OleDBDestination(context, astNode) { _oleDBConnection = new OleDBConnection(astDestination.Table.Connection) };
                context.ParentObject.Children.Add(oleDBDestination._oleDBConnection);
                context.ParentObject.Children.Add(oleDBDestination);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Required for Emitter pattern.")]
        public OleDBDestination(LoweringContext context, AstNode astNode) : base(context, astNode as AstTransformationNode)
        {
            _astDestination = astNode as AstDestinationNode;
            RegisterInputBinding(_astDestination);
        }

        public override string Moniker
        {
            get { return "DTSAdapter.OleDBDestination"; }
        }

        public override void Initialize(SsisEmitterContext context)
        {
            base.Initialize(context);
            TransformationUtility.RegisterOleDBConnection(context, _oleDBConnection, Component);

            switch (_astDestination.AccessMode)
            {
                case DestinationAccessModeFacet.Table:
                    Instance.SetComponentProperty("AccessMode", 0);
                    Instance.SetComponentProperty("OpenRowset", _astDestination.Table.SchemaQualifiedName);
                    break;
                case DestinationAccessModeFacet.TableFastLoad:
                    Instance.SetComponentProperty("AccessMode", 3);
                    Instance.SetComponentProperty("OpenRowset", _astDestination.Table.SchemaQualifiedName);

                    Instance.SetComponentProperty("FastLoadKeepIdentity", _astDestination.KeepIdentity);
                    Instance.SetComponentProperty("FastLoadKeepNulls", _astDestination.KeepNulls);
                    Instance.SetComponentProperty("FastLoadMaxInsertCommitSize", _astDestination.MaximumInsertCommitSize);

                    var fastLoadOptions = new StringBuilder();
                    if (_astDestination.TableLock)
                    {
                        fastLoadOptions.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "TABLOCK,");
                    }

                    if (_astDestination.CheckConstraints)
                    {
                        fastLoadOptions.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "CHECK_CONSTRAINTS,");
                    }

                    if (_astDestination.RowsPerBatch > 0)
                    {
                        fastLoadOptions.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "ROWS_PER_BATCH = {0}", _astDestination.RowsPerBatch);
                    }

                    fastLoadOptions = fastLoadOptions.Replace(",", String.Empty, fastLoadOptions.Length - 5, 5);

                    Instance.SetComponentProperty("FastLoadOptions", fastLoadOptions.ToString());
                    break;
                default:
                    MessageEngine.Trace(Severity.Error, new NotImplementedException(), "Unknown Destination Load Type of {0}", _astDestination.AccessMode);
                    break;
            }

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
                virtualInputDictionary[vc.Name.ToUpperInvariant()] = vc;
            }

            // Automatically map columns
            foreach (IDTSExternalMetadataColumn100 extCol in Component.InputCollection[0].ExternalMetadataColumnCollection)
            {
                string extColName = extCol.Name.ToUpperInvariant();
                if (!_astDestination.Table.ReadOnlyColumns.Any(item => item.Name.ToUpperInvariant() == extColName) && virtualInputDictionary.ContainsKey(extCol.Name.ToUpperInvariant()))
                {
                    IDTSVirtualInputColumn100 vc = virtualInputDictionary[extColName];
                    Instance.SetUsageType(Component.InputCollection[0].ID, cvi, vc.LineageID, DTSUsageType.UT_READONLY);
                    Component.InputCollection[0].InputColumnCollection[vc.Name].ExternalMetadataColumnID = extCol.ID;
                }
            }

            // Map any overrides
            foreach (AstDataflowColumnMappingNode mapping in _astDestination.Mappings)
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
                            MessageEngine.Trace(_astDestination, Severity.Debug, "V0402", "{0}: {1} Unmapping Input {2}", GetType(), Component.Name, inputColumn.Name);
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
                            MessageEngine.Trace(_astDestination, Severity.Error, "V0107", "Could not find source column {0}", mapping.SourceName);
                        }
                    }
                    else
                    {
                        MessageEngine.Trace(_astDestination, Severity.Error, "V0108", "Could not find destination column {0}", mapping.TargetName);
                    }
                }
            }
        }
    }
}

