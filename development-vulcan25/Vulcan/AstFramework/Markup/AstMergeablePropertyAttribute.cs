using System;

namespace AstFramework.Markup
{
    public enum MergeablePropertyType
    {
        Literal,
        Reference,
        Definition,
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AstMergeablePropertyAttribute : Attribute
    {
        public MergeablePropertyType MergeablePropertyType { get; private set; }

        public AstMergeablePropertyAttribute(MergeablePropertyType mergeablePropertyType)
        {
            MergeablePropertyType = mergeablePropertyType;
        }

        public bool IsReference
        {
            get { return MergeablePropertyType == MergeablePropertyType.Reference; }
        }

        public bool IsDefinition
        {
            get { return MergeablePropertyType == MergeablePropertyType.Definition; }
        }

        public bool IsLiteral
        {
            get { return MergeablePropertyType == MergeablePropertyType.Literal; }
        }
    }
}