using System;
using System.ComponentModel;

namespace AstFramework.Markup
{
    public enum ChildType
    {
        Unknown,
        Attribute,
        ChildDefinition,
        ListChildDefinition,
        Self,
    }

    [TypeConverter(typeof(AttributeUseConverter))]
    public enum ChildUse
    {
        Unknown,
        Optional,
        Required,
    }

    ////[TypeConverter(typeof(AttributeUseConverter))]
    public enum AttributeUse
    {
        Unknown,
        Optional,
        Required,
        Prohibited,
        None,
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class AstXNameBindingAttribute : Attribute
    {
        public string Name { get; private set; }

        public ChildType ChildType { get; private set; }

        public string DefaultValue { get; set; }

        public Type SubtypeOverride { get; set; }

        public bool IsRequired { get; set; }

        #region Predicates
        public bool IsAttribute
        {
            get { return ChildType == ChildType.Attribute; }
        }

        public bool IsChildElement
        {
            get { return ChildType == ChildType.ChildDefinition; }
        }

        public bool IsChildListElement
        {
            get { return ChildType == ChildType.ListChildDefinition; }
        }

        public bool IsSelf
        {
            get { return ChildType == ChildType.Self; }
        }

        public bool IsDefinition
        {
            get { return ChildType == ChildType.ChildDefinition || ChildType == ChildType.ListChildDefinition; }
        }

        public bool IsReference
        {
            get { return ChildType == ChildType.Attribute; }
        }

        public bool HasDefaultValue
        {
            get { return !String.IsNullOrEmpty(DefaultValue); }
        }

        public bool HasSubtypeOverride
        {
            get { return SubtypeOverride != null; }
        }
        #endregion  //Predicates

        #region Constructors
        ////public AstXNameBindingAttribute() 
        ////{
            //// Can we grab default name and type off of property that this attribute is applied to?
        ////}

        public AstXNameBindingAttribute(string name, ChildType childType)
        {
            Name = name;
            ChildType = childType;
        }
        #endregion  // Constructors

        public override string ToString()
        {
            return Name;
        }
    }
}