using System.Collections.Generic;
using Vulcan.Utility.Graph;
using VulcanEngine.IR.Ast.Transformation;

namespace VulcanEngine.AstEngine
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Name follows Varigence Graph naming pattern")]
    public class TransformationGraph : Graph<AstTransformationNode>
    {
        public TransformationGraph(IEnumerable<AstTransformationNode> transformations)
        {
            PermitDuplicateNodeItems = false;
            ErrorOnDuplicateNodeAddAttempt = false;

            AstTransformationNode previousTransformation = null;

            foreach (var transformation in transformations)
            {
                var multipleIn = transformation as AstMultipleInTransformationNode;
                var singleIn = transformation as AstSingleInTransformationNode;

                if (multipleIn != null)
                {
                    foreach (var inputPath in multipleIn.InputPaths)
                    {
                        AstDataflowOutputPathNode outputPath = inputPath.OutputPath;
                        AddEdge(outputPath.Transformation, multipleIn, outputPath.Name, outputPath, inputPath);
                    }
                }
                else if (singleIn != null && singleIn.InputPath != null)
                {
                    AstDataflowMappedInputPathNode inputPath = singleIn.InputPath;
                    AstDataflowOutputPathNode outputPath = inputPath.OutputPath;
                    AddEdge(outputPath.Transformation, singleIn, outputPath.Name, outputPath, inputPath);
                }
                else if (singleIn != null && previousTransformation != null)
                {
                    AstDataflowOutputPathNode outputPath = previousTransformation.PreferredOutputPath;
                    AddEdge(previousTransformation, singleIn, outputPath.Name, outputPath, null);
                }
                else
                {
                    AddNode(transformation);
                }

                previousTransformation = transformation;
            }
        }
    }
}
