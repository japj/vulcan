using System;

namespace AstFramework.Markup
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class AstDoNotEmitXmlSchemaTypeAttribute : System.Attribute
    {
    }
}