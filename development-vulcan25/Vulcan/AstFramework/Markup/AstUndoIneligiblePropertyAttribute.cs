using System;

namespace VulcanEngine.AstFramework
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AstUndoIneligiblePropertyAttribute : Attribute
    {
    }
}