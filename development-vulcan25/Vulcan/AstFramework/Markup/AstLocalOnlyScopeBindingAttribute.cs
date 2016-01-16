using System;

namespace AstFramework.Markup
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AstLocalOnlyScopeBindingAttribute : Attribute
    {
    }
}