namespace Ssis2008Emitter.IR.Tasks.Transformations
{
    public class Binding
    {
        public object ParentTransformName { get; private set; }

        public object ParentOutputName { get; private set; }

        public object TargetInputName { get; private set; }

        public Binding(object transformName, object parentTransformName, object parentOutputName, object targetInputName)
        {
            if (transformName.Equals(parentTransformName))
            {
                VulcanEngine.Common.MessageEngine.Trace(
                    AstFramework.Severity.Error, 
                    "LS003:Cyclical mapping detected. Transformation \"{0}\" is using itself as an input.", 
                    transformName);
            }

            ParentTransformName = parentTransformName;
            ParentOutputName = parentOutputName;
            TargetInputName = targetInputName;
        }
    }
}
