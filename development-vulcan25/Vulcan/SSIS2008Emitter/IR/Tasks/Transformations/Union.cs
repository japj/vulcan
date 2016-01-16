using AstFramework;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.Phases.Lowering.Framework;
using VulcanEngine.Common;
using VulcanEngine.IR.Ast;
using VulcanEngine.IR.Ast.Transformation;

namespace Ssis2008Emitter.IR.Tasks.Transformations
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public class Union : SingleOutTransformation
    {
        private readonly AstUnionAllNode _astUnionAllNode;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Generic list is appropriate")]
        public static void CreateAndRegister(AstNode astNode, LoweringContext context)
        {
            context.ParentObject.Children.Add(new Union(context, astNode));
        }

        public Union(LoweringContext context, AstNode astNode)
            : base(context, astNode as AstTransformationNode)
        {
            _astUnionAllNode = astNode as AstUnionAllNode;

            if (_astUnionAllNode != null)
            {
                int i = 0;
                foreach (AstDataflowMappedInputPathNode ip in _astUnionAllNode.InputPaths)
                {
                    BindingList.Add(new MappedBinding(_astUnionAllNode.Name, ip.OutputPath.Transformation.Name, ip.OutputPath.SsisName, i, ip.Mappings));
                    i++;
                }
            }
        }

        public override string Moniker
        {
            get { return "DTSTransform.UnionAll"; }
        }

        public override void Initialize(SsisEmitterContext context)
        {
            base.Initialize(context);
            Flush();
        }

        public override void Emit(SsisEmitterContext context)
        {
            ProcessBindings(context);
        }

        protected override void ProcessBindingMappings(SsisEmitterContext context, MappedBinding mappedBinding, IDTSPath100 path)
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
                            MessageEngine.Trace(_astUnionAllNode, Severity.Error, "V0145", "Could not find input column {0} for transformation {1}", map.SourceName, _astUnionAllNode.Name);
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
                        ic.Name = map.SourceName;
                        ic.LineageID = lineageId;
                    }

                    IDTSOutputColumn100 oc = TransformationUtility.FindOutputColumnByName(map.TargetName, OutputPath, true);
                    if (oc == null)
                    {
                        oc = OutputPath.OutputColumnCollection.New();
                        oc.Name = map.TargetName;
                        oc.SetDataTypeProperties(ic.DataType, ic.Length, ic.Precision, ic.Scale, ic.CodePage);
                    }

                    IDTSCustomProperty100 cp = TransformationUtility.FindCustomPropertyByName("OutputColumnLineageID", ic.CustomPropertyCollection, true);
                    if (cp == null)
                    {
                        cp = ic.CustomPropertyCollection.New();
                        cp.Name = "OutputColumnLineageID";
                    }

                    cp.Value = oc.LineageID;
                }
            }
        }
    }
}

