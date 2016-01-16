using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using AstFramework;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.Phases.Lowering.Framework;
using VulcanEngine.Common;
using VulcanEngine.IR.Ast.Connection;
using VulcanEngine.IR.Ast.Table;
using VulcanEngine.IR.Ast.Transformation;

namespace Ssis2008Emitter.IR.Tasks.Transformations
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public abstract class Transformation : PhysicalObject
    {
        public IDTSComponentMetaData100 Component { get; private set; }

        public CManagedComponentWrapper Instance { get; private set; }

        public bool ValidateExternalMetadata { get; private set; }

        public Collection<Binding> BindingList { get; private set; }

        public DataflowTask Dataflow { get; private set; }

        private AstTransformationNode _astTransformationNode;

        protected Transformation(LoweringContext context, AstTransformationNode astTransformationNode)
            : base(astTransformationNode.Name)
        {
            var dflc = context as DataflowLoweringContext;
            Dataflow = dflc.Dataflow;
            ValidateExternalMetadata = astTransformationNode.ValidateExternalMetadata;
            BindingList = new Collection<Binding>();
            _astTransformationNode = astTransformationNode;
        }

        public abstract string Moniker { get; }

        protected virtual void RegisterInputBinding(AstSingleInTransformationNode transformationNode)
        {
            if (transformationNode != null && transformationNode.InputPath != null)
            {
                BindingList.Add(new MappedBinding(transformationNode.Name, transformationNode.InputPath.OutputPath.Transformation.Name, transformationNode.InputPath.OutputPath.SsisName, 0, transformationNode.InputPath.Mappings));
            }
        }

        protected virtual void ProcessBindings(SsisEmitterContext context)
        {
            if (BindingList.Count > 0)
            {
                foreach (Binding b in BindingList)
                {
                    ProcessBinding(context, b);
                }
            }
            else
            {
                ProcessAutoLexicalBinding(context);
            }
        }

        protected virtual IDTSPath100 ProcessBinding(SsisEmitterContext context, Binding binding)
        {
            if (binding != null)
            {
                try
                {
                    IDTSPath100 path = Dataflow.MainPipe.PathCollection.New();
                    object index = binding.ParentOutputName ?? 0;
                    path.AttachPathAndPropagateNotifications(
                        Dataflow.MainPipe.ComponentMetaDataCollection[binding.ParentTransformName].OutputCollection[index],
                        Component.InputCollection[binding.TargetInputName]);

                    // Shove in annotation for the path reference! :)
                    context.Package.DtsPackage.ExtendedProperties.Add(Dataflow.DtsTaskHost.ID + "-" + path.ID, "dts-designer-1.0", Properties.Resources.DTSDesignerPathAnnotation);

                    ProcessBindingMappings(context, binding as MappedBinding, path);

                    return path;
                }
                catch (System.Runtime.InteropServices.COMException e)
                {
                    MessageEngine.Trace(_astTransformationNode, Severity.Error, "V0210", GetScrubbedErrorDescription(e.ErrorCode));
                    throw;
                }
            }
            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Generic exception is OK in this case - want to catch any exceptions")]
        protected virtual IDTSPath100 ProcessAutoLexicalBinding(SsisEmitterContext context)
        {
            // TODO: Add handling for the exception below
            try
            {
                int lastObject = Dataflow.MainPipe.ComponentMetaDataCollection.GetObjectIndexByID(Dataflow.MainPipe.ComponentMetaDataCollection[Name].ID) - 1;

                if (lastObject >= 0)
                {
                    IDTSPath100 path = Dataflow.MainPipe.PathCollection.New();
                    path.AttachPathAndPropagateNotifications(Dataflow.MainPipe.ComponentMetaDataCollection[lastObject].OutputCollection[0], Component.InputCollection[0]);

                    // Shove in annotation for the path reference! :)
                    context.Package.DtsPackage.ExtendedProperties.Add(Dataflow.DtsTaskHost.ID + "-" + path.ID, "dts-designer-1.0", Properties.Resources.DTSDesignerPathAnnotation);

                    return path;
                }
            }
            catch (Exception e)
            {
                MessageEngine.Trace(this._astTransformationNode, Severity.Error, "TF101", "Error binding AST Data Flow Transformation. This is probably due to having two Transformations with the same name. Please contact support.\n{0}", e.ToString());
            }

            return null;
        }

        protected virtual void ProcessBindingMappings(SsisEmitterContext context, MappedBinding mappedBinding, IDTSPath100 path)
        {
            if (mappedBinding != null && path != null)
            {
                foreach (AstDataflowColumnMappingNode map in mappedBinding.Mappings)
                {
                    int lineageId;
                    var matchedOutput = TransformationUtility.FindOutputColumnByName(map.SourceName, path.StartPoint, true);

                    if (matchedOutput == null)
                    {
                        var matchedInput = TransformationUtility.FindVirtualInputColumnByName(map.SourceName, path.EndPoint, true);
                        if (matchedInput == null)
                        {
                            // Message.Error
                            throw new InvalidOperationException();
                        }

                        lineageId = matchedInput.LineageID;
                    }
                    else
                    {
                        lineageId = matchedOutput.LineageID;
                    }

                    IDTSInputColumn100 ic = TransformationUtility.FindInputColumnByName(map.SourceName, path.EndPoint, true);
                    if (ic == null)
                    {
                        ic = path.EndPoint.InputColumnCollection.New();
                        ic.LineageID = lineageId;
                    }

                    if (String.IsNullOrEmpty(map.TargetName))
                    {
                        // TODO: What is the right way to destroy a column in dataflow?
                        ic.UsageType = DTSUsageType.UT_IGNORED;
                    }
                    else
                    {
                        ic.Name = map.TargetName;
                    }
                }
            }
        }

        protected virtual void Validate()
        {
            DTSValidationStatus dtsStatus = Instance.Validate();

            // TODO: Error messages
            if (dtsStatus != DTSValidationStatus.VS_ISVALID)
            {
                if (dtsStatus == DTSValidationStatus.VS_NEEDSNEWMETADATA)
                {
                    Instance.ReinitializeMetaData();
                }
            }
        }

        public override void Initialize(SsisEmitterContext context)
        {
            Component = Dataflow.NewComponentMetadata();
            Component.ComponentClassID = Moniker;

            Instance = Component.Instantiate();
            Instance.ProvideComponentProperties();

            Component.Name = Name;
            Component.ValidateExternalMetadata = ValidateExternalMetadata;
        }

        public virtual void SetOutputPathName(object outputPathIndex, string outputPathName)
        {
            if (!String.IsNullOrEmpty(outputPathName) && outputPathIndex != null)
            {
                Component.OutputCollection[outputPathIndex].Name = outputPathName;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Need general exception handling.  No risk of bad state.")]
        public virtual IDTSInputColumn100 SetInputColumnUsage(object inputPathIndex, object columnIndex, DTSUsageType usageType, bool forceOverwrite)
        {
            try
            {
                IDTSVirtualInput100 vi = Component.InputCollection[inputPathIndex].GetVirtualInput();
                IDTSVirtualInputColumn100 vcol = vi.VirtualInputColumnCollection[columnIndex];
                return SetInputColumnUsage(inputPathIndex, columnIndex, vi, vcol, usageType, forceOverwrite);
            }
            catch (Exception)
            {
                MessageEngine.Trace(_astTransformationNode, Severity.Error, "V0110", "Could not locate input column {0}", columnIndex);
            }

            return null;
        }

        public virtual IDTSInputColumn100 SetInputColumnUsage(object inputPathIndex, object columnIndex, IDTSVirtualInput100 virtualInput, IDTSVirtualInputColumn100 virtualInputColumn, DTSUsageType usageType, bool forceOverwrite)
        {
            if ((virtualInputColumn.UsageType == usageType || virtualInputColumn.UsageType == DTSUsageType.UT_READWRITE) && !forceOverwrite)
            {
            }
            else
            {
                Instance.SetUsageType(Component.InputCollection[inputPathIndex].ID, virtualInput, virtualInputColumn.LineageID, usageType);
            }

            return TransformationUtility.FindInputColumnByName(columnIndex.ToString(), Component.InputCollection[inputPathIndex], true);
        }

        public virtual IList<string> GetVirtualInputColumns(object inputPathIndex)
        {
            var columnsToMap = new List<string>();
            foreach (IDTSVirtualInputColumn100 input in Component.InputCollection[0].GetVirtualInput().VirtualInputColumnCollection)
            {
                columnsToMap.Add(input.Name);
            }

            return columnsToMap;
        }

        public virtual void Flush()
        {
            bool connectionErrorFound = false;
            try
            {
                Instance.AcquireConnections(null);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                if (e.ErrorCode == -1071611876)
                {
                    connectionErrorFound = true;
                    var connection = _astTransformationNode.FirstChildOfType<AstConnectionNode>();
                    MessageEngine.Trace(_astTransformationNode, Severity.Error, "V0203", "Could not connect to {0} for validation.", connection.Name);
                }
                else
                {
                    MessageEngine.Trace(_astTransformationNode, Severity.Error, null, GetScrubbedErrorDescription(e.ErrorCode));
                }
            }
            catch (System.Runtime.InteropServices.SEHException seh)
            {
                MessageEngine.Trace(_astTransformationNode, Severity.Error, null, "Internal SSIS Object Corruption when calling AcquireConnections HRESULT: {0}\n{1}", seh.ErrorCode.ToString("X"),seh.ToString());
            }

            try
            {
                Instance.ReinitializeMetaData();
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                if (e.ErrorCode == -1071636248)
                {
                    var table = _astTransformationNode.FirstChildOfType<AstTableNode>();
                    string refName = table != null ? table.Name : "table";
                    MessageEngine.Trace(_astTransformationNode, Severity.Error, "V0203", "Failed to open {0} for validation.", refName);
                }
                else if (!connectionErrorFound || e.ErrorCode != -1071636446)
                {
                    MessageEngine.Trace(_astTransformationNode, Severity.Error, "V0210", GetScrubbedErrorDescription(e.ErrorCode));
                }
            }

            Instance.ReleaseConnections();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Need general exception handling.  No risk of bad state.")]
        private string GetScrubbedErrorDescription(int errorCode)
        {
            try
            {
                string temp = Component.GetErrorDescription(errorCode).Replace("%1", "{0}").Replace("%2", "{1:X}");
                return String.Format(CultureInfo.InvariantCulture, temp, Name, errorCode);
            }
            catch (Exception)
            {
                return Component.GetErrorDescription(errorCode);
            }
        }
    }
}
