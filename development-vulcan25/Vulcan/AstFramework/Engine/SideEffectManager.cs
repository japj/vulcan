namespace AstFramework.Engine
{
    public enum AstSideEffectMode
    {
        NoSideEffects,
        ConsistencySideEffects,
    }

    public static class SideEffectManager
    {
        public static AstSideEffectMode SideEffectMode { get; set; }
    }
}
