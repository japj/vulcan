namespace Ssis2008Emitter.IR.Common
{
    public interface ISsisEmitter  
    {
        void Initialize(SsisEmitterContext context);
        
        void Emit(SsisEmitterContext context);
    }
}
