using Vulcan.Utility.Collections;
using VulcanEngine.IR.Ast.Transformation;

namespace Ssis2008Emitter.IR.Tasks.Transformations
{
    public class MappedBinding : Binding
    {
        public VulcanCollection<AstDataflowColumnMappingNode> Mappings { get; private set; }

        public MappedBinding(
            object transformName,
            object parentTransformName,
            object parentOutputName,
            object targetInputName,
            VulcanCollection<AstDataflowColumnMappingNode> mappingList)
            : base(transformName, parentTransformName, parentOutputName, targetInputName)
        {
            Mappings = mappingList;
        }
    }
}
