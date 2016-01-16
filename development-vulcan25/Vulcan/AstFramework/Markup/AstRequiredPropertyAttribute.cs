using System;

namespace AstFramework.Markup
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AstRequiredPropertyAttribute : Attribute
    {
    }
}